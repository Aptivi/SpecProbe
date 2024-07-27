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

using SpecProbe.Parts;

namespace SpecProbe.Parts.Types
{
    /// <summary>
    /// Memory part
    /// </summary>
    public class MemoryPart : BaseHardwarePartInfo, IHardwarePartInfo
    {
        private long totalMemory;
        private long totalPhysicalMemory;

        /// <inheritdoc/>
        public override HardwarePartType Type =>
            HardwarePartType.Memory;

        /// <summary>
        /// Total memory available to the system
        /// </summary>
        public long TotalMemory
        {
            get => totalMemory;
            internal set => totalMemory = value;
        }
        /// <summary>
        /// Total memory installed physically, including reserved memory
        /// </summary>
        public long TotalPhysicalMemory
        {
            get => totalPhysicalMemory;
            internal set => totalPhysicalMemory = value;
        }
        /// <summary>
        /// System reserved memory (can't be used by the operating system)
        /// </summary>
        public long SystemReservedMemory =>
            TotalPhysicalMemory - TotalMemory;
    }
}
