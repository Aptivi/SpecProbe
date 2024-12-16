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

namespace SpecProbe.Pci.Elements
{
    /// <summary>
    /// PCI vendor information
    /// </summary>
    public class PciVendorInfo
    {
        internal string vendorName;
        internal int vendorId;
        internal PciDeviceInfo[] devices = [];

        /// <summary>
        /// Vendor name
        /// </summary>
        public string Name =>
            vendorName;

        /// <summary>
        /// Vendor ID
        /// </summary>
        public int Id =>
            vendorId;

        /// <summary>
        /// List of PCI device info that this vendor made
        /// </summary>
        public PciDeviceInfo[] Devices =>
            devices;

        internal PciVendorInfo(string vendorName, int vendorId)
        {
            this.vendorName = vendorName;
            this.vendorId = vendorId;
        }
    }
}
