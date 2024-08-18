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
        private string name;
        private string cpuidVendor;
        private double speed;
        private bool hypervisor;

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
        /// Whether this program is being run on a hypervisor
        /// </summary>
        public bool Hypervisor
        {
            get => hypervisor;
            internal set => hypervisor = value;
        }
    }
}
