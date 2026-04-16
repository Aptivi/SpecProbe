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

using System.Collections.Generic;
using System.Runtime.InteropServices;
using SpecProbe.Loader.Native;
using SpecProbe.Loader.Native.Helpers;

namespace SpecProbe.Probers.Platform
{
    internal static class ProcessorCpuid
    {
        private static readonly object nativeLock = new();

        internal static string MapRealVendorFromCpuid(string cpuidVendor)
        {
            if (!ProcessorVariables.vendorMappings.ContainsKey(cpuidVendor))
                return cpuidVendor;
            return ProcessorVariables.vendorMappings[cpuidVendor];
        }

        internal static string MapRealHvVendorFromCpuid(string cpuidVendor)
        {
            if (!ProcessorVariables.hypervisorMappings.ContainsKey(cpuidVendor))
                return cpuidVendor;
            return ProcessorVariables.hypervisorMappings[cpuidVendor];
        }

        internal static string[] PopulateFeatures()
        {
            Initializer.InitializeNative();
            List<string> features = [];

            // Look for index 1 and build a feature map using EDX and ECX
            var idx1 = GetValues(1, 0);
            features.AddRange(BuildFeatureList(idx1, 3, ProcessorVariables.eax1EdxFeatures));
            features.AddRange(BuildFeatureList(idx1, 2, ProcessorVariables.eax1EcxFeatures));

            // Look for index 80000001h and build a feature map using EDX and ECX
            var idxAmd = GetValuesExt(0x80000001, 0);
            features.AddRange(BuildFeatureList(idxAmd, 3, ProcessorVariables.eax1EdxAmdFeatures));
            features.AddRange(BuildFeatureList(idxAmd, 2, ProcessorVariables.eax1EcxAmdFeatures));

            // Look for index 7, ECX 0, and build a feature map using EBX, ECX, and EDX
            var idx7Ecx0 = GetValues(7, 0);
            features.AddRange(BuildFeatureList(idx7Ecx0, 1, ProcessorVariables.eax7Ecx0EbxFeatures));
            features.AddRange(BuildFeatureList(idx7Ecx0, 2, ProcessorVariables.eax7Ecx0EcxFeatures));
            features.AddRange(BuildFeatureList(idx7Ecx0, 3, ProcessorVariables.eax7Ecx0EdxFeatures));

            // Look for index 7, ECX 1, and build a feature map using EAX, EBX, ECX, and EDX
            var idx7Ecx1 = GetValues(7, 1);
            features.AddRange(BuildFeatureList(idx7Ecx1, 0, ProcessorVariables.eax7Ecx1EaxFeatures));
            features.AddRange(BuildFeatureList(idx7Ecx1, 1, ProcessorVariables.eax7Ecx1EbxFeatures));
            features.AddRange(BuildFeatureList(idx7Ecx1, 2, ProcessorVariables.eax7Ecx1EcxFeatures));
            features.AddRange(BuildFeatureList(idx7Ecx1, 3, ProcessorVariables.eax7Ecx1EdxFeatures));

            // Look for index 7, ECX 2, and build a feature map using EDX
            var idx7Ecx2 = GetValues(7, 2);
            features.AddRange(BuildFeatureList(idx7Ecx2, 3, ProcessorVariables.eax7Ecx2EdxFeatures));

            // Return as array
            return [.. features];
        }

        internal static string DetectHypervisor()
        {
            var (_, _, ebx, ecx, edx) = GetValuesUnchecked(0x40000000, 0);

            // Get the individual characters from EBX, ECX, and EDX
            char[] chars =
            [
                (char)((ebx >> 0) & 255),
                (char)((ebx >> 8) & 255),
                (char)((ebx >> 16) & 255),
                (char)((ebx >> 24) & 255),
                (char)((ecx >> 0) & 255),
                (char)((ecx >> 8) & 255),
                (char)((ecx >> 16) & 255),
                (char)((ecx >> 24) & 255),
                (char)((edx >> 0) & 255),
                (char)((edx >> 8) & 255),
                (char)((edx >> 16) & 255),
                (char)((edx >> 24) & 255),
            ];

            // Build the hypervisor string
            return new(chars);
        }

