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

namespace SpecProbe.DeviceInfo.Isa.Elements
{
    /// <summary>
    /// ISA device info
    /// </summary>
    public class IsaDeviceInfo
    {
        internal string deviceName;
        internal string deviceId;

        /// <summary>
        /// Device name
        /// </summary>
        public string Name =>
            deviceName;

        /// <summary>
        /// Device ID
        /// </summary>
        public string Id =>
            deviceId;

        internal IsaDeviceInfo(string deviceName, string deviceId)
        {
            this.deviceName = deviceName;
            this.deviceId = deviceId;
        }
    }
}
