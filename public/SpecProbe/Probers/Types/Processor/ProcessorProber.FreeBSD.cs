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
using System.Linq;
using System.Runtime.InteropServices;

namespace SpecProbe.Probers.Types.Processor
{
    internal static partial class ProcessorProber
    {
        public static ProcessorPart ProbeFreeBSD(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            string[] features = [];
            int numberOfCores = 0;
            int numberOfCoresForEachCore = 1;
            uint cacheL1 = 0;
            uint cacheL2 = 0;
            uint cacheL3 = 0;
            string name = "";
            string cpuidVendor = "";
            string vendor = "";
            string hypervisorVendor = "";
            double clockSpeed = 0.0;

            // Some constants
            const string physicalId = "kern.smp.cores: ";
            const string cpuCores = "kern.smp.threads_per_core: ";
            const string cpuClockSpeed = "machdep.tsc_freq: ";
            const string vendorId = "machdep.cpu.vendor: ";
            const string modelId = "hw.model: ";

            try
            {
                // First, get the vendor information from the SpecProber if not running on ARM
                if (!PlatformHelper.IsOnArmOrArm64())
                {
                    Initializer.InitializeNative();
                    var cpuNameDelegate = ProcessorHelper.GetCpuNameDelegate();
                    var vendorDelegate = ProcessorHelper.GetVendorDelegate();
                    cpuidVendor = Marshal.PtrToStringAnsi(vendorDelegate.Invoke());
                    name = Marshal.PtrToStringAnsi(cpuNameDelegate.Invoke());
                    vendor = ProcessorCpuid.MapRealVendorFromCpuid(cpuidVendor);
                }

                // Then, the features
                if (!PlatformHelper.IsOnArmOrArm64())
                {
                    features = ProcessorCpuid.PopulateFeatures();
                    if (features.Contains("hypervisor"))
                        hypervisorVendor = ProcessorCpuid.DetectHypervisor();
                }

                // Then, fill the rest
                string sysctlOutput = PlatformHelper.ExecuteProcessToString("/sbin/sysctl", "kern.smp.cores kern.smp.threads_per_core machdep.tsc_freq machdep.cpu.vendor hw.model");
                string[] sysctlOutputLines = sysctlOutput.Replace("\r", "").Split('\n');
                foreach (string sysctlOutputLine in sysctlOutputLines)
                {
                    if (sysctlOutputLine.StartsWith(physicalId))
                        numberOfCores = int.Parse(sysctlOutputLine.Substring(physicalId.Length));
                    if (sysctlOutputLine.StartsWith(cpuCores))
                        numberOfCoresForEachCore = int.Parse(sysctlOutputLine.Substring(cpuCores.Length));
                    if (sysctlOutputLine.StartsWith(cpuClockSpeed))
                        clockSpeed = double.Parse(sysctlOutputLine.Substring(cpuClockSpeed.Length)) / 1000 / 1000;
                    if (sysctlOutputLine.StartsWith(vendorId) && string.IsNullOrEmpty(cpuidVendor))
                    {
                        cpuidVendor = sysctlOutputLine.Substring(vendorId.Length);
                        vendor = ProcessorCpuid.MapRealVendorFromCpuid(cpuidVendor);
                    }
                    if (sysctlOutputLine.StartsWith(modelId) && string.IsNullOrEmpty(name))
                        name = sysctlOutputLine.Substring(modelId.Length);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            // Get the real hypervisor vendor
            string realHvVendor = ProcessorCpuid.MapRealHvVendorFromCpuid(hypervisorVendor);

            // Get the cache sizes
            if (!PlatformHelper.IsOnArmOrArm64())
                (cacheL1, cacheL2, cacheL3) = ProcessorCpuid.GetCacheLevelSizes(vendor);

            // Finally, return a single item array containing processor information
            ProcessorPart processorPart = new()
            {
                ProcessorCores = numberOfCores,
                Cores = numberOfCoresForEachCore,
                LogicalCores = numberOfCoresForEachCore * numberOfCores,
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
