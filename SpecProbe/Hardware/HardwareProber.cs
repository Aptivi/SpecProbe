
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

using SpecProbe.Hardware.Parts.Types;
using SpecProbe.Hardware.Probers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpecProbe.Hardware
{
    /// <summary>
    /// Hardware probing class
    /// </summary>
    public static class HardwareProber
    {
        internal static bool notarized = false;
        internal static List<Exception> errors = new();
        private static ProcessorPart[] cachedProcessors;
        private static MemoryPart[] cachedMemory;
        private static VideoPart[] cachedVideos;
        private static HardDiskPart[] cachedHardDisks;
        private static readonly ProcessorProber processorProber = new();
        private static readonly MemoryProber memoryProber = new();
        private static readonly VideoProber videoProber = new();
        private static readonly HardDiskProber hardDiskProber = new();

        /// <summary>
        /// Gets the list of processors (always 1)
        /// </summary>
        public static ProcessorPart[] Processors =>
            cachedProcessors is not null ? cachedProcessors : ProbeProcessors();

        /// <summary>
        /// Gets the list of memory (always 1)
        /// </summary>
        public static MemoryPart[] Memory =>
            cachedMemory is not null ? cachedMemory : ProbeMemory();

        /// <summary>
        /// Gets the list of video cards
        /// </summary>
        public static VideoPart[] Video =>
            cachedVideos is not null ? cachedVideos : ProbeVideo();

        /// <summary>
        /// Gets the list of hard disks
        /// </summary>
        public static HardDiskPart[] HardDisk =>
            cachedHardDisks is not null ? cachedHardDisks : ProbeHardDisk();

        /// <summary>
        /// Gets the list of hardware prober errors
        /// </summary>
        public static Exception[] Errors =>
            errors.ToArray();

        /// <summary>
        /// For Apple's code signing.
        /// </summary>
        /// <param name="notarized">If your application is using hardened macOS runtime, set this to true.</param>
        public static void SetNotarized(bool notarized) =>
            HardwareProber.notarized = notarized;

        private static ProcessorPart[] ProbeProcessors()
        {
            // Get the base part class instances from the part prober
            var processorBases = processorProber.GetBaseHardwareParts();
            var processors = processorBases.Cast<ProcessorPart>();
            cachedProcessors = processors.ToArray();
            return cachedProcessors;
        }

        private static MemoryPart[] ProbeMemory()
        {
            // Get the base part class instances from the part prober
            var memoryBases = memoryProber.GetBaseHardwareParts();
            var memory = memoryBases.Cast<MemoryPart>();
            cachedMemory = memory.ToArray();
            return cachedMemory;
        }

        private static VideoPart[] ProbeVideo()
        {
            // Get the base part class instances from the part prober
            var videoBases = videoProber.GetBaseHardwareParts();
            var videos = videoBases.Cast<VideoPart>();
            cachedVideos = videos.ToArray();
            return cachedVideos;
        }

        private static HardDiskPart[] ProbeHardDisk()
        {
            // Get the base part class instances from the part prober
            var hardDiskBases = hardDiskProber.GetBaseHardwareParts();
            var hardDisks = hardDiskBases.Cast<HardDiskPart>();
            cachedHardDisks = hardDisks.ToArray();
            return cachedHardDisks;
        }
    }
}
