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
using SpecProbe.Pci;
using SpecProbe.Software.Platform;
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
            SeparatorWriterColor.WriteSeparatorColor("Processor information", 15);
            stopwatch.Start();
            totalStopwatch.Start();
            var processor = HardwareProber.GetProcessor();
            var processorErrors = HardwareProber.GetParseExceptions(HardwarePartType.Processor);
            stopwatch.Stop();
            TextWriterColor.WriteColor("- Processor cores: ", false, 3);
            TextWriterColor.WriteColor($"{processor.ProcessorCores}", true, 8);
            TextWriterColor.WriteColor("- Cores for each core: ", false, 3);
            TextWriterColor.WriteColor($"{processor.Cores}", true, 8);
            TextWriterColor.WriteColor("- Logical cores: ", false, 3);
            TextWriterColor.WriteColor($"{processor.LogicalCores}", true, 8);
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
            TextWriterRaw.Write();
            foreach (var exc in processorErrors)
            {
                TextWriterColor.WriteColor("Error: ", false, 3);
                TextWriterColor.WriteColor($"{exc.Message}", true, 8);
                TextWriterColor.WriteColor($"{exc.StackTrace}", true, 8);
            }
            TextWriterColor.WriteColor("Total time taken to parse: ", false, 3);
            TextWriterColor.WriteColor($"{stopwatch.Elapsed}", true, 8);
            TextWriterRaw.Write();
            stopwatch.Reset();

            // Memory
            SeparatorWriterColor.WriteSeparator("Memory information", true, 15);
            stopwatch.Start();
            var memory = HardwareProber.GetMemory();
            var memoryErrors = HardwareProber.GetParseExceptions(HardwarePartType.Memory);
            stopwatch.Stop();
            TextWriterColor.WriteColor("- Total memory (system): ", false, 3);
            TextWriterColor.WriteColor($"{memory.TotalMemory}", true, 8);
            TextWriterColor.WriteColor("- Total memory (real): ", false, 3);
            TextWriterColor.WriteColor($"{memory.TotalPhysicalMemory}", true, 8);
            TextWriterColor.WriteColor("- Reserved memory: ", false, 3);
            TextWriterColor.WriteColor($"{memory.SystemReservedMemory}", true, 8);
            TextWriterRaw.Write();
            foreach (var exc in memoryErrors)
            {
                TextWriterColor.WriteColor("Error: ", false, 3);
                TextWriterColor.WriteColor($"{exc.Message}", true, 8);
                TextWriterColor.WriteColor($"{exc.StackTrace}", true, 8);
            }
            TextWriterColor.WriteColor("Total time taken to parse: ", false, 3);
            TextWriterColor.WriteColor($"{stopwatch.Elapsed}", true, 8);
            TextWriterRaw.Write();
            stopwatch.Reset();

            // Video
            SeparatorWriterColor.WriteSeparator("Video information", true, 15);
            stopwatch.Start();
            var videoParts = HardwareProber.GetVideos();
            var videoErrors = HardwareProber.GetParseExceptions(HardwarePartType.Video);
            stopwatch.Stop();
            foreach (var video in videoParts)
            {
                TextWriterColor.WriteColor("- Video card name: ", false, 3);
                TextWriterColor.WriteColor($"{video.VideoCardName}", true, 8);
                TextWriterColor.WriteColor("- Video card vendor ID: ", false, 3);
                TextWriterColor.WriteColor($"{video.VendorId} [0x{video.VendorId:X4}]", true, 8);
                TextWriterColor.WriteColor("- Video card model ID: ", false, 3);
                TextWriterColor.WriteColor($"{video.ModelId} [0x{video.ModelId:X4}]", true, 8);
            }
            TextWriterRaw.Write();
            foreach (var exc in videoErrors)
            {
                TextWriterColor.WriteColor("Error: ", false, 3);
                TextWriterColor.WriteColor($"{exc.Message}", true, 8);
                TextWriterColor.WriteColor($"{exc.StackTrace}", true, 8);
            }
            TextWriterColor.WriteColor("Total time taken to parse: ", false, 3);
            TextWriterColor.WriteColor($"{stopwatch.Elapsed}", true, 8);
            TextWriterRaw.Write();
            stopwatch.Reset();

            // Hard drive
            SeparatorWriterColor.WriteSeparator("Hard drive information", true, 15);
            stopwatch.Start();
            var hardDiskParts = HardwareProber.GetHardDisks();
            var hardDiskErrors = HardwareProber.GetParseExceptions(HardwarePartType.HardDisk);
            stopwatch.Stop();
            foreach (var hardDisk in hardDiskParts)
            {
                TextWriterColor.WriteColor("- Hard drive size: ", false, 3);
                TextWriterColor.WriteColor($"{hardDisk.HardDiskSize}", true, 8);
                TextWriterColor.WriteColor("- Partition count: ", false, 3);
                TextWriterColor.WriteColor($"{hardDisk.PartitionCount}", true, 8);
                TextWriterColor.WriteColor("- Partition table type: ", false, 3);
                TextWriterColor.WriteColor($"{hardDisk.PartitionTableType}", true, 8);
                for (int i = 0; i < hardDisk.Partitions.Length; i++)
                {
                    HardDiskPart.PartitionPart partition = hardDisk.Partitions[i];
                    TextWriterColor.WriteColor("--- Partition number (real): ", false, 3);
                    TextWriterColor.WriteColor($"{i + 1}", true, 8);
                    TextWriterColor.WriteColor("  - Partition number (OS): ", false, 3);
                    TextWriterColor.WriteColor($"{partition.PartitionNumber}", true, 8);
                    TextWriterColor.WriteColor("  - Partition size: ", false, 3);
                    TextWriterColor.WriteColor($"{partition.PartitionSize}", true, 8);
                    TextWriterColor.WriteColor("  - Partition offset: ", false, 3);
                    TextWriterColor.WriteColor($"{partition.PartitionOffset} -> {partition.PartitionOffsetTo}", true, 8);
                    TextWriterColor.WriteColor("  - Partition type: ", false, 3);
                    TextWriterColor.WriteColor($"{partition.PartitionType}", true, 8);
                    TextWriterColor.WriteColor("  - Partition bootable? ", false, 3);
                    TextWriterColor.WriteColor($"{partition.PartitionBootable}", true, 8);
                }
            }
            TextWriterRaw.Write();
            foreach (var exc in hardDiskErrors)
            {
                TextWriterColor.WriteColor("Error: ", false, 3);
                TextWriterColor.WriteColor($"{exc.Message}", true, 8);
                TextWriterColor.WriteColor($"{exc.StackTrace}", true, 8);
            }
            TextWriterColor.WriteColor("Total time taken to parse: ", false, 3);
            TextWriterColor.WriteColor($"{stopwatch.Elapsed}", true, 8);
            TextWriterRaw.Write();
            stopwatch.Reset();

            // RID graph
            SeparatorWriterColor.WriteSeparator("RID graph", true, 15);
            stopwatch.Start();
            string[] graph = RidGraphReader.GetGraphFromRid();
            stopwatch.Stop();
            TextWriterColor.WriteColor("- Found RIDs: ", false, 3);
            TextWriterColor.WriteColor($"{string.Join(", ", graph)}", true, 8);
            TextWriterRaw.Write();
            TextWriterColor.WriteColor("Total time taken to parse: ", false, 3);
            TextWriterColor.WriteColor($"{stopwatch.Elapsed}", true, 8);
            TextWriterRaw.Write();
            stopwatch.Reset();

            // WSL parse
            SeparatorWriterColor.WriteSeparator("Running on WSL?", true, 15);
            stopwatch.Start();
            bool wsl = PlatformHelper.IsOnUnixWsl();
            stopwatch.Stop();
            TextWriterColor.WriteColor("- WSL: ", false, 3);
            TextWriterColor.WriteColor($"{wsl}", true, 8);
            TextWriterRaw.Write();
            TextWriterColor.WriteColor("Total time taken to parse: ", false, 3);
            TextWriterColor.WriteColor($"{stopwatch.Elapsed}", true, 8);
            TextWriterRaw.Write();
            stopwatch.Reset();

            // PCI ID parse
            SeparatorWriterColor.WriteSeparator("PCI ID", true, 15);
            stopwatch.Start();
            var vendors = PciListParser.ListVendors();
            var vendor = PciListParser.GetVendor(0x1000);
            var devices = PciListParser.ListDevices(0x1000);
            var device = PciListParser.GetDevice(0x1000, 0x0014);
            var subDevices = PciListParser.ListSubDevices(0x1000, 0x0014);
            var subDevice = PciListParser.GetSubDevice(0x1000, 0x0014, 0x1d49, 0x0602);
            stopwatch.Stop();
            TextWriterColor.WriteColor("- Vendor count: ", false, 3);
            TextWriterColor.WriteColor($"{vendors.Length} vendors", true, 8);
            TextWriterColor.WriteColor($"- Device count for {vendor.Name} [0x{vendor.Id:x4}]: ", false, 3);
            TextWriterColor.WriteColor($"{devices.Length} devices", true, 8);
            TextWriterColor.WriteColor($"- Sub-device count for {device.Name} [0x{device.Id:x4}]: ", false, 3);
            TextWriterColor.WriteColor($"{subDevices.Length} devices", true, 8);
            TextWriterColor.WriteColor($"- Sub-device of 0x{device.Id:x4}: ", false, 3);
            TextWriterColor.WriteColor($"{subDevice.Name}", true, 8);
            TextWriterRaw.Write();
            TextWriterColor.WriteColor("Total time taken to parse: ", false, 3);
            TextWriterColor.WriteColor($"{stopwatch.Elapsed}", true, 8);
            TextWriterRaw.Write();
            stopwatch.Reset();

            totalStopwatch.Stop();
            SeparatorWriterColor.WriteSeparator("Conclusion", true, 15);
            TextWriterColor.WriteColor("Total time: ", false, 3);
            TextWriterColor.WriteColor($"{totalStopwatch.Elapsed}", true, 8);
            TextWriterRaw.Write();
            totalStopwatch.Reset();
        }
    }
}
