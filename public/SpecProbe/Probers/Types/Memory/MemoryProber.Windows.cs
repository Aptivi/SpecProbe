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

using SpecProbe.Languages;
using SpecProbe.Parts.Types;
using SpecProbe.Probers.Platform;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SpecProbe.Probers.Types.Memory
{
    internal static partial class MemoryProber
    {
        public static MemoryPart ProbeWindows(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            long totalMemory = 0;
            long totalPhysicalMemory = 0;

            try
            {
                // Get memory info (base)
                var status = new PlatformWindowsInterop.MEMORYSTATUSEX()
                {
                    dwLength = (uint)Marshal.SizeOf(typeof(PlatformWindowsInterop.MEMORYSTATUSEX)),
                };
                if (!PlatformWindowsInterop.GlobalMemoryStatusEx(ref status))
                    throw new Exception(LanguageTools.GetLocalized("SPECPROBE_PROBERS_EXCEPTION_CANTPARSEMEMSTATUS"));
                totalMemory = (long)status.ullTotalPhys;

                // Get physically installed memory to all the RAM slots
                if (!PlatformWindowsInterop.GetPhysicallyInstalledSystemMemory(ref totalPhysicalMemory))
                    throw new Exception(LanguageTools.GetLocalized("SPECPROBE_PROBERS_EXCEPTION_CANTPARSEMEMSTATUS"));
                totalPhysicalMemory *= 1024;
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
