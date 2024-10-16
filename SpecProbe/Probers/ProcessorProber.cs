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

using SpecProbe.Native;
using SpecProbe.Native.Helpers;
using SpecProbe.Parts.Types;
using SpecProbe.Probers.Platform;
using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SpecProbe.Probers
{
    internal static class ProcessorProber
    {
        private static readonly string[] eax1EdxFeatures =
        [
            "fpu", "vme", "de", "pse", "tsc", "msr", "pae", "mce", "cx8", "apic", "mtrr_reserved", "sep", "mtrr",
            "pge", "mca", "cmov", "pat", "pse-36", "psn", "clfsh", "nx", "ds", "acpi", "mmx", "fxsr", "sse", "sse2",
            "ss", "htt", "tm", "ia64", "pbe"
        ];

        private static readonly string[] eax1EcxFeatures =
        [
            "sse3", "pclmulqdq", "dtes64", "monitor", "ds-cpl", "vmx", "smx", "est", "tm2", "ssse3", "cnxt-id", "sdbg", "fma",
            "cx16", "xtpr", "pdcm", "pchnl", "pcid", "dca", "sse4.1", "sse4.2", "x2apic", "movbe", "popcnt", "tsc-deadline",
            "aes-ni", "xsave", "osxsave", "avx", "f16c", "rdrnd", "hypervisor"
        ];

        private static readonly string[] eax1EdxAmdFeatures =
        [
            "fpu", "vme", "de", "pse", "tsc", "msr", "pae", "mce", "cx8", "apic", "syscall_k6", "syscall", "mtrr",
            "pge", "mca", "cmov", "pat", "pse-36", "a1d_reserved18", "ecc", "nx", "a1d_reserved18", "mmxext", "mmx", "fxsr", "fxsr_opt", "pdpe1gb",
            "rdtscp", "a1d_reserved28", "lm", "3dnowext", "3dnow"
        ];

        private static readonly string[] eax1EcxAmdFeatures =
        [
            "lahf_lm", "cmp_legacy", "svm", "extapic", "cr8_legacy", "abm/lzcnt", "sse4a", "misalignsse", "3dnowprefetch", "osvw",
            "ibs", "xop", "skinit", "wdt", "a1c_reserved14", "lwp", "fma4", "tce", "a1c_reserved18", "nodeid_msr", "a1c_reserved20",
            "tbm", "topoext", "perfctr_core", "perfctr_nb", "streamperfmon", "dbx", "perftsc", "pcx_l2i", "monitorx", "addr_mask_ext"
        ];

        private static readonly string[] eax7Ecx0EbxFeatures =
        [
            "fsgsbase", "ia32_tsc_adjust_msr", "sgx", "bmi1", "hle", "avx2", "fdp-excptn-only", "smep", "bmi2", "erms", "invpcid",
            "rtm", "rdt-m/pqm", "fpucsds", "mpx", "rdt-a/pqe", "avx512-f", "avx512-dq", "rdseed", "adx", "smap", "avx512-ifma",
            "pcommit", "clflushopt", "clwb", "pt", "avx512-pf", "avx512-er", "avx512-cd", "sha", "avx512-bw", "avx512-vl"
        ];

        private static readonly string[] eax7Ecx0EcxFeatures =
        [
            "prefetchwt1", "avx512-vbmi", "umip", "pku", "ospke", "waitpkg", "avx512-vbmi2", "cet_ss/shstk", "gfni", "vaes",
            "vpclmulqdq", "avx512-vnni", "avx512-bitalg", "tme_en", "avx512-vpopcntdq", "fzm", "la57", "mawau", "rdpid", "kl",
            "bus-lock-detect", "cldemote", "mprr", "movdiri", "movdir64b", "enqcmd", "sgx-lc", "pks"
        ];

        private static readonly string[] eax7Ecx0EdxFeatures =
        [
            "sgx-tem", "sgx-keys", "avx512-4vnniw", "avx512-4fmaps", "fsrm", "uintr", "a7c0d_reserved6", "a7c0d_reserved7", "avx512-vp2intersect",
            "srbds-ctrl", "md-clear", "rtm-always-abort", "a7c0d_reserved12", "rtm-force-abort", "serialize", "hybrid", "tsxldtrk", "a7c0d_reserved17",
            "pconfig", "lbr", "cet-ibt", "a7c0d_reserved21", "amx-bf16", "avx512-fp16", "amx-tile", "amx-int8", "ibrs/spec_ctrl", "stibp",
            "l1d_flush", "ia32_arch_capabilities_msr", "is32_core_capabilities_msr", "ssbd"
        ];

        private static readonly string[] eax7Ecx1EaxFeatures =
        [
            "sha512", "sm3", "sm4", "rao-int", "avx-vnni", "avx512-bf16", "lass", "cmpccxadd", "archperfmonext", "dedup", "fzrm",
            "fsrs", "rsrcs", "a7c1a_reserved13", "a7c1a_reserved14", "a7c1a_reserved15", "a7c1a_reserved16", "fred", "lkgs", "wrmsrns", "nmi_src", "amx-fp16",
            "hreset", "avx-ifma", "a7c1a_reserved24", "a7c1a_reserved25", "lam", "msrlist", "a7c1a_reserved28", "a7c1a_reserved29", "invd_disable_­post_bios_done"
        ];

        private static readonly string[] eax7Ecx1EbxFeatures =
        [
            "pbn", "pbndkb"
        ];

        private static readonly string[] eax7Ecx1EcxFeatures =
        [
            "a7c1c_reserved0", "a7c1c_reserved1", "legacy_reduced_isa", "a7c1c_reserved3", "sipi64"
        ];

        private static readonly string[] eax7Ecx1EdxFeatures =
        [
            "a7c1d_reserved0", "a7c1d_reserved1", "a7c1d_reserved2", "a7c1d_reserved3", "avx-vnni-int8", "avx-ne-convert", "a7c1d_reserved6", "a7c1d_reserved7",
            "amx-complex", "a7c1d_reserved9", "avx-vnni-int16", "a7c1d_reserved11", "a7c1d_reserved12", "utmr", "prefetchi", "user_msr", "a7c1d_reserved16",
            "uiret-uif-from-rflags", "cet-sss", "avx10", "a7c1d_reserved20", "apx_f", "a7c1d_reserved22", "mwait"
        ];

        private static readonly string[] eax7Ecx2EdxFeatures =
        [
            "psfd", "ipred_ctrl", "rrsba_ctrl", "ddpd_u", "bhi_ctrl", "mcdt_no", "uc_lock_no", "monitor_mitg_no"
        ];

        public static ProcessorPart Probe(out Exception[] errors)
        {
            if (PlatformHelper.IsOnWindows())
                return ProbeWindows(out errors);
            else if (PlatformHelper.IsOnMacOS())
                return ProbeMacOS(out errors);
            else
                return ProbeLinux(out errors);
        }

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
            string name = "";
            string cpuidVendor = "";
            double clockSpeed = 0.0;
            string[] features = [];

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

            try
            {
                // Get the features
                if (!PlatformHelper.IsOnArmOrArm64())
                    features = PopulateFeatures();

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
                        {
                            string armVendorId = cpuInfoLine.Replace(armVendorName, "");
                            armVendorId = armVendorId.Substring(2);
                            int armVendorIdInt = int.Parse(armVendorId, NumberStyles.AllowHexSpecifier);
                            cpuidVendor = armVendorIdInt switch
                            {
                                0x41 => "ARM",
                                _ => "",
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
                            numberOfPhysicalCores = physicalIdNum + 1;
                        }

                        // Get the number of logical processors
                        if (cpuInfoLine.StartsWith(logicalId))
                        {
                            string logicalIdString = cpuInfoLine.Replace(logicalId, "");
                            int logicalIdNum = int.Parse(logicalIdString);
                            numberOfLogicalCores = logicalIdNum + 1;
                        }

                        // Get the number of logical processors
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
                exceptions.Add(ex);
            }

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
                Speed = clockSpeed,
                Hypervisor = features.Contains("hypervisor"),
                Flags = features,
            };
            errors = [.. exceptions];
            return processorPart;
        }

        public static ProcessorPart ProbeMacOS(out Exception[] errors)
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

                // Then, the features
                if (!PlatformHelper.IsOnArmOrArm64())
                    features = PopulateFeatures();

                // Then, fill the rest
                string sysctlOutput = PlatformHelper.ExecuteProcessToString("/usr/sbin/sysctl", "machdep.cpu.core_count machdep.cpu.cores_per_package hw.cpufrequency machdep.cpu.vendor machdep.cpu.brand_string machdep.cpu.features machdep.cpu.leaf7_features machdep.cpu.extfeatures hw.l1icachesize hw.l2cachesize");
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
                exceptions.Add(ex);
            }

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
                Speed = clockSpeed,
                Hypervisor = features.Contains("hypervisor"),
                Flags = features,
            };
            errors = [.. exceptions];
            return processorPart;
        }

        public static ProcessorPart ProbeWindows(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            string[] features = [];
            int numberOfCores = 0;
            int numberOfCoresForEachCore = 1;
            int numberOfLogicalCores = 0;
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

                // Then, the features
                if (!PlatformHelper.IsOnArmOrArm64())
                    features = PopulateFeatures();

                // Finally, get the actual logical processor count
                PlatformWindowsInterop.GetSystemInfo(out PlatformWindowsInterop.SYSTEM_INFO system);
                numberOfLogicalCores = (int)system.dwNumberOfProcessors;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

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
                Speed = clockSpeed,
                Hypervisor = features.Contains("hypervisor"),
                Flags = features,
            };
            errors = [.. exceptions];
            return processorPart;
        }

        private static string[] PopulateFeatures()
        {
            Initializer.InitializeNative();
            var cpuMaxDelegate = ProcessorHelper.GetMaxDelegate();
            var cpuMaxExtendedDelegate = ProcessorHelper.GetMaxExtendedDelegate();
            var cpuValuesDelegate = ProcessorHelper.GetValuesDelegate();
            uint max = cpuMaxDelegate.Invoke();
            uint maxExt = cpuMaxExtendedDelegate.Invoke();
            List<string> features = [];

            (bool exists, uint eax, uint ebx, uint ecx, uint edx) GetValues(uint eax, uint ecx)
            {
                if (eax < 1 || eax > max)
                    return (false, 0, 0, 0, 0);
                return GetValuesUnchecked(eax, ecx);
            }

            (bool exists, uint eax, uint ebx, uint ecx, uint edx) GetValuesExt(uint eax, uint ecx)
            {
                if (eax < 1 || eax > maxExt)
                    return (false, 0, 0, 0, 0);
                return GetValuesUnchecked(eax, ecx);
            }

            (bool exists, uint eax, uint ebx, uint ecx, uint edx) GetValuesUnchecked(uint eax, uint ecx)
            {
                // Get the value in a specified leaf
                var values = cpuValuesDelegate.Invoke(eax, ecx);

                // Extract the values from the native array
                uint[] parsedValues = new uint[4];
                for (int step = 0; step < parsedValues.Length; step++)
                {
                    var nativeValue = values + sizeof(uint) * step;
                    parsedValues[step] = (uint)Marshal.ReadInt32(nativeValue);
                }

                // Return the result
                return (true, parsedValues[0], parsedValues[1], parsedValues[2], parsedValues[3]);
            }

            // Look for index 1 and build a feature map using EDX and ECX
            var idx1 = GetValues(1, 0);
            features.AddRange(BuildFeatureList(idx1, 3, eax1EdxFeatures));
            features.AddRange(BuildFeatureList(idx1, 2, eax1EcxFeatures));

            // Look for index 80000001h and build a feature map using EDX and ECX
            var idxAmd = GetValuesExt(0x80000001, 0);
            features.AddRange(BuildFeatureList(idxAmd, 3, eax1EdxAmdFeatures));
            features.AddRange(BuildFeatureList(idxAmd, 2, eax1EcxAmdFeatures));

            // Look for index 7, ECX 0, and build a feature map using EBX, ECX, and EDX
            var idx7Ecx0 = GetValues(7, 0);
            features.AddRange(BuildFeatureList(idx7Ecx0, 1, eax7Ecx0EbxFeatures));
            features.AddRange(BuildFeatureList(idx7Ecx0, 2, eax7Ecx0EcxFeatures));
            features.AddRange(BuildFeatureList(idx7Ecx0, 3, eax7Ecx0EdxFeatures));

            // Look for index 7, ECX 1, and build a feature map using EAX, EBX, ECX, and EDX
            var idx7Ecx1 = GetValues(7, 1);
            features.AddRange(BuildFeatureList(idx7Ecx1, 0, eax7Ecx1EaxFeatures));
            features.AddRange(BuildFeatureList(idx7Ecx1, 1, eax7Ecx1EbxFeatures));
            features.AddRange(BuildFeatureList(idx7Ecx1, 2, eax7Ecx1EcxFeatures));
            features.AddRange(BuildFeatureList(idx7Ecx1, 3, eax7Ecx1EdxFeatures));

            // Look for index 7, ECX 2, and build a feature map using EDX
            var idx7Ecx2 = GetValues(7, 2);
            features.AddRange(BuildFeatureList(idx7Ecx2, 3, eax7Ecx2EdxFeatures));

            // Return as array
            return [.. features];
        }

        private static string[] BuildFeatureList((bool exists, uint eax, uint ebx, uint ecx, uint edx) values, int valueNum, string[] featureNames)
        {
            List<string> features = [];
            uint value =
                valueNum == 0 ? values.eax :
                valueNum == 1 ? values.ebx :
                valueNum == 2 ? values.ecx :
                valueNum == 3 ? values.edx : 0;
            for (int i = 0; i < featureNames.Length; i++)
            {
                string name = featureNames[i];
                bool supported = (value & (1 << i)) > 0;
                if (supported && !name.Contains("reserved") && !features.Contains(name))
                {
                    if (name == "mawau")
                    {
                        uint mawau1 = (uint)(value & (1 << i));
                        uint mawau2 = (uint)(value & (1 << i + 1));
                        uint mawau3 = (uint)(value & (1 << i + 2));
                        uint mawau4 = (uint)(value & (1 << i + 3));
                        uint mawau5 = (uint)(value & (1 << i + 4));
                        uint result = mawau5 << 4 | mawau4 << 3 | mawau3 << 2 | mawau2 << 1 | mawau1;
                        if (result > 0)
                            features.Add(name);
                        i += 4;
                    }
                    else
                        features.Add(name);
                }
            }
            return [.. features];
        }
    }
}
