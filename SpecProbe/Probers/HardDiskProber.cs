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
            else if (partitionTypeGptGuid == Guid.Parse("ebd0a0a2-b9e5-4433-87c0-68b6b72699c7"))
            {
                // Basic data
                return PlatformHelper.IsOnUnix() ? PartitionType.Linux : PartitionType.NTFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("c12a7328-f81f-11d2-ba4b-00a0c93ec93b"))
            {
                // EFI system
                return PartitionType.EFISystem;
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
            return PartitionType.Unknown;
        }
    }
}
