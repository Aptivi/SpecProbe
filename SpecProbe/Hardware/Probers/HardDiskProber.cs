
// SpecProbe  Copyright (C) 2020-2021  Aptivi
// 
// This file is part of SpecProbe
// 
// SpecProbe is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SpecProbe is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using SpecProbe.Hardware.Parts;
using SpecProbe.Hardware.Parts.Types;
using SpecProbe.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SpecProbe.Hardware.Probers
{
    internal class HardDiskProber : IHardwareProber
    {
        public BaseHardwarePartInfo[] GetBaseHardwareParts()
        {
            if (PlatformHelper.IsOnWindows())
                return GetBaseHardwarePartsWindows();
            else
                return GetBaseHardwarePartsLinux();
        }

        public BaseHardwarePartInfo[] GetBaseHardwarePartsLinux()
        {
            // TODO: Android devices must be rooted to be able to run this.
            // Some variables to install.
            List<HardDiskPart> diskParts = new();
            List<HardDiskPart.PartitionPart> partitions = new();

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
                        string devName = blockFolder[(blockFolder.LastIndexOf("/") + 1)..];
                        string devPartFolderInitial = $"{blockFolder}/{devName}";

                        // Check if the dev name ends with a number (such as nvme0n1p1, mmcblk0p1, etc.)
                        bool appendPartitionChar = int.TryParse($"{devName[^1]}", out int devNum);
                        int partNum = 1;
                        while (true)
                        {
                            string partPath = appendPartitionChar ?
                                $"{devPartFolderInitial}p{partNum}" :
                                $"{devPartFolderInitial}{partNum}";
                            if (!Directory.Exists(partPath))
                            {
                                if (partNum <= 4)
                                {
                                    partNum++;
                                    continue;
                                }
                                break;
                            }
                            string partSizeFile = $"{partPath}/size";
                            string partSizeStr = File.ReadAllLines(partSizeFile)[0];
                            long partSize = long.Parse(partSizeStr);
                            long partActualSize = partSize * 512;
                            partitions.Add(new HardDiskPart.PartitionPart
                            {
                                PartitionNumber = partNum,
                                PartitionSize = partActualSize,
                            });
                            partNum++;
                        }

                        // Add disk
                        diskParts.Add(new HardDiskPart
                        {
                            HardDiskSize = actualSize,
                            HardDiskNumber = devNum,
                            Partitions = partitions.ToArray(),
                        });
                        partitions.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                HardwareProber.errors.Add(ex);
            }

            // Finally, return an array containing information
            return diskParts.ToArray();
        }

        public BaseHardwarePartInfo[] GetBaseHardwarePartsWindows()
        {
            List<HardDiskPart> diskParts = new();
            List<HardDiskPart.PartitionPart> partitions = new();

            try
            {
                // First, get the list of logical partitions so that we can get the letters agnostically
                var drives = DriveInfo.GetDrives();
                List<(string letter, long partSize)> parts = new();
                foreach (var drive in drives)
                {
                    if (drive.IsReady)
                    {
                        // Drive is ready. Now, check the type, as this prober only checks for HDD
                        if (drive.DriveType == DriveType.Fixed)
                            parts.Add(("\\\\.\\" + drive.Name[..(drive.Name.Length - 1)], drive.TotalSize));
                    }
                }

                // Then, open the file handle to these drives for us to be able to get the hard drive geometry
                List<IntPtr> driveHandles = new();
                foreach (var (letter, partSize) in parts)
                {
                    IntPtr driveHandle = PlatformWindowsInterop.CreateFile(letter, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, FileMode.Open, FileAttributes.System, IntPtr.Zero);
                    if (driveHandle != new IntPtr(-1))
                        driveHandles.Add(driveHandle);
                }

                // Enumerate through the handles to try to get their drive geometry
                for (int i = 0; i < driveHandles.Count; i++)
                {
                    // Try to get drive storage info
                    IntPtr drive = driveHandles[i];
                    bool result = PlatformWindowsInterop.DeviceIoControl(drive, PlatformWindowsInterop.EIOControlCode.DiskGetDriveGeometry, null, 0, out PlatformWindowsInterop.DISK_GEOMETRY drvGeom, Marshal.SizeOf<PlatformWindowsInterop.DISK_GEOMETRY>(), out _, IntPtr.Zero);
                    if (!result)
                        continue;

                    // Get the hard disk size and number
                    bool partResult = PlatformWindowsInterop.DeviceIoControl(drive, PlatformWindowsInterop.EIOControlCode.StorageGetDeviceNumber, null, 0, out PlatformWindowsInterop.STORAGE_DEVICE_NUMBER devNumber, Marshal.SizeOf<PlatformWindowsInterop.STORAGE_DEVICE_NUMBER>(), out _, IntPtr.Zero);
                    if (!partResult)
                        continue;
                    ulong hardDiskSize = (ulong)(drvGeom.Cylinders * drvGeom.TracksPerCylinder * drvGeom.SectorsPerTrack * drvGeom.BytesPerSector);
                    int diskNum = devNumber.DeviceNumber;

                    // Install the initial hard disk part
                    if (diskParts.Count == 0 || !diskParts.Any((part) => part.HardDiskNumber == diskNum))
                    {
                        partitions.Clear();
                        diskParts.Add(new HardDiskPart
                        {
                            HardDiskSize = hardDiskSize,
                        });
                    }

                    // Now, deal with making partition info classes
                    int partNum = devNumber.PartitionNumber;
                    partitions.Add(new HardDiskPart.PartitionPart
                    {
                        PartitionNumber = partNum,
                        PartitionSize = parts[i].partSize,
                    });
                    diskParts[^1].HardDiskNumber = diskNum;
                    diskParts[^1].Partitions = partitions.ToArray();
                }
            }
            catch (Exception ex)
            {
                HardwareProber.errors.Add(ex);
            }

            // Finally, return an item array containing information
            return diskParts.ToArray();
        }
    }
}
