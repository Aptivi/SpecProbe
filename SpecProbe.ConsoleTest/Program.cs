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

using SpecProbe.Hardware;
using SpecProbe.Hardware.Parts.Types;
using System.Diagnostics;
using Terminaux.Writer.ConsoleWriters;
using Terminaux.Writer.FancyWriters;

namespace SpecProbe.ConsoleTest
{

    static class Program
    {
        public static void Main()
        {
            // Stopwatch for measurement
            var stopwatch = new Stopwatch();
            var totalStopwatch = new Stopwatch();

            // Processor
            SeparatorWriterColor.WriteSeparatorColor("Processor information", true, 15);
            stopwatch.Start();
            totalStopwatch.Start();
            var processors = HardwareProber.Processors;
            stopwatch.Stop();
            foreach (var processor in processors)
            {
                TextWriterColor.WriteColor("- Processor cores: ", false, 3);
                TextWriterColor.WriteColor($"{processor.ProcessorCores}", true, 8);
                TextWriterColor.WriteColor("- Cores for each core: ", false, 3);
                TextWriterColor.WriteColor($"{processor.CoresForEachCore}", true, 8);
                TextWriterColor.WriteColor("- Total cores: ", false, 3);
                TextWriterColor.WriteColor($"{processor.TotalCores}", true, 8);
                TextWriterColor.WriteColor("- L1, L2, L3 cache sizes in bytes: ", false, 3);
                TextWriterColor.WriteColor($"{processor.L1CacheSize}, {processor.L2CacheSize}, {processor.L3CacheSize}", true, 8);
                TextWriterColor.WriteColor("- Name: ", false, 3);
                TextWriterColor.WriteColor($"{processor.Name}", true, 8);
                TextWriterColor.WriteColor("- Vendor (CPUID): ", false, 3);
                TextWriterColor.WriteColor($"{processor.CpuidVendor}", true, 8);
                TextWriterColor.WriteColor("- Vendor (Real): ", false, 3);
                TextWriterColor.WriteColor($"{processor.Vendor}", true, 8);
                TextWriterColor.WriteColor("- Clock speed: ", false, 3);
                TextWriterColor.WriteColor($"{processor.Speed}", true, 8);
            }
            TextWriterColor.WriteColor("Total time taken to parse: ", false, 3);
            TextWriterColor.WriteColor($"{stopwatch.Elapsed}", true, 8);
            TextWriterColor.Write();
            stopwatch.Reset();

            // Memory
            SeparatorWriterColor.WriteSeparator("Memory information", true, 15);
            stopwatch.Start();
            var memoryParts = HardwareProber.Memory;
            stopwatch.Stop();
            foreach (var memory in memoryParts)
            {
                TextWriterColor.WriteColor("- Total memory (system): ", false, 3);
                TextWriterColor.WriteColor($"{memory.TotalMemory}", true, 8);
                TextWriterColor.WriteColor("- Total memory (real): ", false, 3);
                TextWriterColor.WriteColor($"{memory.TotalPhysicalMemory}", true, 8);
                TextWriterColor.WriteColor("- Reserved memory: ", false, 3);
                TextWriterColor.WriteColor($"{memory.SystemReservedMemory}", true, 8);
            }
            TextWriterColor.WriteColor("Total time taken to parse: ", false, 3);
            TextWriterColor.WriteColor($"{stopwatch.Elapsed}", true, 8);
            TextWriterColor.Write();
            stopwatch.Reset();

            // Video
            SeparatorWriterColor.WriteSeparator("Video information", true, 15);
            stopwatch.Start();
            var videoParts = HardwareProber.Video;
            stopwatch.Stop();
            foreach (var video in videoParts)
            {
                TextWriterColor.WriteColor("- Video card name: ", false, 3);
                TextWriterColor.WriteColor($"{video.VideoCardName}", true, 8);
            }
            TextWriterColor.WriteColor("Total time taken to parse: ", false, 3);
            TextWriterColor.WriteColor($"{stopwatch.Elapsed}", true, 8);
            TextWriterColor.Write();
            stopwatch.Reset();

            // Hard drive
            SeparatorWriterColor.WriteSeparator("Hard drive information", true, 15);
            stopwatch.Start();
            var hardDiskParts = HardwareProber.HardDisk;
            stopwatch.Stop();
            foreach (var hardDisk in hardDiskParts)
            {
                TextWriterColor.WriteColor("- Hard drive size: ", false, 3);
                TextWriterColor.WriteColor($"{hardDisk.HardDiskSize}", true, 8);
                TextWriterColor.WriteColor("- Partition count: ", false, 3);
                TextWriterColor.WriteColor($"{hardDisk.PartitionCount}", true, 8);
                for (int i = 0; i < hardDisk.Partitions.Length; i++)
                {
                    HardDiskPart.PartitionPart partition = hardDisk.Partitions[i];
                    TextWriterColor.WriteColor("  - Partition number (real): ", false, 3);
                    TextWriterColor.WriteColor($"{i + 1}", true, 8);
                    TextWriterColor.WriteColor("  - Partition number (OS): ", false, 3);
                    TextWriterColor.WriteColor($"{partition.PartitionNumber}", true, 8);
                    TextWriterColor.WriteColor("  - Partition size: ", false, 3);
                    TextWriterColor.WriteColor($"{partition.PartitionSize}", true, 8);
                }
            }
            TextWriterColor.WriteColor("Total time taken to parse: ", false, 3);
            TextWriterColor.WriteColor($"{stopwatch.Elapsed}", true, 8);
            TextWriterColor.Write();
            stopwatch.Reset();

            totalStopwatch.Stop();
            TextWriterColor.WriteColor("Total time: ", false, 3);
            TextWriterColor.WriteColor($"{totalStopwatch.Elapsed}", true, 8);
            TextWriterColor.Write();
            totalStopwatch.Reset();

            // List errors
            foreach (var exc in HardwareProber.Errors)
            {
                TextWriterColor.WriteColor("Error: ", false, 3);
                TextWriterColor.WriteColor($"{exc.Message}", true, 8);
                TextWriterColor.WriteColor($"{exc.StackTrace}", true, 8);
            }
        }
    }
}
