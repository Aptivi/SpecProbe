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
using System.Globalization;
using System.IO;
using System.Linq;
using Textify.General;

namespace SpecProbe.Probers.Types.HardDisk
{
    internal static partial class HardDiskProber
    {
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
                string[] blockFolders = Directory.GetDirectories(blockListFolder).Where((dir) => !dir.ContainsAnyOf(["/sys/block/loop", "/sys/block/zram"])).ToArray();
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
                                type = GptPartitionTypeTools.TranslateFromGpt(gptType);
                            else if (int.TryParse(output.Substring(2), NumberStyles.HexNumber, null, out int mbrType))
                                type = (PartitionType)mbrType;
                            ptType = outputPt switch
                            {
                                "dos" => PartitionTableType.MBR,
                                "msdos" => PartitionTableType.MBR,
                                "gpt" => PartitionTableType.GPT,

                                // Rare partition tables
                                "aix" => PartitionTableType.AIX,
                                "amiga" => PartitionTableType.Amiga,
                                "atari" => PartitionTableType.Atari,
                                "bsd" => PartitionTableType.BSD,
                                "dasd" => PartitionTableType.DASD,
                                "dvh" => PartitionTableType.DVH,
                                "mac" => PartitionTableType.Apple,
                                "pc98" => PartitionTableType.PC98,
                                "rdb" => PartitionTableType.RDB,
                                "sun" => PartitionTableType.Sun,

                                // Loopback
                                "loop" => PartitionTableType.Loop,

                                // Nonexistent tables
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
    }
}
