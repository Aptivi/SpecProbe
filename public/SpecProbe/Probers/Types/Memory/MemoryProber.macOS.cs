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
        public static MemoryPart ProbeMacOS(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            long totalMemory = 0;
            long totalPhysicalMemory = 0;

            // Some constants
            const string total = "hw.memsize: ";
            const string totalUsable = "hw.memsize_usable: ";

            try
            {
                string sysctlOutput = PlatformHelper.ExecuteProcessToString("/usr/sbin/sysctl", "hw.memsize_usable hw.memsize");
                string[] sysctlOutputLines = sysctlOutput.Replace("\r", "").Split('\n');
                foreach (string sysctlOutputLine in sysctlOutputLines)
                {
                    if (sysctlOutputLine.StartsWith(total))
                        totalMemory = long.Parse(sysctlOutputLine.Substring(total.Length));
                    if (sysctlOutputLine.StartsWith(totalUsable))
                        totalPhysicalMemory = long.Parse(sysctlOutputLine.Substring(totalUsable.Length));
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
