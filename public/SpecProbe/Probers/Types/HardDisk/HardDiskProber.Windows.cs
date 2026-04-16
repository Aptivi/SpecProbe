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
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SpecProbe.Probers.Types.HardDisk
{
    internal static partial class HardDiskProber
    {
        public static HardDiskPart[] ProbeWindows(out Exception[] errors)
        {
            List<Exception> exceptions = [];
            List<HardDiskPart> diskParts = [];

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
                        // Get the hard drive number, size, and removable condition
                        bool result = PlatformWindowsInterop.DeviceIoControl(driveHandle, PlatformWindowsInterop.EIOControlCode.DiskGetDriveGeometry, null, 0, out PlatformWindowsInterop.DISK_GEOMETRY drvGeom, Marshal.SizeOf<PlatformWindowsInterop.DISK_GEOMETRY>(), out _, IntPtr.Zero);
                        if (!result)
                            continue;
                        bool numResult = PlatformWindowsInterop.DeviceIoControl(driveHandle, PlatformWindowsInterop.EIOControlCode.StorageGetDeviceNumber, null, 0, out PlatformWindowsInterop.STORAGE_DEVICE_NUMBER devNumber, Marshal.SizeOf<PlatformWindowsInterop.STORAGE_DEVICE_NUMBER>(), out _, IntPtr.Zero);
                        if (!numResult)
                            continue;
                        ulong hardDiskSize = (ulong)(drvGeom.Cylinders * drvGeom.TracksPerCylinder * drvGeom.SectorsPerTrack * drvGeom.BytesPerSector);
                        int diskNum = devNumber.DeviceNumber;
                        bool removable = drvGeom.MediaType == PlatformWindowsInterop.MEDIA_TYPE.RemovableMedia;

                        // Get the partitions
                        var partitionTableType = PartitionTableType.Unknown;
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
                                        partitionTableType = PartitionTableType.MBR;
                                        break;
                                    case PlatformWindowsInterop.PARTITION_STYLE.PARTITION_STYLE_GPT:
                                        partitionTableType = PartitionTableType.GPT;
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
                                            type = GptPartitionTypeTools.TranslateFromGpt(typeGuid);
                                        }
                                        else
                                            type = (PartitionType)partInfo.Mbr.PartitionType;
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
                        var disk = new HardDiskPart()
                        {
                            HardDiskSize = hardDiskSize,
                            HardDiskNumber = diskNum,
                            Removable = removable,
                            Partitions = [.. parts],
                            PartitionTableType = partitionTableType,
                        };
                        diskParts.Add(disk);

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
    }
}
