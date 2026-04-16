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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SpecProbe.Probers.Types.Memory
{
    internal static partial class MemoryProber
    {
        public static MemoryPart ProbeLinux(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            long totalMemory = 0;
            long totalPhysicalMemory = 0;

            // Some constants
            const string total = "MemTotal:";

            try
            {
                // Open the meminfo file
                string memInfoFile = "/proc/meminfo";
                string[] memInfoContents = File.ReadAllLines(memInfoFile);
                foreach (string memInfoLine in memInfoContents)
                {
                    // Get the total memory amount
                    if (memInfoLine.StartsWith(total))
                    {
                        // in KiB
                        string totalMemString = memInfoLine.Replace(total, "").Trim().Replace(" kB", "");
                        long totalMemKb = long.Parse(totalMemString);
                        long totalMemNum = totalMemKb * 1024;
                        totalMemory = totalMemNum;
                    }
                }

                // Open the cache list to get cache sizes in kilobytes
                string memoryBlockListFolder = "/sys/devices/system/memory";
                string memoryBlockSizeFile = $"{memoryBlockListFolder}/block_size_bytes";
                if (File.Exists(memoryBlockSizeFile))
                {
                    string memoryBlockSizeStr = File.ReadAllLines(memoryBlockSizeFile)[0];
                    long memoryBlockSize = long.Parse(memoryBlockSizeStr, NumberStyles.AllowHexSpecifier);

                    // Get all memory blocks
                    string[] memoryBlockFolders = Directory.GetDirectories(memoryBlockListFolder).Where((dir) => !dir.EndsWith("power")).ToArray();
                    foreach (string memoryBlockFolder in memoryBlockFolders)
                    {
                        // Verify that the memory is online
                        string memoryBlockStateFile = $"{memoryBlockFolder}/online";
                        string memoryBlockStateStr = File.ReadAllLines(memoryBlockStateFile)[0];
                        int memoryBlockState = int.Parse(memoryBlockStateStr);
                        if (memoryBlockState == 1)
                            totalPhysicalMemory += memoryBlockSize;
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            // Finally, return a single item array containing information
            MemoryPart part = new()
            {
                TotalMemory = totalMemory,
                TotalPhysicalMemory = totalPhysicalMemory,
            };
            errors = [.. exceptions];
            return part;
        }
    }
}
