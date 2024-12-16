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
    /// USB video terminal information
    /// </summary>
    public class UsbVideoTerminalInfo
    {
        internal string videoTerminalName;
        internal int videoTerminalId;

        /// <summary>
        /// Video Terminal name
        /// </summary>
        public string Name =>
            videoTerminalName;

        /// <summary>
        /// Video Terminal ID
        /// </summary>
        public int Id =>
            videoTerminalId;

        internal UsbVideoTerminalInfo(string videoTerminalName, int videoTerminalId)
        {
            this.videoTerminalName = videoTerminalName;
            this.videoTerminalId = videoTerminalId;
        }
    }
}
