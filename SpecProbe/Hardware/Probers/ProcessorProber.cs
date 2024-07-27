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

using SpecProbe.Hardware.Parts;
using SpecProbe.Hardware.Parts.Types;
using SpecProbe.Native;
using SpecProbe.Native.Helpers;
using SpecProbe.Platform;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace SpecProbe.Hardware.Probers
{
    internal class ProcessorProber : IHardwareProber
    {
        public BaseHardwarePartInfo[] GetBaseHardwareParts()
        {
            if (PlatformHelper.IsOnWindows())
                return GetBaseHardwarePartsWindows();
            else if (PlatformHelper.IsOnMacOS())
                return GetBaseHardwarePartsMacOS();
            else
                return GetBaseHardwarePartsLinux();
        }

        public BaseHardwarePartInfo[] GetBaseHardwarePartsLinux()
        {
            // Some variables to install.
            int numberOfCores = 0;
            int numberOfCoresForEachCore = 1;
            uint cacheL1 = 0;
            uint cacheL2 = 0;
            uint cacheL3 = 0;
            string name = "";
            string cpuidVendor = "";
            double clockSpeed = 0.0;

            // Some constants
            const string physicalId = "physical id\t: ";
            const string cpuCores = "cpu cores\t: ";
            const string cpuClockSpeed = "cpu MHz\t\t: ";
            const string vendorId = "vendor_id\t: ";
            const string modelId = "model name\t: ";
            const string processorNum = "processor\t: ";
            const string armProcessorName = "Processor\t: ";
            const string armVendorName = "CPU implementer\t: ";

            try
            {
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
                            string physicalIdString = cpuInfoLine.Replace(processorNum, "");
                            int physicalIdNum = int.Parse(physicalIdString);
                            numberOfCores = physicalIdNum + 1;
                        }

                        // Get the processor name
                        if (cpuInfoLine.StartsWith(armProcessorName))
                            name = cpuInfoLine.Replace(armProcessorName, "");

                        // Get the processor vendor
                        if (cpuInfoLine.StartsWith(armVendorName))
                        {
                            string armVendorId = cpuInfoLine.Replace(armVendorName, "");
                            armVendorId = armVendorId.Substring(2);
                            int armVendorIdInt = int.Parse(armVendorId, NumberStyles.AllowHexSpecifier);
                            cpuidVendor = armVendorIdInt switch
                            {
                                0x41 => "ARM",
                                _    => "",
                            };
                            if (string.IsNullOrEmpty(name))
                                name = "ARM Processor";
                        }
                    }
                    else
                    {
                        // Get the processor physical ID index and parse it to the number of cores
                        if (cpuInfoLine.StartsWith(physicalId))
                        {
                            string physicalIdString = cpuInfoLine.Replace(physicalId, "");
                            int physicalIdNum = int.Parse(physicalIdString);
                            numberOfCores = physicalIdNum + 1;
                        }

                        // Get the number of cores for each physical processor
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
            }
            catch (Exception ex)
            {
                HardwareProber.errors.Add(ex);
            }

            // Finally, return a single item array containing processor information
            ProcessorPart processorPart = new()
            {
                ProcessorCores = numberOfCores,
                CoresForEachCore = numberOfCoresForEachCore,
                L1CacheSize = cacheL1,
                L2CacheSize = cacheL2,
                L3CacheSize = cacheL3,
                Name = name,
                CpuidVendor = cpuidVendor,
                Speed = clockSpeed,
            };
            return new[] { processorPart };
        }

        public BaseHardwarePartInfo[] GetBaseHardwarePartsMacOS()
        {
            // Some variables to install.
            int numberOfCores = 0;
            int numberOfCoresForEachCore = 1;
            uint cacheL1 = 0;
            uint cacheL2 = 0;
            uint cacheL3 = 0;
            string name = "";
            string cpuidVendor = "";
            double clockSpeed = 0.0;

            // Some constants
            const string physicalId = "machdep.cpu.core_count: ";
            const string cpuCores = "machdep.cpu.cores_per_package: ";
            const string cpuClockSpeed = "hw.cpufrequency: ";
            const string vendorId = "machdep.cpu.vendor: ";
            const string modelId = "machdep.cpu.brand_string: ";
            const string l1Name = "hw.l1icachesize: ";
            const string l2Name = "hw.l2cachesize: ";

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
                }

                // Then, fill the rest
                string sysctlOutput = PlatformHelper.ExecuteProcessToString("/usr/sbin/sysctl", "machdep.cpu.core_count machdep.cpu.cores_per_package hw.cpufrequency machdep.cpu.vendor machdep.cpu.brand_string hw.l1icachesize hw.l2cachesize");
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
                        cpuidVendor = sysctlOutputLine.Substring(vendorId.Length);
                    if (sysctlOutputLine.StartsWith(modelId) && string.IsNullOrEmpty(name))
                        name = sysctlOutputLine.Substring(modelId.Length);
                    if (sysctlOutputLine.StartsWith(l1Name))
                        cacheL1 = uint.Parse(sysctlOutputLine.Substring(l1Name.Length));
                    if (sysctlOutputLine.StartsWith(l2Name))
                        cacheL2 = uint.Parse(sysctlOutputLine.Substring(l2Name.Length));
                }
            }
            catch (Exception ex)
            {
                HardwareProber.errors.Add(ex);
            }

            // Finally, return a single item array containing processor information
            ProcessorPart processorPart = new()
            {
                ProcessorCores = numberOfCores,
                CoresForEachCore = numberOfCoresForEachCore,
                L1CacheSize = cacheL1,
                L2CacheSize = cacheL2,
                L3CacheSize = cacheL3,
                Name = name,
                CpuidVendor = cpuidVendor,
                Speed = clockSpeed,
            };
            return new[] { processorPart };
        }

        public BaseHardwarePartInfo[] GetBaseHardwarePartsWindows()
        {
            // Some variables to install.
            int numberOfCores = 0;
            int numberOfCoresForEachCore = 1;
            uint cacheL1 = 0;
            uint cacheL2 = 0;
            uint cacheL3 = 0;
            string name = "";
            string cpuidVendor = "";
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
                    PlatformWindowsInterop.PROCESSOR_POWER_INFORMATION[] procInfo = new PlatformWindowsInterop.PROCESSOR_POWER_INFORMATION[numberOfCores * numberOfCoresForEachCore];
                    uint powerBufferSize = (uint)(procInfo.Length * Marshal.SizeOf(typeof(PlatformWindowsInterop.PROCESSOR_POWER_INFORMATION)));
                    uint status = PlatformWindowsInterop.CallNtPowerInformation(11, IntPtr.Zero, 0, procInfo, powerBufferSize);
                    if (status == 0)
                        clockSpeed = procInfo[0].MaxMhz;
                }
            }
            catch (Exception ex)
            {
                HardwareProber.errors.Add(ex);
            }

            // Finally, return a single item array containing processor information
            ProcessorPart processorPart = new()
            {
                ProcessorCores = numberOfCores,
                CoresForEachCore = numberOfCoresForEachCore,
                L1CacheSize = cacheL1,
                L2CacheSize = cacheL2,
                L3CacheSize = cacheL3,
                Name = name,
                CpuidVendor = cpuidVendor,
                Speed = clockSpeed,
            };
            return new[] { processorPart };
        }
    }
}
