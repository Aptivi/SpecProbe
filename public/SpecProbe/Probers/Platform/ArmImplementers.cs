﻿//
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

using System.Collections.Generic;

namespace SpecProbe.Probers.Platform
{
    internal static class ArmImplementers
    {
        // Sourced from https://github.com/util-linux/util-linux/blob/master/sys-utils/lscpu-arm.c
        internal static List<ArmImplementer> implementers =
        [
            new(0x41, "ARM", 
                [
                    new(0x810, "ARM810"),
                    new(0x920, "ARM920"),
                    new(0x922, "ARM922"),
                    new(0x926, "ARM926"),
                    new(0x940, "ARM940"),
                    new(0x946, "ARM946"),
                    new(0x966, "ARM966"),
                    new(0xa20, "ARM1020"),
                    new(0xa22, "ARM1022"),
                    new(0xa26, "ARM1026"),
                    new(0xb02, "ARM11 MPCore"),
                    new(0xb36, "ARM1136"),
                    new(0xb56, "ARM1156"),
                    new(0xb76, "ARM1176"),
                    new(0xc05, "Cortex-A5"),
                    new(0xc07, "Cortex-A7"),
                    new(0xc08, "Cortex-A8"),
                    new(0xc09, "Cortex-A9"),
                    new(0xc0d, "Cortex-A17"),
                    new(0xc0f, "Cortex-A15"),
                    new(0xc0e, "Cortex-A17"),
                    new(0xc14, "Cortex-R4"),
                    new(0xc15, "Cortex-R5"),
                    new(0xc17, "Cortex-R7"),
                    new(0xc18, "Cortex-R8"),
                    new(0xc20, "Cortex-M0"),
                    new(0xc21, "Cortex-M1"),
                    new(0xc23, "Cortex-M3"),
                    new(0xc24, "Cortex-M4"),
                    new(0xc27, "Cortex-M7"),
                    new(0xc60, "Cortex-M0+"),
                    new(0xd01, "Cortex-A32"),
                    new(0xd02, "Cortex-A34"),
                    new(0xd03, "Cortex-A53"),
                    new(0xd04, "Cortex-A35"),
                    new(0xd05, "Cortex-A55"),
                    new(0xd06, "Cortex-A65"),
                    new(0xd07, "Cortex-A57"),
                    new(0xd08, "Cortex-A72"),
                    new(0xd09, "Cortex-A73"),
                    new(0xd0a, "Cortex-A75"),
                    new(0xd0b, "Cortex-A76"),
                    new(0xd0c, "Neoverse-N1"),
                    new(0xd0d, "Cortex-A77"),
                    new(0xd0e, "Cortex-A76AE"),
                    new(0xd13, "Cortex-R52"),
                    new(0xd15, "Cortex-R82"),
                    new(0xd16, "Cortex-R52+"),
                    new(0xd20, "Cortex-M23"),
                    new(0xd21, "Cortex-M33"),
                    new(0xd22, "Cortex-M55"),
                    new(0xd23, "Cortex-M85"),
                    new(0xd40, "Neoverse-V1"),
                    new(0xd41, "Cortex-A78"),
                    new(0xd42, "Cortex-A78AE"),
                    new(0xd43, "Cortex-A65AE"),
                    new(0xd44, "Cortex-X1"),
                    new(0xd46, "Cortex-A510"),
                    new(0xd47, "Cortex-A710"),
                    new(0xd48, "Cortex-X2"),
                    new(0xd49, "Neoverse-N2"),
                    new(0xd4a, "Neoverse-E1"),
                    new(0xd4b, "Cortex-A78C"),
                    new(0xd4c, "Cortex-X1C"),
                    new(0xd4d, "Cortex-A715"),
                    new(0xd4e, "Cortex-X3"),
                    new(0xd4f, "Neoverse-V2"),
                    new(0xd80, "Cortex-A520"),
                    new(0xd81, "Cortex-A720"),
                    new(0xd82, "Cortex-X4"),
                    new(0xd84, "Neoverse-V3"),
                    new(0xd85, "Cortex-X925"),
                    new(0xd87, "Cortex-A725"),
                    new(0xd8e, "Neoverse-N3"),
                ]),
            new(0x42, "Broadcom",
                [
                    new(0x0f, "Brahma-B15"),
                    new(0x100, "Brahma-B53"),
                    new(0x516, "ThunderX2"),
                ]),
            new(0x43, "Cavium",
                [
                    new(0x0a0, "ThunderX"),
                    new(0x0a1, "ThunderX-88XX"),
                    new(0x0a2, "ThunderX-81XX"),
                    new(0x0a3, "ThunderX-83XX"),
                    new(0x0af, "ThunderX2-99xx"),
                    new(0x0b0, "OcteonTX2"),
                    new(0x0b1, "OcteonTX2-98XX"),
                    new(0x0b2, "OcteonTX2-96XX"),
                    new(0x0b3, "OcteonTX2-95XX"),
                    new(0x0b4, "OcteonTX2-95XXN"),
                    new(0x0b5, "OcteonTX2-95XXMM"),
                    new(0x0b6, "OcteonTX2-95XXO"),
                    new(0x0b8, "ThunderX3-T110"),
                ]),
            new(0x44, "DEC",
                [
                    new(0xa10, "SA110"),
                    new(0xa11, "SA1100"),
                ]),
            new(0x46, "FUJITSU",
                [
                    new(0x001, "A64FX"),
                ]),
            new(0x48, "HiSilicon",
                [
                    new(0xd01, "TaiShan-v110"),
                    new(0xd02, "TaiShan-v120"),
                    new(0xd40, "Cortex-A76"),
                    new(0xd41, "Cortex-A77"),
                ]),
            new(0x4e, "NVIDIA",
                [
                    new(0x000, "Denver"),
                    new(0x003, "Denver 2"),
                    new(0x004, "Carmel"),
                ]),
            new(0x50, "APM",
                [
                    new(0x000, "X-Gene"),
                ]),
            new(0x51, "Qualcomm",
                [
                    new(0x001, "Oryon"),
                    new(0x00f, "Scorpion"),
                    new(0x02d, "Scorpion"),
                    new(0x04d, "Krait"),
                    new(0x06f, "Krait"),
                    new(0x201, "Kryo"),
                    new(0x205, "Kryo"),
                    new(0x211, "Kryo"),
                    new(0x800, "Falkor-V1/Kryo"),
                    new(0x801, "Kryo-V2"),
                    new(0x802, "Kryo-3XX-Gold"),
                    new(0x803, "Kryo-3XX-Silver"),
                    new(0x804, "Kryo-4XX-Gold"),
                    new(0x805, "Kryo-4XX-Silver"),
                    new(0xc00, "Falkor"),
                    new(0xc01, "Saphira"),
                ]),
            new(0x53, "Samsung",
                [
                    new(0x001, "exynos-m1"),
                    new(0x002, "exynos-m3"),
                    new(0x003, "exynos-m4"),
                    new(0x004, "exynos-m5"),
                ]),
            new(0x56, "Marvell",
                [
                    new(0x131, "Feroceon-88FR131"),
                    new(0x581, "PJ4/PJ4b"),
                    new(0x584, "PJ4B-MP"),
                ]),
            new(0x61, "Apple",
                [
                    new(0x000, "Swift"),
                    new(0x001, "Cyclone"),
                    new(0x002, "Typhoon"),
                    new(0x003, "Typhoon/Capri"),
                    new(0x004, "Twister"),
                    new(0x005, "Twister/Elba/Malta"),
                    new(0x006, "Hurricane"),
                    new(0x007, "Hurricane/Myst"),
                    new(0x008, "Monsoon"),
                    new(0x009, "Mistral"),
                    new(0x00b, "Vortex"),
                    new(0x00c, "Tempest"),
                    new(0x00f, "Tempest-M9"),
                    new(0x010, "Vortex/Aruba"),
                    new(0x011, "Tempest/Aruba"),
                    new(0x012, "Lightning"),
                    new(0x013, "Thunder"),
                    new(0x020, "Icestorm-A14"),
                    new(0x021, "Firestorm-A14"),
                    new(0x022, "Icestorm-M1"),
                    new(0x023, "Firestorm-M1"),
                    new(0x024, "Icestorm-M1-Pro"),
                    new(0x025, "Firestorm-M1-Pro"),
                    new(0x026, "Thunder-M10"),
                    new(0x028, "Icestorm-M1-Max"),
                    new(0x029, "Firestorm-M1-Max"),
                    new(0x030, "Blizzard-A15"),
                    new(0x031, "Avalanche-A15"),
                    new(0x032, "Blizzard-M2"),
                    new(0x033, "Avalanche-M2"),
                    new(0x034, "Blizzard-M2-Pro"),
                    new(0x035, "Avalanche-M2-Pro"),
                    new(0x036, "Sawtooth-A16"),
                    new(0x037, "Everest-A16"),
                    new(0x038, "Blizzard-M2-Max"),
                    new(0x039, "Avalanche-M2-Max"),
                ]),
            new(0x66, "Faraday",
                [
                    new(0x526, "FA526"),
                    new(0x626, "FA626"),
                ]),
            new(0x69, "Intel",
                [
                    new(0x200, "i80200"),
                    new(0x210, "PXA250A"),
                    new(0x212, "PXA210A"),
                    new(0x242, "i80321-400"),
                    new(0x243, "i80321-600"),
                    new(0x290, "PXA250B/PXA26x"),
                    new(0x292, "PXA210B"),
                    new(0x2c2, "i80321-400-B0"),
                    new(0x2c3, "i80321-600-B0"),
                    new(0x2d0, "PXA250C/PXA255/PXA26x"),
                    new(0x2d2, "PXA210C"),
                    new(0x411, "PXA27x"),
                    new(0x41c, "IPX425-533"),
                    new(0x41d, "IPX425-400"),
                    new(0x41f, "IPX425-266"),
                    new(0x682, "PXA32x"),
                    new(0x683, "PXA930/PXA935"),
                    new(0x688, "PXA30x"),
                    new(0x689, "PXA31x"),
                    new(0xb11, "SA1110"),
                    new(0xc12, "IPX1200"),
                ]),
            new(0x6d, "Microsoft",
                [
                    new(0xd49, "Azure-Cobalt-100"),
                ]),
            new(0x70, "Phytium",
                [
                    new(0x303, "FTC310"),
                    new(0x660, "FTC660"),
                    new(0x661, "FTC661"),
                    new(0x662, "FTC662"),
                    new(0x663, "FTC663"),
                    new(0x664, "FTC664"),
                    new(0x862, "FTC862"),
                ]),
            new(0xc0, "Ampere",
                [
                    new(0xac3, "Ampere-1"),
                    new(0xac4, "Ampere-1a"),
                ]),
        ];
    }
}
