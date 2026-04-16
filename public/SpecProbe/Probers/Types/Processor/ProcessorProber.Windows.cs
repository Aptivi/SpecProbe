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
        public static ProcessorPart ProbeWindows(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            string[] features = [];
            int numberOfCores = 0;
            int numberOfCoresForEachCore = 0;
            int numberOfLogicalCores = 0;
            uint cacheL1 = 0;
            uint cacheL2 = 0;
            uint cacheL3 = 0;
            string name = "";
            string cpuidVendor = "";
            string vendor = "";
            string hypervisorVendor = "";
            double clockSpeed = 0.0;

            try
            {
                // First, the logical processor information (cores and caches)
                unsafe
                {
                    // Invoke the GetLogicalProcessorInformation() function to get necessary information. See the below link:
                    // https://learn.microsoft.com/en-gb/windows/win32/api/sysinfoapi/nf-sysinfoapi-getlogicalprocessorinformation
                    // ...for more information.
                    PlatformWindowsInterop.GetLogicalProcessorInformation(null, out int bufferSize);

                    // Get the number of entries and allocate info
                    int numEntries = bufferSize / sizeof(PlatformWindowsInterop.SYSTEM_LOGICAL_PROCESSOR_INFORMATION);
                    var coreInfo = new PlatformWindowsInterop.SYSTEM_LOGICAL_PROCESSOR_INFORMATION[numEntries];

                    // Iterate through every CPU info
                    fixed (PlatformWindowsInterop.SYSTEM_LOGICAL_PROCESSOR_INFORMATION* pCoreInfo = coreInfo)
                    {
                        // Try again...
                        PlatformWindowsInterop.GetLogicalProcessorInformation(pCoreInfo, out bufferSize);

                        // Now, we get to the actual info piece.
                        for (int i = 0; i < numEntries; ++i)
                        {
                            // Get the actual info
                            ref PlatformWindowsInterop.SYSTEM_LOGICAL_PROCESSOR_INFORMATION info = ref pCoreInfo[i];

                            // Now, adjust the variables based on the relationship values.
                            switch (info.Relationship)
                            {
                                case PlatformWindowsInterop.LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore:
                                    // Processor cores
                                    numberOfCores++;
                                    break;
                                case PlatformWindowsInterop.LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorPackage:
                                    // Processor cores for each core
                                    numberOfCoresForEachCore++;
                                    break;
                                case PlatformWindowsInterop.LOGICAL_PROCESSOR_RELATIONSHIP.RelationCache:
                                    // Processor L1, L2, and L3 cache
                                    var cacheInfo = info.ProcessorInformation.Cache;
                                    switch (cacheInfo.Level)
                                    {
                                        case 1:
                                            // L1 cache
                                            cacheL1 += cacheInfo.Size;
                                            break;
                                        case 2:
                                            // L2 cache
                                            cacheL2 += cacheInfo.Size;
                                            break;
                                        case 3:
                                            // L3 cache
                                            cacheL3 += cacheInfo.Size;
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                }

                // Then, the processor name, vendor, and speed
                if (!PlatformHelper.IsOnArmOrArm64())
                {
                    Initializer.InitializeNative();
                    var cpuNameDelegate = ProcessorHelper.GetCpuNameDelegate();
                    var vendorDelegate = ProcessorHelper.GetVendorDelegate();
                    cpuidVendor = Marshal.PtrToStringAnsi(vendorDelegate.Invoke());
                    name = Marshal.PtrToStringAnsi(cpuNameDelegate.Invoke());
                    vendor = ProcessorCpuid.MapRealVendorFromCpuid(cpuidVendor);
                    PlatformWindowsInterop.PROCESSOR_POWER_INFORMATION[] procInfo = new PlatformWindowsInterop.PROCESSOR_POWER_INFORMATION[numberOfCores * numberOfCoresForEachCore];
                    uint powerBufferSize = (uint)(procInfo.Length * Marshal.SizeOf(typeof(PlatformWindowsInterop.PROCESSOR_POWER_INFORMATION)));
                    uint status = PlatformWindowsInterop.CallNtPowerInformation(11, IntPtr.Zero, 0, procInfo, powerBufferSize);
                    if (status == 0)
                        clockSpeed = procInfo[0].MaxMhz;
                }

                // Then, the features
                if (!PlatformHelper.IsOnArmOrArm64())
                {
                    features = ProcessorCpuid.PopulateFeatures();
                    if (features.Contains("hypervisor"))
                        hypervisorVendor = ProcessorCpuid.DetectHypervisor();
                }

                // Finally, get the actual logical processor count
                PlatformWindowsInterop.GetSystemInfo(out PlatformWindowsInterop.SYSTEM_INFO system);
                numberOfLogicalCores = (int)system.dwNumberOfProcessors;
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
                ProcessorCores = numberOfCores,
                Cores = numberOfCoresForEachCore,
                LogicalCores = numberOfLogicalCores,
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
