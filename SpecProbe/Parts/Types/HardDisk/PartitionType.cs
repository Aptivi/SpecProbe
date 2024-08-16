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

namespace SpecProbe.Parts.Types.HardDisk
{
    /// <summary>
    /// Partition type
    /// </summary>
    public enum PartitionType
    {
        /// <summary>
        /// Unallocated partition
        /// </summary>
        Unallocated = 0x00,
        /// <summary>
        /// Extended partition
        /// </summary>
        Extended = 0x05,
        /// <summary>
        /// FAT12 partition
        /// </summary>
        FAT12 = 0x01,
        /// <summary>
        /// FAT16 partition
        /// </summary>
        FAT16 = 0x04,
        /// <summary>
        /// FAT32 partition
        /// </summary>
        FAT32 = 0x0B,
        /// <summary>
        /// NTFS/HPFS/exFAT partition
        /// </summary>
        NTFS = 0x07,
        /// <summary>
        /// SFS/LDM partition
        /// </summary>
        SFS = 0x42,
        /// <summary>
        /// EFI system partition
        /// </summary>
        EFISystem = 0xEF,
        /// <summary>
        /// Old Minix or NTFT partition
        /// </summary>
        OldMinix = 0x80,
        /// <summary>
        /// Linux Swap / Solaris partition
        /// </summary>
        SwapOrSolaris = 0x82,
        /// <summary>
        /// Linux partition
        /// </summary>
        Linux = 0x83,
        /// <summary>
        /// Valid NTFT partition
        /// </summary>
        NTFT = 0xC0,
        /// <summary>
        /// Unknown partition type
        /// </summary>
        Unknown = -1,
    }
}
