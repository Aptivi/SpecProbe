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
using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpecProbe.Probers.Types.HardDisk
{
    internal static partial class HardDiskProber
    {
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
                            Removable = !blockFixed,
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
    }
}
