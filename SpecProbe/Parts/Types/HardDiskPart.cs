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
    /// Hard disk part
    /// </summary>
    public class HardDiskPart : BaseHardwarePartInfo, IHardwarePartInfo
    {
        /// <summary>
        /// Partition class for the hard drive
        /// </summary>
        public class PartitionPart
        {
            private int partNum;
            private long partSize;

            /// <summary>
            /// Partition number defined by the operating system
            /// </summary>
            public int PartitionNumber
            {
                get => partNum;
                internal set => partNum = value;
            }
            /// <summary>
            /// Partition size in bytes
            /// </summary>
            public long PartitionSize
            {
                get => partSize;
                internal set => partSize = value;
            }
        }

        private ulong hardDiskSize;
        private int hardDiskNum;
        private PartitionPart[] parts;

        /// <inheritdoc/>
        public override HardwarePartType Type =>
            HardwarePartType.HardDisk;

        /// <summary>
        /// Hard disk size in bytes
        /// </summary>
        public ulong HardDiskSize
        {
            get => hardDiskSize;
            internal set => hardDiskSize = value;
        }
        /// <summary>
        /// Hard disk number
        /// </summary>
        public int HardDiskNumber
        {
            get => hardDiskNum;
            internal set => hardDiskNum = value;
        }
        /// <summary>
        /// Number of partitions
        /// </summary>
        public int PartitionCount =>
            Partitions.Length;
        /// <summary>
        /// List of partitions
        /// </summary>
        public PartitionPart[] Partitions
        {
            get => parts;
            internal set => parts = value;
        }
    }
}
