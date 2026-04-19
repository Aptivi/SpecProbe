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

using System;
using System.Collections.Generic;
using System.IO;
using SpecProbe.DeviceInfo.Isa.Elements;
using SpecProbe.Languages;
using Textify.General;

namespace SpecProbe.DeviceInfo.Isa
{
    /// <summary>
    /// ISA ID list parser
    /// </summary>
    public static class IsaListParser
    {
        private static IsaDeviceInfo[] cachedDevices = [];

        /// <summary>
        /// Lists all the devices that a vendor made
        /// </summary>
        /// <returns>List of devices</returns>
        public static IsaDeviceInfo[] ListDevices() =>
            cachedDevices;

        /// <summary>
        /// Get a device made by a specified vendor
        /// </summary>
        /// <param name="deviceId">Device ID</param>
        /// <returns>Device information</returns>
        public static IsaDeviceInfo GetDevice(string deviceId)
        {
            var devices = ListDevices();
            foreach (var device in devices)
                if (device.Id == deviceId)
                    return device;
            throw new ArgumentException(LanguageTools.GetLocalized("SPECPROBE_PCI_EXCEPTION_DEVICENOTFOUND").FormatString(deviceId));
        }

        /// <summary>
        /// Checks to see if a device made by a specified vendor is registered
        /// </summary>
        /// <param name="deviceId">Device ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsDeviceRegistered(string deviceId)
        {
            var devices = ListDevices();
            if (devices.Length == 0)
                return false;
            foreach (var device in devices)
                if (device.Id == deviceId)
                    return true;
            return false;
        }

        private static void SerializeIsaList()
        {
            // Get the ISA ID lines and parse all the vendors
            var lines = GetIsaIdsLines();
            List<IsaDeviceInfo> devices = [];
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Some variables
                string id = line.Substring(0, line.IndexOf(' '));
                string name = line.Substring(line.IndexOf(' ') + 1).ReleaseDoubleQuotes();

                // Make a new device class (blanket)
                devices.Add(new(name, id));
            }
            cachedDevices = [.. devices];
        }

        private static string[] GetIsaIdsLines()
        {
            // Open the ISA ID list stream (source: https://github.com/torvalds/linux/blob/master/drivers/eisa/eisa.ids)
            var stream = typeof(IsaListParser).Assembly.GetManifestResourceStream("SpecProbe.DeviceInfo.Isa.List.eisa.ids");
            var reader = new StreamReader(stream);

            // Get the lines
            string[] lines = reader.ReadToEnd().SplitNewLines();
            return lines;
        }

        static IsaListParser()
        {
            if (cachedDevices.Length == 0)
                SerializeIsaList();
        }
    }
}
