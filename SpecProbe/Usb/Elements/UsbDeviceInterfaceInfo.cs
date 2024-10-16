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

namespace SpecProbe.Usb.Elements
{
    /// <summary>
    /// USB device interface information
    /// </summary>
    public class UsbDeviceInterfaceInfo
    {
        internal string interfaceName;
        internal int interfaceId;

        /// <summary>
        /// Interface name
        /// </summary>
        public string Name =>
            interfaceName;

        /// <summary>
        /// Interface ID
        /// </summary>
        public int Id =>
            interfaceId;

        internal UsbDeviceInterfaceInfo(string interfaceName, int interfaceId)
        {
            this.interfaceName = interfaceName;
            this.interfaceId = interfaceId;
        }
    }
}
