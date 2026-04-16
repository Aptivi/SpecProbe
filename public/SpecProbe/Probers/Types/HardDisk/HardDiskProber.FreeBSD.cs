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
using Textify.General;

namespace SpecProbe.Probers.Types.HardDisk
{
    internal static partial class HardDiskProber
    {
        public static HardDiskPart[] ProbeFreeBSD(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            List<HardDiskPart> diskParts = [];

            // Get the blocks
            try
            {
                string blockListFolder = "/dev";
                string[] blockFolders = Directory.GetFiles(blockListFolder).Where((dir) => dir.StartsWithAnyOf(["/dev/da", "/dev/ada", "/dev/nda"]) && !dir.ContainsAnyOf(["p", "s"])).ToArray();
                for (int i = 0; i < blockFolders.Length; i++)
                {
                    List<HardDiskPart.PartitionPart> partitions = [];
                    string blockFolder = blockFolders[i];

                    // Necessary for gpart list parsing
                    string camRemovable = "REMOVABLE";
                    string providersTag = "Providers:";
                    string consumersTag = "Consumers:";
                    string mediaSizeTag = "Mediasize:";
                    string partTypeTag = "rawtype:";
                    string partOffsetTag = "offset:";
                    string blockSchemeTag = "scheme:";

                    // Some variables for the block
                    bool blockFixed = true;
                    string reallyDiskId = "";
                    string partType = "";
                    Guid gptPartType = Guid.Empty;
                    var partTypeEnum = PartitionType.Unknown;
                    string blockScheme = "";
                    ulong partOffset = 0;
                    ulong actualSize = 0;

                    // Get the block name
                    reallyDiskId = blockFolder.Split('/')[2];
                    string daName = reallyDiskId.Substring(0, reallyDiskId.IndexOf("da") + 2);
                    string daPartId = daName.Substring(daName.Length);
                    string daNameTag = $". Name: {daName}";

                    // Execute "sysctl" on the block to determine whether this disk is fixed or removable and set flag as appropriate
                    string sysctlOutput = PlatformHelper.ExecuteProcessToString("/sbin/sysctl", $"kern.cam.{daName}.{daPartId}.flags");
                    string[] sysctlOutputLines = sysctlOutput.Replace("\r", "").Split('\n');
                    blockFixed = !sysctlOutputLines[0].Contains(camRemovable);

                    // Execute "gpart list" on that block
                    string gpartOutput = PlatformHelper.ExecuteProcessToString("/sbin/gpart", $"list {reallyDiskId}");
                    string[] gpartOutputLines = gpartOutput.Replace("\r", "").Split('\n');
                    bool inPartition = false;
                    int partNum = 0;
                    foreach (string gpartOutputLine in gpartOutputLines)
                    {
                        string trimmedLine = gpartOutputLine.Trim();

                        // Determine whether we're dealing with partition or disk
                        if (inPartition && trimmedLine.StartsWith(consumersTag))
                            inPartition = false;
                        if (!inPartition && trimmedLine.StartsWith(providersTag))
                            inPartition = true;

                        // Check to see if we're traversing to another partition
                        bool traversing = trimmedLine.Contains(daNameTag);
                        if (traversing)
                        {
                            // Add this partition if we have info
                            if (partNum > 0)
                            {
                                partitions.Add(new HardDiskPart.PartitionPart
                                {
                                    PartitionNumber = partNum,
                                    PartitionSize = (long)actualSize,
                                    PartitionOffset = (long)partOffset,
                                    PartitionType = partTypeEnum,
                                });
                            }

                            // Increase partition number
                            if (traversing)
                                partNum++;
                            continue;
                        }

                        // Now, add partition information
                        if (trimmedLine.StartsWith(blockSchemeTag))
                        {
                            // Trim the tag to get the value.
                            blockScheme = trimmedLine.Substring(blockSchemeTag.Length).Trim();
                        }
                        if (trimmedLine.StartsWith(partTypeTag))
                        {
                            // Trim the tag to get the value.
                            partType = trimmedLine.Substring(partTypeTag.Length).Trim();
                            if (Guid.TryParse(partType, out Guid gptType))
                                partTypeEnum = GptPartitionTypeTools.TranslateFromGpt(gptType);
                            else if (int.TryParse(partType, out int mbrType))
                                partTypeEnum = (PartitionType)mbrType;
                        }
                        if (trimmedLine.StartsWith(partOffsetTag))
                        {
                            // Trim the tag to get the value like:
                            //    offset: 1074790400
                            string sizes = trimmedLine.Substring(partOffsetTag.Length).Trim();
                            partOffset = ulong.Parse(sizes);
                        }
                        if (trimmedLine.StartsWith(mediaSizeTag))
                        {
                            // Trim the tag to get the value like:
                            //    Mediasize: 511033999360 (476G)
                            string sizes = trimmedLine.Substring(mediaSizeTag.Length).Trim();

                            // We don't want to make the same mistake as we've done in the past for Inxi.NET, so we need to
                            // get the number of bytes from that.
                            sizes = sizes.Substring(0, sizes.IndexOf(' '));
                            actualSize = ulong.Parse(sizes);
                        }
                    }

                    // Add the hard disk part
                    diskParts.Add(new HardDiskPart
                    {
                        HardDiskSize = actualSize,
                        HardDiskNumber = i,
                        Removable = !blockFixed,
                        Partitions = [.. partitions],
                        PartitionTableType =
                            Enum.TryParse<PartitionTableType>(blockScheme, out var partitionTableType) ? partitionTableType :
                            PartitionTableType.Unknown,
                    });
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
