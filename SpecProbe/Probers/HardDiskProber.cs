//
// SpecProbe  Copyright (C) 2023-2024  Aptivi
//
// This file is part of SpecProbe
//
// SpecProbe is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// SpecProbe is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using SpecProbe.Parts.Types;
using SpecProbe.Parts.Types.HardDisk;
using SpecProbe.Probers.Platform;
using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SpecProbe.Probers
{
    internal static class HardDiskProber
    {
        public static HardDiskPart[] Probe(out Exception[] errors)
        {
            if (PlatformHelper.IsOnWindows())
                return ProbeWindows(out errors);
            else if (PlatformHelper.IsOnMacOS())
                return ProbeMacOS(out errors);
            else
                return ProbeLinux(out errors);
        }

        public static HardDiskPart[] ProbeLinux(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            List<HardDiskPart> diskParts = [];
            List<HardDiskPart.PartitionPart> partitions = [];

            // Get the blocks
            try
            {
                string blockListFolder = "/sys/block";
                string[] blockFolders = Directory.GetDirectories(blockListFolder).Where((dir) => !dir.Contains("/sys/block/loop")).ToArray();
                for (int i = 0; i < blockFolders.Length; i++)
                {
                    string blockFolder = blockFolders[i];

                    // Verify that the block is non-removable (HDDs)
                    string blockStateFile = $"{blockFolder}/removable";
                    string blockStateStr = File.ReadAllLines(blockStateFile)[0];
                    int blockState = int.Parse(blockStateStr);
                    if (blockState == 0)
                    {
                        // Now, get the size
                        string blockSizeFile = $"{blockFolder}/size";
                        string blockSizeStr = File.ReadAllLines(blockSizeFile)[0];
                        ulong blockSize = ulong.Parse(blockSizeStr);
                        ulong actualSize = blockSize * 512;

                        // Get the partitions
                        string devName = blockFolder.Substring(blockFolder.LastIndexOf("/") + 1);
                        string devPartFolderInitial = $"{blockFolder}/{devName}";

                        // Check if the dev name ends with a number (such as nvme0n1p1, mmcblk0p1, etc.)
                        bool appendPartitionChar = int.TryParse($"{devName[devName.Length - 1]}", out int devNum);
                        int partNum = 1;
                        PartitionTableType ptType = PartitionTableType.Unknown;
                        while (true)
                        {
                            string partPath = appendPartitionChar ?
                                $"{devPartFolderInitial}p{partNum}" :
                                $"{devPartFolderInitial}{partNum}";
                            string partDevPath = appendPartitionChar ?
                                $"/dev/{devName}p{partNum}" :
                                $"/dev/{devName}{partNum}";
                            if (!Directory.Exists(partPath))
                            {
                                if (partNum <= 4)
                                {
                                    partNum++;
                                    continue;
                                }
                                break;
                            }

                            // Get the actual size
                            string partSizeFile = $"{partPath}/size";
                            string partStartFile = $"{partPath}/start";
                            string partSizeStr = File.ReadAllLines(partSizeFile)[0];
                            string partStartStr = File.ReadAllLines(partStartFile)[0];
                            long partSize = long.Parse(partSizeStr);
                            long partStart = long.Parse(partStartStr);
                            long partActualSize = partSize * 512;
                            long partActualStart = partStart * 512;

                            // Get the partition type
                            string options = $"-f --output PARTTYPE -n {partDevPath}";
                            string optionsPt = $"-f --output PTTYPE -n {partDevPath}";
                            string output = PlatformHelper.ExecuteProcessToString("/usr/bin/lsblk", options).Trim(['\r', '\n']);
                            string outputPt = PlatformHelper.ExecuteProcessToString("/usr/bin/lsblk", optionsPt).Trim(['\r', '\n']);
                            var type = PartitionType.Unknown;
                            if (Guid.TryParse(output, out Guid gptType))
                                type = DetermineType(gptType);
                            else if (int.TryParse(output.Substring(2), NumberStyles.HexNumber, null, out int mbrType))
                                type = (PartitionType)mbrType;
                            ptType = outputPt switch
                            {
                                "dos" => PartitionTableType.MBR,
                                "gpt" => PartitionTableType.GPT,
                                "mac" => PartitionTableType.Apple,
                                "" =>    PartitionTableType.Unknown,
                                _ =>     PartitionTableType.Other,
                            };
                            partitions.Add(new HardDiskPart.PartitionPart
                            {
                                PartitionNumber = partNum,
                                PartitionSize = partActualSize,
                                PartitionOffset = partActualStart,
                                PartitionType = type,
                                PartitionBootable = type == PartitionType.EFISystem,
                            });
                            partNum++;
                        }

                        // Add disk
                        diskParts.Add(new HardDiskPart
                        {
                            HardDiskSize = actualSize,
                            HardDiskNumber = devNum,
                            Partitions = [.. partitions],
                            PartitionTableType = ptType,
                        });
                        partitions.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            // Finally, return an array containing information
            errors = [.. exceptions];
            return diskParts.ToArray();
        }

        public static HardDiskPart[] ProbeMacOS(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            List<HardDiskPart> diskParts = [];
            List<HardDiskPart.PartitionPart> partitions = [];

            // Get the blocks
            try
            {
                List<int> virtuals = [];
                string blockListFolder = "/dev";
                string[] blockFolders = Directory.GetFiles(blockListFolder).Where((dir) => dir.Contains("/dev/disk")).ToArray();
                for (int i = 0; i < blockFolders.Length; i++)
                {
                    string blockFolder = blockFolders[i];

                    // Necessary for diskutil parsing
                    string diskUtilTrue = "Yes";
                    string diskUtilFixed = "Fixed";
                    string blockVirtualTag = "Virtual:";
                    string blockDiskSizeTag = "Disk Size:";
                    string blockVirtualDiskSizeTag = "Volume Used Space:";
                    string blockRemovableMediaTag = "Removable Media:";
                    string blockIsWholeTag = "Whole:";
                    string blockDiskTag = "Part of Whole:";
                    string partTypeTag = "Partition Type:";
                    string partOffsetTag = "Partition Offset:";
                    string blockSchemeTag = "Content (IOContent):";

                    // Some variables for the block
                    bool blockVirtual = false;
                    bool blockFixed = true;
                    bool blockIsDisk = true;
                    string reallyDiskId = "";
                    string partType = "";
                    string blockScheme = "";
                    ulong partOffset = 0;
                    ulong actualSize = 0;
                    int diskNum = 1;

                    // Execute "diskutil info" on that block
                    string diskutilOutput = PlatformHelper.ExecuteProcessToString("/usr/sbin/diskutil", $"info {blockFolder}");
                    string[] diskutilOutputLines = diskutilOutput.Replace("\r", "").Split('\n');
                    foreach (string diskutilOutputLine in diskutilOutputLines)
                    {
                        if (!blockFixed)
                            break;
                        string trimmedLine = diskutilOutputLine.Trim();
                        if (trimmedLine.StartsWith(blockVirtualTag))
                        {
                            // Trim the tag to get the value.
                            blockVirtual = trimmedLine.Substring(blockVirtualTag.Length).Trim() == diskUtilTrue;
                        }
                        if (trimmedLine.StartsWith(blockRemovableMediaTag))
                        {
                            // Trim the tag to get the value.
                            blockFixed = trimmedLine.Substring(blockRemovableMediaTag.Length).Trim() == diskUtilFixed;
                        }
                        if (trimmedLine.StartsWith(blockIsWholeTag))
                        {
                            // Trim the tag to get the value.
                            blockIsDisk = trimmedLine.Substring(blockIsWholeTag.Length).Trim() == diskUtilTrue;
                        }
                        if (trimmedLine.StartsWith(blockSchemeTag))
                        {
                            // Trim the tag to get the value.
                            blockScheme = trimmedLine.Substring(blockSchemeTag.Length).Trim();
                        }
                        if (trimmedLine.StartsWith(partTypeTag))
                        {
                            // Trim the tag to get the value.
                            partType = trimmedLine.Substring(partTypeTag.Length).Trim();
                        }
                        if (trimmedLine.StartsWith(partOffsetTag))
                        {
                            // Trim the tag to get the value like:
                            //    Partition Offset:          20480 Bytes (40 512-Byte-Device-Blocks)
                            string sizes = trimmedLine.Substring(partOffsetTag.Length).Trim();

                            // We don't want to make the same mistake as we've done in the past for Inxi.NET, so we need to
                            // get the number of bytes from that.
                            sizes = sizes.Substring(0, sizes.IndexOf(' '));
                            partOffset = ulong.Parse(sizes);
                        }
                        if (trimmedLine.StartsWith(partTypeTag))
                        {
                            // Trim the tag to get the value.
                            partType = trimmedLine.Substring(partTypeTag.Length).Trim();
                        }
                        if (trimmedLine.StartsWith(blockDiskTag))
                        {
                            // Trim the tag to get the value.
                            reallyDiskId = trimmedLine.Substring(blockDiskTag.Length).Trim();
                            diskNum = int.Parse(reallyDiskId.Substring("disk".Length)) + 1;
                            if (virtuals.Contains(diskNum))
                                blockVirtual = true;
                        }
                        if (trimmedLine.StartsWith(blockDiskSizeTag) && !blockVirtual)
                        {
                            // Trim the tag to get the value like:
                            //    Disk Size:                 107.4 GB (107374182400 Bytes) (exactly 209715200 512-Byte-Units)
                            string sizes = trimmedLine.Substring(blockDiskSizeTag.Length).Trim();

                            // We don't want to make the same mistake as we've done in the past for Inxi.NET, so we need to
                            // get the number of bytes from that.
                            sizes = sizes.Substring(sizes.IndexOf('(') + 1);
                            sizes = sizes.Substring(0, sizes.IndexOf(' '));
                            actualSize = ulong.Parse(sizes);
                        }
                        if (trimmedLine.StartsWith(blockVirtualDiskSizeTag) && blockVirtual)
                        {
                            // Trim the tag to get the value like:
                            //    Volume Used Space:         2.0 GB (2013110272 Bytes) (exactly 3931856 512-Byte-Units)
                            string sizes = trimmedLine.Substring(blockVirtualDiskSizeTag.Length).Trim();

                            // We don't want to make the same mistake as we've done in the past for Inxi.NET, so we need to
                            // get the number of bytes from that.
                            sizes = sizes.Substring(sizes.IndexOf('(') + 1);
                            sizes = sizes.Substring(0, sizes.IndexOf(' '));
                            actualSize = ulong.Parse(sizes);
                        }
                    }

                    // Don't continue if the drive is not fixed
                    if (!blockFixed)
                        continue;

                    // Get the disk and the partition number
                    int partNum = 0;
                    if (!blockIsDisk)
                    {
                        string part = Path.GetFileName(blockFolder).Substring(reallyDiskId.Length + 1);
                        part = part.Contains("s") ? part.Substring(0, part.IndexOf("s")) : part;
                        partNum = int.Parse(part);
                    }
                    if (blockVirtual && !virtuals.Contains(diskNum))
                        virtuals.Add(diskNum);

                    // Now, either put it to a partition or a disk
                    if (blockIsDisk)
                    {
                        partitions.Clear();
                        diskParts.Add(new HardDiskPart
                        {
                            HardDiskSize = actualSize,
                            HardDiskNumber = diskNum,
                            Partitions = [.. partitions],
                            PartitionTableType =
                                blockScheme == "GUID_partition_scheme" ? PartitionTableType.GPT :
                                PartitionTableType.Unknown,
                        });
                    }
                    else
                    {
                        partitions.Add(new HardDiskPart.PartitionPart
                        {
                            PartitionNumber = partNum,
                            PartitionSize = (long)actualSize,
                            PartitionOffset = (long)partOffset,
                            PartitionType =
                                partType == "EFI" ? PartitionType.EFISystem :
                                partType == "Apple_APFS" ? PartitionType.HFS :
                                PartitionType.Unknown
                        });
                        diskParts[diskNum - 1].Partitions = [.. partitions];
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            // Finally, return an array containing information
            errors = [.. exceptions];
            return diskParts.ToArray();
        }

        public static HardDiskPart[] ProbeWindows(out Exception[] errors)
        {
            List<Exception> exceptions = [];
            List<HardDiskPart> diskParts = [];
            List<HardDiskPart.PartitionPart> partitions = [];

            try
            {
                // First, get the list of physical partitions up to \\.\PHYSICALDRIVE15
                int maxDrives = 15;
                List<string> drives = [];
                for (int drvIdx = 0; drvIdx <= maxDrives; drvIdx++)
                    drives.Add(@$"\\.\PHYSICALDRIVE{drvIdx}");

                // Then, open the file handle to the physical drives for us to be able to filter them for partitions
                foreach (string drive in drives)
                {
                    IntPtr driveHandle = PlatformWindowsInterop.CreateFile(drive, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.System, IntPtr.Zero);
                    if (driveHandle != new IntPtr(-1))
                    {
                        // Get the hard drive number
                        bool result = PlatformWindowsInterop.DeviceIoControl(driveHandle, PlatformWindowsInterop.EIOControlCode.DiskGetDriveGeometry, null, 0, out PlatformWindowsInterop.DISK_GEOMETRY drvGeom, Marshal.SizeOf<PlatformWindowsInterop.DISK_GEOMETRY>(), out _, IntPtr.Zero);
                        if (!result)
                            continue;
                        bool numResult = PlatformWindowsInterop.DeviceIoControl(driveHandle, PlatformWindowsInterop.EIOControlCode.StorageGetDeviceNumber, null, 0, out PlatformWindowsInterop.STORAGE_DEVICE_NUMBER devNumber, Marshal.SizeOf<PlatformWindowsInterop.STORAGE_DEVICE_NUMBER>(), out _, IntPtr.Zero);
                        if (!numResult)
                            continue;
                        ulong hardDiskSize = (ulong)(drvGeom.Cylinders * drvGeom.TracksPerCylinder * drvGeom.SectorsPerTrack * drvGeom.BytesPerSector);
                        int diskNum = devNumber.DeviceNumber;

                        // Check the type
                        if (drvGeom.MediaType == PlatformWindowsInterop.MEDIA_TYPE.FixedMedia)
                        {
                            // Add a hard disk
                            var disk = new HardDiskPart()
                            {
                                HardDiskSize = hardDiskSize,
                                HardDiskNumber = diskNum,
                                Partitions = [],
                            };

                            // Get the partitions
                            var parts = new List<HardDiskPart.PartitionPart>();
                            IntPtr driveLayoutPtr = IntPtr.Zero;
                            int buffSize = 1024;
                            int error;
                            do
                            {
                                // Allocate the drive layout pointer
                                error = 0;
                                driveLayoutPtr = Marshal.AllocHGlobal(buffSize);

                                // Try to get the drive layout
                                bool partResult = PlatformWindowsInterop.DeviceIoControl(driveHandle, PlatformWindowsInterop.EIOControlCode.DiskGetDriveLayoutEx, IntPtr.Zero, 0, driveLayoutPtr, (uint)buffSize, out _, IntPtr.Zero);
                                if (partResult)
                                {
                                    // Get partition table type
                                    PlatformWindowsInterop.DRIVE_LAYOUT_INFORMATION_EX driveLayout = Marshal.PtrToStructure<PlatformWindowsInterop.DRIVE_LAYOUT_INFORMATION_EX>(driveLayoutPtr);
                                    switch (driveLayout.PartitionStyle)
                                    {
                                        case PlatformWindowsInterop.PARTITION_STYLE.PARTITION_STYLE_MBR:
                                            disk.PartitionTableType = PartitionTableType.MBR;
                                            break;
                                        case PlatformWindowsInterop.PARTITION_STYLE.PARTITION_STYLE_GPT:
                                            disk.PartitionTableType = PartitionTableType.GPT;
                                            break;
                                    }

                                    // Get all the partitions
                                    for (uint partIdx = 0; partIdx < driveLayout.PartitionCount; partIdx++)
                                    {
                                        // Make a pointer to a partition info instance
                                        IntPtr ptr = new(driveLayoutPtr.ToInt64() + Marshal.OffsetOf(typeof(PlatformWindowsInterop.DRIVE_LAYOUT_INFORMATION_EX), "PartitionEntry").ToInt64() + partIdx * Marshal.SizeOf(typeof(PlatformWindowsInterop.PARTITION_INFORMATION_EX)));
                                        PlatformWindowsInterop.PARTITION_INFORMATION_EX partInfo = Marshal.PtrToStructure<PlatformWindowsInterop.PARTITION_INFORMATION_EX>(ptr);

                                        // Check to see if this partition is a recognizable MBR/GPT partition
                                        var type = PartitionType.Unknown;
                                        bool isGpt = partInfo.PartitionStyle == PlatformWindowsInterop.PARTITION_STYLE.PARTITION_STYLE_GPT;
                                        if (isGpt || partInfo.Mbr.RecognizedPartition)
                                        {
                                            // Try to get partition type
                                            if (isGpt)
                                            {
                                                var typeGuid = partInfo.Gpt.PartitionType;
                                                type = DetermineType(typeGuid);
                                            }
                                            else
                                            {
                                                switch (partInfo.Mbr.PartitionType)
                                                {
                                                    // Values from https://learn.microsoft.com/en-us/windows/win32/fileio/disk-partition-types
                                                    case PlatformWindowsInterop.PARTITION_INFORMATION_MBR.PARTITION_ENTRY_UNUSED:
                                                        // Unused entry
                                                        type = PartitionType.Unallocated;
                                                        break;
                                                    case PlatformWindowsInterop.PARTITION_INFORMATION_MBR.PARTITION_EXTENDED:
                                                        // Extended
                                                        type = PartitionType.Extended;
                                                        break;
                                                    case PlatformWindowsInterop.PARTITION_INFORMATION_MBR.PARTITION_FAT_12:
                                                        // FAT12
                                                        type = PartitionType.FAT12;
                                                        break;
                                                    case PlatformWindowsInterop.PARTITION_INFORMATION_MBR.PARTITION_FAT_16:
                                                        // FAT16
                                                        type = PartitionType.FAT16;
                                                        break;
                                                    case PlatformWindowsInterop.PARTITION_INFORMATION_MBR.PARTITION_FAT32:
                                                        // FAT32
                                                        type = PartitionType.FAT32;
                                                        break;
                                                    case PlatformWindowsInterop.PARTITION_INFORMATION_MBR.PARTITION_IFS:
                                                        // IFS
                                                        type = PartitionType.NTFS;
                                                        break;
                                                    case PlatformWindowsInterop.PARTITION_INFORMATION_MBR.PARTITION_LDM:
                                                        // LDM
                                                        type = PartitionType.SFS;
                                                        break;
                                                    case PlatformWindowsInterop.PARTITION_INFORMATION_MBR.PARTITION_NTFT:
                                                        // NTFT
                                                        type = PartitionType.OldMinix;
                                                        break;
                                                    case PlatformWindowsInterop.PARTITION_INFORMATION_MBR.PARTITION_VALID_NTFT:
                                                        // Valid NTFT
                                                        type = PartitionType.NTFT;
                                                        break;
                                                }
                                            }
                                        }

                                        // Add this partition
                                        parts.Add(new()
                                        {
                                            PartitionNumber = (int)partInfo.PartitionNumber,
                                            PartitionSize = partInfo.PartitionLength,
                                            PartitionType = type,
                                            PartitionBootable = isGpt ? type == PartitionType.EFISystem : partInfo.Mbr.BootIndicator,
                                            PartitionOffset = partInfo.StartingOffset,
                                        });
                                    }
                                }
                                else
                                {
                                    // Increase the buffer size by multiplying it by two.
                                    error = Marshal.GetLastWin32Error();
                                    buffSize *= 2;
                                }

                                // Free the drive layout handle
                                Marshal.FreeHGlobal(driveLayoutPtr);
                                driveLayoutPtr = IntPtr.Zero;
                            } while (error == 0x7A);

                            // Add a hard disk
                            disk.Partitions = parts.ToArray();
                            diskParts.Add(disk);
                        }

                        // Close the handle
                        PlatformWindowsInterop.CloseHandle(driveHandle);
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            // Finally, return an item array containing information
            errors = [.. exceptions];
            return diskParts.ToArray();
        }

        private static PartitionType DetermineType(Guid partitionTypeGptGuid)
        {
            if (partitionTypeGptGuid == Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                // Unused entry
                return PartitionType.Unallocated;
            }
            else if (partitionTypeGptGuid == Guid.Parse("c12a7328-f81f-11d2-ba4b-00a0c93ec93b"))
            {
                // EFI system
                return PartitionType.EFISystem;
            }
            else if (partitionTypeGptGuid == Guid.Parse("21686148-6449-6e6f-744e-656564454649"))
            {
                // BIOS boot system
                return PartitionType.GptBiosBoot;
            }
            else if (partitionTypeGptGuid == Guid.Parse("024DEE41-33E7-11D3-9D69-0008C781F39F"))
            {
                // BIOS boot system
                return PartitionType.GptMbrScheme;
            }
            else if (partitionTypeGptGuid == Guid.Parse("ebd0a0a2-b9e5-4433-87c0-68b6b72699c7"))
            {
                // Basic data (Windows)
                return PartitionType.NTFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("e3c9e316-0b5c-4db8-817d-f92df00215ae"))
            {
                // Microsoft Reserved
                return PartitionType.FAT32;
            }
            else if (partitionTypeGptGuid == Guid.Parse("de94bba4-06d1-4d40-a16a-bfd50179d6ac"))
            {
                // Microsoft Recovery
                return PartitionType.FAT32;
            }
            else if (partitionTypeGptGuid == Guid.Parse("5808c8aa-7e8f-42e0-85d2-e1e90434cfb3"))
            {
                // LDM metadata
                return PartitionType.SFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("af9b60a0-1431-4f62-bc68-3311714a69ad"))
            {
                // LDM metadata
                return PartitionType.SFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("75894C1E-3AEB-11D3-B7C1-7B03A0000000"))
            {
                // HP/UX Data
                return PartitionType.GptHpUxData;
            }
            else if (partitionTypeGptGuid == Guid.Parse("E2A1E728-32E3-11D6-A682-7B03A0000000"))
            {
                // HP/UX Service
                return PartitionType.GptHpUxService;
            }
            else if (partitionTypeGptGuid == Guid.Parse("0FC63DAF-8483-4772-8E79-3D69D8477DE4"))
            {
                // Linux
                return PartitionType.Linux;
            }
            else if (partitionTypeGptGuid == Guid.Parse("4F68BCE3-E8CD-4DB1-96E7-FBCAF984B709"))
            {
                // Linux
                return PartitionType.Linux;
            }
            else if (partitionTypeGptGuid == Guid.Parse("0657FD6D-A4AB-43C4-84E5-0933C84B4F4F"))
            {
                // Linux Swap
                return PartitionType.SwapOrSolaris;
            }
            else if (partitionTypeGptGuid == Guid.Parse("933AC7E1-2EB4-4F13-B844-0E14E2AEF915"))
            {
                // Linux
                return PartitionType.Linux;
            }
            else if (partitionTypeGptGuid == Guid.Parse("3B8F8425-20E0-4F3B-907F-1A25A76F98E8"))
            {
                // Linux
                return PartitionType.Linux;
            }
            else if (partitionTypeGptGuid == Guid.Parse("4D21B016-B534-45C2-A9FB-5C16E091FD2D"))
            {
                // Linux
                return PartitionType.Linux;
            }
            else if (partitionTypeGptGuid == Guid.Parse("7EC6F557-3BC5-4ACA-B293-16EF5DF639D1"))
            {
                // Linux
                return PartitionType.Linux;
            }
            else if (partitionTypeGptGuid == Guid.Parse("E6D6D379-F507-44C2-A23C-238F2A3DF928"))
            {
                // Linux LVM
                return PartitionType.LinuxLVM;
            }
            else if (partitionTypeGptGuid == Guid.Parse("A19D880F-05FC-4D3B-A006-743F0F84911E"))
            {
                // Linux RAID
                return PartitionType.LinuxRaid;
            }
            else if (partitionTypeGptGuid == Guid.Parse("CA7D7CCB-63ED-4C53-861C-1742536059CC"))
            {
                // Linux LUKS
                return PartitionType.Luks;
            }
            else if (partitionTypeGptGuid == Guid.Parse("7FFEC5C9-2D00-49B7-8941-3EA10A5586B7"))
            {
                // Linux dm-crypt
                return PartitionType.GptLinuxDmCrypt;
            }
            else if (partitionTypeGptGuid == Guid.Parse("83BD6B9D-7F41-11DC-BE0B-001560B84F0F"))
            {
                // FreeBSD Boot
                return PartitionType.GptFreeBsdBoot;
            }
            else if (partitionTypeGptGuid == Guid.Parse("516E7CB4-6ECF-11D6-8FF8-00022D09712B"))
            {
                // FreeBSD Data
                return PartitionType.BSD386;
            }
            else if (partitionTypeGptGuid == Guid.Parse("516E7CB5-6ECF-11D6-8FF8-00022D09712B"))
            {
                // FreeBSD Swap
                return PartitionType.GptFreeBsdSwap;
            }
            else if (partitionTypeGptGuid == Guid.Parse("516E7CB6-6ECF-11D6-8FF8-00022D09712B"))
            {
                // FreeBSD UFS
                return PartitionType.GptFreeBsdUfs;
            }
            else if (partitionTypeGptGuid == Guid.Parse("516E7CB8-6ECF-11D6-8FF8-00022D09712B"))
            {
                // FreeBSD Vinum
                return PartitionType.GptFreeBsdVinum;
            }
            else if (partitionTypeGptGuid == Guid.Parse("516E7CBA-6ECF-11D6-8FF8-00022D09712B"))
            {
                // FreeBSD ZFS
                return PartitionType.GptFreeBsdZfs;
            }
            else if (partitionTypeGptGuid == Guid.Parse("48465300-0000-11AA-AA11-00306543ECAC"))
            {
                // macOS HFS+
                return PartitionType.HFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("55465300-0000-11AA-AA11-00306543ECAC"))
            {
                // macOS UFS
                return PartitionType.MacOSX;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A898CC3-1DD2-11B2-99A6-080020736631"))
            {
                // macOS ZFS
                return PartitionType.GptMacOSZfs;
            }
            else if (partitionTypeGptGuid == Guid.Parse("52414944-0000-11AA-AA11-00306543ECAC"))
            {
                // macOS RAID (online)
                return PartitionType.GptMacOSOnlineRaid;
            }
            else if (partitionTypeGptGuid == Guid.Parse("52414944-5F4F-11AA-AA11-00306543ECAC"))
            {
                // macOS RAID (offline)
                return PartitionType.GptMacOSOfflineRaid;
            }
            else if (partitionTypeGptGuid == Guid.Parse("426F6F74-0000-11AA-AA11-00306543ECAC"))
            {
                // macOS Boot
                return PartitionType.MacOSXBoot;
            }
            else if (partitionTypeGptGuid == Guid.Parse("4C616265-6C00-11AA-AA11-00306543ECAC"))
            {
                // macOS Label
                return PartitionType.GptMacOSLabel;
            }
            else if (partitionTypeGptGuid == Guid.Parse("5265636F-7665-11AA-AA11-00306543ECAC"))
            {
                // macOS Apple TV Recovery
                return PartitionType.GptMacOSAppleTVRecovery;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A82CB45-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris boot
                return PartitionType.Solaris8Boot;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A85CF4D-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris root
                return PartitionType.SolarisNew;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A87C46F-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris swap
                return PartitionType.SwapOrSolaris;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A8B642B-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris backup
                return PartitionType.GptSolarisBackup;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A898CC3-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris /usr
                return PartitionType.SolarisNew;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A8EF2E9-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris /var
                return PartitionType.SolarisNew;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A90BA39-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris /home
                return PartitionType.SolarisNew;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A9283A5-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris EFI_ALTSCTR
                return PartitionType.GptSolarisAltsctr;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A945A3B-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris Reserved
                return PartitionType.GptSolarisReserved1;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A9630D1-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris Reserved
                return PartitionType.GptSolarisReserved2;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A980767-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris Reserved
                return PartitionType.GptSolarisReserved3;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A96237F-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris Reserved
                return PartitionType.GptSolarisReserved4;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A8D2AC7-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris Reserved
                return PartitionType.GptSolarisReserved5;
            }
            else if (partitionTypeGptGuid == Guid.Parse("49F48D32-B10E-11DC-B99B-0019D1879648"))
            {
                // NetBSD Swap
                return PartitionType.GptNetBSDSwap;
            }
            else if (partitionTypeGptGuid == Guid.Parse("49F48D5A-B10E-11DC-B99B-0019D1879648"))
            {
                // NetBSD FFS
                return PartitionType.GptNetBSDFFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("49F48D82-B10E-11DC-B99B-0019D1879648"))
            {
                // NetBSD LFS
                return PartitionType.GptNetBSDLFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("49F48DAA-B10E-11DC-B99B-0019D1879648"))
            {
                // NetBSD RAID
                return PartitionType.GptNetBSDRAID;
            }
            else if (partitionTypeGptGuid == Guid.Parse("2DB519C4-B10F-11DC-B99B-0019D1879648"))
            {
                // NetBSD Concatenated
                return PartitionType.GptNetBSDConcatenated;
            }
            else if (partitionTypeGptGuid == Guid.Parse("2DB519EC-B10F-11DC-B99B-0019D1879648"))
            {
                // NetBSD Encrypted
                return PartitionType.GptNetBSDEncrypted;
            }
            else if (partitionTypeGptGuid == Guid.Parse("03fedbca-aaaa-aaaa-aaaa-3f19aa5c2bb1"))
            {
                // Aptivi ParelOS Boot
                return PartitionType.ParelOSBoot;
            }
            else if (partitionTypeGptGuid == Guid.Parse("03fedbca-aaaa-aaaa-aaaa-2f19aa5c2bb2"))
            {
                // Aptivi ParelOS Data
                return PartitionType.ParelOSData;
            }
            else if (partitionTypeGptGuid == Guid.Parse("03fedbca-aaaa-aaaa-aaaa-1f19aa5c2bb3"))
            {
                // Aptivi ParelOS Swap
                return PartitionType.ParelOSSwap;
            }
            return PartitionType.Unknown;
        }
    }
}
