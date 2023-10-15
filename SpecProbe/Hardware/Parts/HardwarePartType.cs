
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

namespace SpecProbe.Hardware.Parts
{
    /// <summary>
    /// Hardware part type
    /// </summary>
    public enum HardwarePartType
    {
        /// <summary>
        /// Unknown hardware type
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Processors
        /// </summary>
        Processor,
        /// <summary>
        /// Random Access Memory
        /// </summary>
        Memory,
        /// <summary>
        /// Video Adapters (Graphics Cards)
        /// </summary>
        Video,
        /// <summary>
        /// Hard Drive
        /// </summary>
        HardDisk,
    }
}
