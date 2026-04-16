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
using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;

namespace SpecProbe.Probers.Types.Memory
{
    internal static partial class MemoryProber
    {
        public static MemoryPart ProbeFreeBSD(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            long totalMemory = 0;
            long totalPhysicalMemory = 0;

            // Some constants
            const string total = "hw.realmem: ";
            const string totalUsable = "hw.physmem: ";

            try
            {
                string sysctlOutput = PlatformHelper.ExecuteProcessToString("/sbin/sysctl", "hw.realmem hw.physmem");
                string[] sysctlOutputLines = sysctlOutput.Replace("\r", "").Split('\n');
                foreach (string sysctlOutputLine in sysctlOutputLines)
                {
                    if (sysctlOutputLine.StartsWith(total))
                        totalPhysicalMemory = long.Parse(sysctlOutputLine.Substring(total.Length));
                    if (sysctlOutputLine.StartsWith(totalUsable))
                        totalMemory = long.Parse(sysctlOutputLine.Substring(totalUsable.Length));
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
