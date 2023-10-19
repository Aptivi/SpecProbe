
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

namespace SpecProbe.Hardware.Probers
{
    internal interface IHardwareProber
    {
        // Wrapper function for platform-specific functions
        BaseHardwarePartInfo[] GetBaseHardwareParts();

        // Platform-specific functions
        BaseHardwarePartInfo[] GetBaseHardwarePartsWindows();
        BaseHardwarePartInfo[] GetBaseHardwarePartsLinux();
        BaseHardwarePartInfo[] GetBaseHardwarePartsMacOS();
    }
}
