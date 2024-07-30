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
using SpecProbe.Parts.Types;
using SpecProbe.Probers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpecProbe
{
    /// <summary>
    /// Hardware probing class
    /// </summary>
    public static class HardwareProber
    {
        internal static bool notarized = false;
        internal static List<Exception> errors = [];
        private static readonly Dictionary<HardwarePartType, BaseHardwarePartInfo[]> cachedParts = [];

        /// <summary>
        /// Gets the list of processors (always 1)
        /// </summary>
        public static ProcessorPart[] Processors =>
            (cachedParts.Keys.Contains(HardwarePartType.Processor) && cachedParts[HardwarePartType.Processor].Length > 0 ?
             cachedParts[HardwarePartType.Processor] :
             ProcessorProber.GetBaseHardwareParts()) as ProcessorPart[];

        /// <summary>
        /// Gets the list of memory (always 1)
        /// </summary>
        public static MemoryPart[] Memory =>
            (cachedParts.Keys.Contains(HardwarePartType.Memory) && cachedParts[HardwarePartType.Memory].Length > 0 ?
             cachedParts[HardwarePartType.Memory] :
             MemoryProber.GetBaseHardwareParts()) as MemoryPart[];

        /// <summary>
        /// Gets the list of video cards
        /// </summary>
        public static VideoPart[] Video =>
            (cachedParts.Keys.Contains(HardwarePartType.Video) && cachedParts[HardwarePartType.Video].Length > 0 ?
             cachedParts[HardwarePartType.Video] :
             VideoProber.GetBaseHardwareParts()) as VideoPart[];

        /// <summary>
        /// Gets the list of hard disks
        /// </summary>
        public static HardDiskPart[] HardDisk =>
            (cachedParts.Keys.Contains(HardwarePartType.HardDisk) && cachedParts[HardwarePartType.HardDisk].Length > 0 ?
             cachedParts[HardwarePartType.HardDisk] :
             HardDiskProber.GetBaseHardwareParts()) as HardDiskPart[];

        /// <summary>
        /// Gets the list of hardware prober errors
        /// </summary>
        public static Exception[] Errors =>
            [.. errors];

        /// <summary>
        /// For Apple's code signing.
        /// </summary>
        /// <param name="notarized">If your application is using hardened macOS runtime, set this to true.</param>
        public static void SetNotarized(bool notarized) =>
            HardwareProber.notarized = notarized;
    }
}
