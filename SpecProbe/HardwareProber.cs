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
        private static readonly Dictionary<HardwarePartType, (BaseHardwarePartInfo[] parts, Exception[] errors)> cachedParts = [];

        /// <summary>
        /// Gets processor information
        /// </summary>
        public static ProcessorPart? GetProcessor()
        {
            if (cachedParts.Keys.Contains(HardwarePartType.Processor) && cachedParts[HardwarePartType.Processor].parts.Length > 0)
                return cachedParts[HardwarePartType.Processor].parts[0] as ProcessorPart;
            var part = ProcessorProber.Probe(out Exception[] errors);
            cachedParts.Add(HardwarePartType.Processor, ([part], errors));
            return part;
        }

        /// <summary>
        /// Gets memory information
        /// </summary>
        public static MemoryPart? GetMemory()
        {
            if (cachedParts.Keys.Contains(HardwarePartType.Memory) && cachedParts[HardwarePartType.Memory].parts.Length > 0)
                return cachedParts[HardwarePartType.Memory].parts[0] as MemoryPart;
            var part = MemoryProber.Probe(out Exception[] errors);
            cachedParts.Add(HardwarePartType.Memory, ([part], errors));
            return part;
        }

        /// <summary>
        /// Gets the list of video cards
        /// </summary>
        public static VideoPart[]? GetVideos()
        {
            if (cachedParts.Keys.Contains(HardwarePartType.Video) && cachedParts[HardwarePartType.Video].parts.Length > 0)
                return cachedParts[HardwarePartType.Video].parts as VideoPart[];
            var parts = VideoProber.Probe(out Exception[] errors);
            cachedParts.Add(HardwarePartType.Video, (parts, errors));
            return parts;
        }

        /// <summary>
        /// Gets the list of hard disks
        /// </summary>
        public static HardDiskPart[]? GetHardDisks()
        {
            if (cachedParts.Keys.Contains(HardwarePartType.HardDisk) && cachedParts[HardwarePartType.HardDisk].parts.Length > 0)
                return cachedParts[HardwarePartType.HardDisk].parts as HardDiskPart[];
            var parts = HardDiskProber.Probe(out Exception[] errors);
            cachedParts.Add(HardwarePartType.HardDisk, (parts, errors));
            return parts;
        }

        /// <summary>
        /// Gets hardware parsing errors for a specific hardware part type
        /// </summary>
        /// <param name="type">Hardware part type</param>
        /// <returns>An array of parse exceptions</returns>
        public static Exception[] GetParseExceptions(HardwarePartType type)
        {
            if (!cachedParts.TryGetValue(type, out var partsTuple))
                return [];
            return partsTuple.errors;
        }

        /// <summary>
        /// For Apple's code signing.
        /// </summary>
        /// <param name="notarized">If your application is using hardened macOS runtime, set this to true.</param>
        public static void SetNotarized(bool notarized) =>
            HardwareProber.notarized = notarized;
    }
}