        internal static (uint l1, uint l2, uint l3) GetCacheLevelSizes(string vendor)
        {
            if (vendor == "AMD")
                return GetCacheLevelSizesAMD();
            return GetCacheLevelSizesIntel();
        }

        internal static (uint l1, uint l2, uint l3) GetCacheLevelSizesAMD()
        {
            // Pass 1: use CPUID with eax 0x8000001D, ecx subleaf
            (uint l1, uint l2, uint l3) = GetCacheLevelSizesIntel(0x8000001D);
            if (l1 == 0 && l2 == 0 && l3 == 0)
            {
                // Most likely an old AMD processor. Use CPUID with eax 0x80000005, ecx 0
                var (_, _, _, ecx, edx) = GetValuesUnchecked(0x80000005, 0);
                uint dataL1SizeKB = (ecx >> 24) & 0xFF;
                uint instructionL1SizeKB = (edx >> 24) & 0xFF;

                // Use CPUID with eax 0x80000006, ecx 0
                (_, _, _, ecx, edx) = GetValuesUnchecked(0x80000006, 0);
                uint l2SizeKB = (ecx >> 16) & 0xFFFF;
                uint l3SizeKB = ((edx >> 18) & 0x3FFF) * 512;

                // Convert to bytes
                l1 = (dataL1SizeKB + instructionL1SizeKB) * 1024;
                l2 = l2SizeKB * 1024;
                l3 = l3SizeKB * 1024;
            }
            return (l1, l2, l3);
        }

        internal static (uint l1, uint l2, uint l3) GetCacheLevelSizesIntel(uint inputEAX = 0x04)
        {
            // Use CPUID with eax 0x00000004, ecx subleaf
            uint subleaf = 0;
            uint l1 = 0, l2 = 0, l3 = 0;
            while (true)
            {
                // Get cache values
                var (exists, eax, ebx, ecx, _) = GetValuesUnchecked(inputEAX, subleaf);
                if (!exists)
                    break;

                // Get cache type and level
                uint cacheType = eax & 0x1F;
                uint cacheLevel = (eax >> 5) & 0x07;

                // Check to see if we have any caches left
                if (cacheType == 0)
                    break;

                // Get ways, partitions, line sizes, and sets
                uint lineSize = (ebx & 0xFFF) + 1;
                uint partitions = ((ebx >> 12) & 0x3FF) + 1;
                uint ways = ((ebx >> 22) & 0x3FF) + 1;
                uint sets = ecx + 1;
                uint size = ways * partitions * lineSize * sets;

                // Install the size values
                if (size > 0)
                {
                    if (cacheLevel == 1)
                        l1 += size;
                    else if (cacheLevel == 2)
                        l2 += size;
                    else if (cacheLevel == 3)
                        l3 += size;
                }

                // Move on to the next cache info array item
                subleaf++;
            }
            return (l1, l2, l3);
        }

        internal static (bool exists, uint eax, uint ebx, uint ecx, uint edx) GetValues(uint eax, uint ecx)
        {
            var cpuMaxDelegate = ProcessorHelper.GetMaxDelegate();
            uint max = cpuMaxDelegate.Invoke();
            if (eax < 1 || eax > max)
                return (false, 0, 0, 0, 0);
            return GetValuesUnchecked(eax, ecx);
        }

        internal static (bool exists, uint eax, uint ebx, uint ecx, uint edx) GetValuesExt(uint eax, uint ecx)
        {
            var cpuMaxExtendedDelegate = ProcessorHelper.GetMaxExtendedDelegate();
            uint maxExt = cpuMaxExtendedDelegate.Invoke();
            if (eax < 1 || eax > maxExt)
                return (false, 0, 0, 0, 0);
            return GetValuesUnchecked(eax, ecx);
        }

        internal static (bool exists, uint eax, uint ebx, uint ecx, uint edx) GetValuesUnchecked(uint eax, uint ecx)
        {
            lock (nativeLock)
            {
                var cpuValuesDelegate = ProcessorHelper.GetValuesDelegate();

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
        }

        internal static string[] BuildFeatureList((bool exists, uint eax, uint ebx, uint ecx, uint edx) values, int valueNum, string[] featureNames)
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
