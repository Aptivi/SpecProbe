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

using SpecProbe.Loader.Native;
using SpecProbe.Loader.Native.Helpers;
using SpecProbe.Parts.Types;
using SpecProbe.Probers.Platform;
using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SpecProbe.Probers.Types.Processor
{
    internal static partial class ProcessorProber
    {
        public static ProcessorPart ProbeLinux(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            int numberOfLogicalCores = 0;
            int numberOfPhysicalCores = 0;
            int numberOfCoresForEachCore = 1;
            uint cacheL1 = 0;
            uint cacheL2 = 0;
            uint cacheL3 = 0;
            string name = "ARM Processor";
            string cpuidVendor = "";
            string vendor = "";
            string hypervisorVendor = "";
            double clockSpeed = 0.0;
            string[] features = [];

            // ARM-specific variables
            List<string> armVendorIds = [];
            List<string> armPartIds = [];

            // Some constants
            const string physicalId = "physical id\t: ";
            const string logicalId = "processor\t: ";
            const string cpuCores = "cpu cores\t: ";
            const string cpuClockSpeed = "cpu MHz\t\t: ";
            const string vendorId = "vendor_id\t: ";
            const string modelId = "model name\t: ";
            const string processorNum = "processor\t: ";
            const string armProcessorName = "Processor\t: ";
            const string armVendorName = "CPU implementer\t: ";
            const string armPartName = "CPU part\t: ";

            try
            {
                // Get the features
                if (!PlatformHelper.IsOnArmOrArm64())
                {
                    features = ProcessorCpuid.PopulateFeatures();
                    if (features.Contains("hypervisor"))
                        hypervisorVendor = ProcessorCpuid.DetectHypervisor();
                }

                // Open the cpuinfo file
                string cpuInfoFile = "/proc/cpuinfo";
                string[] cpuInfoContents = File.ReadAllLines(cpuInfoFile);
                foreach (string cpuInfoLine in cpuInfoContents)
                {
                    if (PlatformHelper.IsOnArmOrArm64())
                    {
                        // Get the processor number
                        if (cpuInfoLine.StartsWith(processorNum))
                        {
                            string logicalIdString = cpuInfoLine.Replace(processorNum, "");
                            int logicalIdNum = int.Parse(logicalIdString);
                            numberOfLogicalCores = logicalIdNum + 1;
                        }

                        // Get the processor name
                        if (cpuInfoLine.StartsWith(armProcessorName))
                            name = cpuInfoLine.Replace(armProcessorName, "");

                        // Get the processor vendor
                        if (cpuInfoLine.StartsWith(armVendorName))
                            armVendorIds.Add(cpuInfoLine.Replace(armVendorName, "").Substring(2));

                        // Get the processor part
                        if (cpuInfoLine.StartsWith(armPartName))
                            armPartIds.Add(cpuInfoLine.Replace(armPartName, "").Substring(2));
                    }
                    else
                    {
                        // Get the processor physical ID index and parse it to the number of cores
                        if (cpuInfoLine.StartsWith(physicalId))
                        {
                            string physicalIdString = cpuInfoLine.Replace(physicalId, "");
                            int physicalIdNum = int.Parse(physicalIdString);
                            numberOfPhysicalCores = physicalIdNum + 1;
                        }

                        // Get the number of logical processors
                        if (cpuInfoLine.StartsWith(logicalId))
                        {
                            string logicalIdString = cpuInfoLine.Replace(logicalId, "");
                            int logicalIdNum = int.Parse(logicalIdString);
                            numberOfLogicalCores = logicalIdNum + 1;
                        }

                        // Get the number of cores
                        if (cpuInfoLine.StartsWith(cpuCores))
                        {
                            string coreNumString = cpuInfoLine.Replace(cpuCores, "");
                            numberOfCoresForEachCore = int.Parse(coreNumString);
                        }

                        // Get the clock speed
                        if (cpuInfoLine.StartsWith(cpuClockSpeed))
                        {
                            string clockString = cpuInfoLine.Replace(cpuClockSpeed, "");
                            clockSpeed = double.Parse(clockString);
                        }

                        // Get the name and the vendor
                        if (cpuInfoLine.StartsWith(vendorId))
                        {
                            Initializer.InitializeNative();
                            var vendorDelegate = ProcessorHelper.GetVendorDelegate();
                            cpuidVendor = Marshal.PtrToStringAnsi(vendorDelegate.Invoke());
                            if (string.IsNullOrEmpty(cpuidVendor))
                                cpuidVendor = cpuInfoLine.Replace(vendorId, "");
                            vendor = ProcessorCpuid.MapRealVendorFromCpuid(cpuidVendor);
                        }
                        if (cpuInfoLine.StartsWith(modelId))
                        {
                            Initializer.InitializeNative();
                            var cpuNameDelegate = ProcessorHelper.GetCpuNameDelegate();
                            name = Marshal.PtrToStringAnsi(cpuNameDelegate.Invoke());
                            if (string.IsNullOrEmpty(name))
                                name = cpuInfoLine.Replace(modelId, "");
                        }
                    }
                }

                // Open the cache list to get cache sizes in kilobytes
                if (!PlatformHelper.IsOnArmOrArm64())
                {
                    string cacheFolder = "/sys/devices/system/cpu/cpu0/cache";
                    string cacheFolderL1 = $"{cacheFolder}/index1";
                    string cacheFolderL2 = $"{cacheFolder}/index2";
                    string cacheFolderL3 = $"{cacheFolder}/index3";
                    if (Directory.Exists(cacheFolderL1))
                    {
                        // Get the size in kilobytes
                        string fullPath = $"{cacheFolderL1}/size";
                        string cacheKilobytes = File.ReadAllLines(fullPath)[0];
                        cacheKilobytes = cacheKilobytes.Remove(cacheKilobytes.Length - 1);

                        // Convert this size to bytes
                        cacheL1 = uint.Parse(cacheKilobytes) * 1024;
                    }
                    if (Directory.Exists(cacheFolderL2))
                    {
                        // Get the size in kilobytes
                        string fullPath = $"{cacheFolderL2}/size";
                        string cacheKilobytes = File.ReadAllLines(fullPath)[0];
                        cacheKilobytes = cacheKilobytes.Remove(cacheKilobytes.Length - 1);

                        // Convert this size to bytes
                        cacheL2 = uint.Parse(cacheKilobytes) * 1024;
                    }
                    if (Directory.Exists(cacheFolderL3))
                    {
                        // Get the size in kilobytes
                        string fullPath = $"{cacheFolderL3}/size";
                        string cacheKilobytes = File.ReadAllLines(fullPath)[0];
                        cacheKilobytes = cacheKilobytes.Remove(cacheKilobytes.Length - 1);

                        // Convert this size to bytes
                        cacheL3 = uint.Parse(cacheKilobytes) * 1024;
                    }
                }

                // If running on ARM, get the CPU implementer and part names
                if (PlatformHelper.IsOnArmOrArm64())
                {
                    // Get the implementer and the part
                    List<string> implementerList = [];
                    List<string> partList = [];
                    foreach (var implementer in ArmImplementers.implementers)
                    {
                        if (implementer == null)
                            continue;
                        if (armVendorIds.Contains($"{implementer.id:x2}"))
                        {
                            if (!implementerList.Contains(implementer.name))
                                implementerList.Add(implementer.name);
                            foreach (var part in implementer.parts)
                            {
                                if (armPartIds.Contains($"{part.id:x2}"))
                                {
                                    if (!partList.Contains(part.name))
                                        partList.Add(part.name);
                                }
                            }
                        }
                    }

                    // Form the CPUID vendor and the name
                    cpuidVendor = vendor = string.Join(", ", implementerList);
                    name = string.Join(", ", partList);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            
            // Get the real hypervisor vendor
            string realHvVendor = ProcessorCpuid.MapRealHvVendorFromCpuid(hypervisorVendor);

            // Finally, return a single item array containing processor information
            ProcessorPart processorPart = new()
            {
                ProcessorCores = numberOfPhysicalCores,
                LogicalCores = numberOfLogicalCores,
                Cores = numberOfCoresForEachCore,
                L1CacheSize = cacheL1,
                L2CacheSize = cacheL2,
                L3CacheSize = cacheL3,
                Name = name,
                CpuidVendor = cpuidVendor,
                Vendor = vendor,
                Speed = clockSpeed,
                CpuidHypervisorVendor = hypervisorVendor,
                HypervisorVendor = realHvVendor,
                Hypervisor = features.Contains("hypervisor"),
                Flags = features,
            };
            errors = [.. exceptions];
            return processorPart;
        }
    }
}
