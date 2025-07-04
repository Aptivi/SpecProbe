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

using SpecProbe.Languages;
using SpecProbe.Pci;

namespace SpecProbe.Parts.Types
{
    /// <summary>
    /// Video card part
    /// </summary>
    public class VideoPart : BaseHardwarePartInfo, IHardwarePartInfo
    {
        private string videoCardName = LanguageTools.GetLocalized("SPECPROBE_COMMON_UNKNOWN");
        private uint vendorId;
        private uint modelId;

        /// <inheritdoc/>
        public override HardwarePartType Type =>
            HardwarePartType.Video;

        /// <summary>
        /// Video card name
        /// </summary>
        public string VideoCardName
        {
            get => videoCardName;
            internal set => videoCardName = value;
        }

        /// <summary>
        /// Video card PCI name from the PCI ID database
        /// </summary>
        public string VideoCardPciName =>
            PciListParser.IsDeviceRegistered((int)vendorId, (int)modelId) ?
            PciListParser.GetDevice((int)vendorId, (int)modelId).Name :
            LanguageTools.GetLocalized("SPECPROBE_PARTS_UNKNOWNGPU");

        /// <summary>
        /// Video card PCI name from the PCI ID database
        /// </summary>
        public string VendorName =>
            PciListParser.IsVendorRegistered((int)vendorId) ?
            PciListParser.GetVendor((int)vendorId).Name :
            LanguageTools.GetLocalized("SPECPROBE_COMMON_UNKNOWNVENDOR");

        /// <summary>
        /// Vendor ID for this video card
        /// </summary>
        public uint VendorId
        {
            get => vendorId;
            internal set => vendorId = value;
        }

        /// <summary>
        /// Model ID for this video card
        /// </summary>
        public uint ModelId
        {
            get => modelId;
            internal set => modelId = value;
        }
    }
}
