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

using SpecProbe.Probers;
using SpecProbe.Probers.Platform;
using System.Linq;

namespace SpecProbe.Parts.Types
{
    /// <summary>
    /// Processor part
    /// </summary>
    public class ProcessorPart : BaseHardwarePartInfo, IHardwarePartInfo
    {
        private int coresNum;
        private int logicalCores;
        private int numCores;
        private uint cacheL1;
        private uint cacheL2;
        private uint cacheL3;
        private string name = "Unknown";
        private string cpuidVendor = "Unknown";
        private string hypervisorVendor = "";
        private double speed;
        private bool hypervisor;
        private string[] flags = [];

        /// <inheritdoc/>
        public override HardwarePartType Type =>
            HardwarePartType.Processor;

        /// <summary>
        /// Number of cores (physical)
        /// </summary>
        public int ProcessorCores
        {
            get => coresNum;
            internal set => coresNum = value;
        }
        /// <summary>
        /// Number of cores (logical)
        /// </summary>
        public int LogicalCores
        {
            get => logicalCores;
            internal set => logicalCores = value;
        }
        /// <summary>
        /// Number of cores for each physical processor
        /// </summary>
        public int Cores
        {
            get => numCores;
            internal set => numCores = value;
        }
        /// <summary>
        /// Size of the L1 cache in bytes
        /// </summary>
        public uint L1CacheSize
        {
            get => cacheL1;
            internal set => cacheL1 = value;
        }
        /// <summary>
        /// Size of the L2 cache in bytes
        /// </summary>
        public uint L2CacheSize
        {
            get => cacheL2;
            internal set => cacheL2 = value;
        }
        /// <summary>
        /// Size of the L3 cache in bytes
        /// </summary>
        public uint L3CacheSize
        {
            get => cacheL3;
            internal set => cacheL3 = value;
        }
        /// <summary>
        /// Processor name
        /// </summary>
        public string Name
        {
            get => name;
            internal set => name = value;
        }
        /// <summary>
        /// Processor vendor from CPUID
        /// </summary>
        public string CpuidVendor
        {
            get => cpuidVendor;
            internal set => cpuidVendor = value;
        }
        /// <summary>
        /// Processor vendor from CPUID's hypervisor feature
        /// </summary>
        public string HypervisorVendor
        {
            get => hypervisorVendor;
            internal set => hypervisorVendor = value;
        }
        /// <summary>
        /// Processor vendor (real)
        /// </summary>
        public string Vendor
        {
            get
            {
                return CpuidVendor switch
                {
                    "AuthenticAMD" => "AMD",
                    "GenuineIntel" => "Intel",
                    _ => Name.Contains(" ") ? Name.Split(' ')[0] : "Unknown",
                };
            }
        }
        /// <summary>
        /// Processor in Mhz
        /// </summary>
        public double Speed
        {
            get => speed;
            internal set => speed = value;
        }
        /// <summary>
        /// Whether this computer is running on a hypervisor (not 100% accurate as this returns true on Windows systems that have
        /// Hyper-V/WSL installed even if the application calling this property is a host OS)
        /// </summary>
        public bool Hypervisor
        {
            get => hypervisor;
            internal set => hypervisor = value;
        }
        /// <summary>
        /// Whether this program is run on a hypervisor (only true if <see cref="Hypervisor"/> is true and the <see cref="HypervisorVendor"/> is in a list of known hypervisors)
        /// </summary>
        public bool OnHypervisor =>
            ProcessorVariables.knownHypervisorBrands.Contains(hypervisorVendor);
        /// <summary>
        /// List of processor flags
        /// </summary>
        public string[] Flags
        {
            get => flags;
            internal set => flags = value;
        }
    }
}
