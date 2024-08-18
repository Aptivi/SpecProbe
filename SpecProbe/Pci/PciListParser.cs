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

using SpecProbe.Pci.Elements;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Textify.General;

namespace SpecProbe.Pci
{
    /// <summary>
    /// PCI ID list parser
    /// </summary>
    public static class PciListParser
    {
        private static PciVendorInfo[] cachedVendors = [];
        private static PciDeviceClassInfo[] cachedClasses = [];

        /// <summary>
        /// Lists all the vendors
        /// </summary>
        /// <returns>List of vendors</returns>
        public static PciVendorInfo[] ListVendors() =>
            cachedVendors;

        /// <summary>
        /// Gets a vendor
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <returns>Vendor information</returns>
        public static PciVendorInfo GetVendor(int vendorId)
        {
            var vendors = ListVendors();
            foreach (var vendor in vendors)
                if (vendor.Id == vendorId)
                    return vendor;
            throw new ArgumentException($"Vendor ID {vendorId} not found.");
        }

        /// <summary>
        /// Checks to see if a vendor is registered
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsVendorRegistered(int vendorId)
        {
            var vendors = ListVendors();
            foreach (var vendor in vendors)
                if (vendor.Id == vendorId)
                    return true;
            return false;
        }

        /// <summary>
        /// Lists all the devices that a vendor made
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <returns>List of devices</returns>
        public static PciDeviceInfo[] ListDevices(int vendorId) =>
            GetVendor(vendorId).Devices;

		/// <summary>
		/// Get a device made by a specified vendor
		/// </summary>
		/// <param name="vendorId">Vendor ID</param>
		/// <param name="deviceId">Device ID</param>
		/// <returns>Device information</returns>
		public static PciDeviceInfo GetDevice(int vendorId, int deviceId)
		{
			var devices = ListDevices(vendorId);
            if (devices.Length == 0)
			    throw new ArgumentException($"Vendor ID {vendorId} doesn't have any devices.");
			foreach (var device in devices)
                if (device.Id == deviceId)
                    return device;
			throw new ArgumentException($"Device ID {deviceId} not found.");
		}

		/// <summary>
		/// Checks to see if a device made by a specified vendor is registered
		/// </summary>
		/// <param name="vendorId">Vendor ID</param>
		/// <param name="deviceId">Device ID</param>
		/// <returns>True if registered; false otherwise.</returns>
		public static bool IsDeviceRegistered(int vendorId, int deviceId)
		{
			var devices = ListDevices(vendorId);
            if (devices.Length == 0)
                return false;
			foreach (var device in devices)
                if (device.Id == deviceId)
                    return true;
			return false;
		}

		/// <summary>
		/// Lists all the sub-devices from a device that a vendor made
		/// </summary>
		/// <param name="vendorId">Vendor ID</param>
		/// <param name="deviceId">Device ID</param>
		/// <returns>List of sub-devices</returns>
		public static PciDeviceInfo[] ListSubDevices(int vendorId, int deviceId) =>
			GetDevice(vendorId, deviceId).SubDevices;

		/// <summary>
		/// Get a sub-device made by a specified vendor
		/// </summary>
		/// <param name="vendorId">Vendor ID</param>
		/// <param name="deviceId">Device ID</param>
		/// <param name="subVendorId">Sub-vendor ID</param>
		/// <param name="subDeviceId">Sub-device ID</param>
		/// <returns>Sub-device information</returns>
		public static PciDeviceInfo GetSubDevice(int vendorId, int deviceId, int subVendorId, int subDeviceId)
		{
			var devices = ListSubDevices(vendorId, deviceId);
			if (devices.Length == 0)
				throw new ArgumentException($"Vendor ID {vendorId} doesn't have any sub-devices that device ID {deviceId} uses.");
			foreach (var device in devices)
				if (device.VendorId == subVendorId && device.Id == subDeviceId)
					return device;
			throw new ArgumentException($"Device ID {deviceId} not found.");
		}

		/// <summary>
		/// Checks to see if a sub-device made by a specified vendor is registered
		/// </summary>
		/// <param name="vendorId">Vendor ID</param>
		/// <param name="deviceId">Device ID</param>
		/// <param name="subVendorId">Sub-vendor ID</param>
		/// <param name="subDeviceId">Sub-device ID</param>
		/// <returns>True if registered; false otherwise.</returns>
		public static bool IsSubDeviceRegistered(int vendorId, int deviceId, int subVendorId, int subDeviceId)
		{
			var devices = ListSubDevices(vendorId, deviceId);
			if (devices.Length == 0)
				return false;
			foreach (var device in devices)
				if (device.VendorId == subVendorId && device.Id == subDeviceId)
					return true;
			return false;
		}

		private static void SerializePciList()
        {
            // Get the PCI ID lines and parse all the vendors
            var lines = GetPciIdsLines();
            List<PciVendorInfo> vendors = [];
            List<PciDeviceInfo> devices = [];
            List<PciDeviceInfo> subDevices = [];
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Break if we've reached device classes
                if (line.StartsWith("C 00"))
                    break;

                // Count the number of tabs to indicate either a vendor, a device, or a subdevice
                bool IsSubDevice = line[0] == '\t' && line[1] == '\t';
                bool IsDevice = line[0] == '\t' && !IsSubDevice;
                bool IsVendor = !IsDevice && !IsSubDevice;
                if (IsVendor)
                {
                    // Save the changes if we have a vendor
                    if (vendors.Count > 0)
                    {
						vendors[vendors.Count - 1].devices = [.. devices];
                        var finalDevices = vendors[vendors.Count - 1].devices;
                        if (finalDevices.Length > 0 && subDevices.Count > 0)
							finalDevices[finalDevices.Length - 1].subDevices = [.. subDevices];
					}

                    // Clear the devices and the subdevices since we have a new vendor
                    devices.Clear();
					subDevices.Clear();

                    // Some variables
                    string name = "";
                    int id = 0;

                    // Now, parse a vendor line
                    string idString = line.Substring(0, 4);
                    name = line.Substring(6);
                    id = int.Parse(idString, NumberStyles.HexNumber);

                    // Make a new vendor class (blanket)
                    vendors.Add(new(name, id));
                }
                else if (IsDevice)
				{
					// Save the changes if we have a device
					if (devices.Count > 0 && subDevices.Count > 0)
						devices[devices.Count - 1].subDevices = [.. subDevices];

					// Clear the subdevices since we have a new device
					subDevices.Clear();

					// Some variables
					string name = "";
					int id = 0;

					// Now, parse a device line
					string idString = line.Substring(1, 4);
					name = line.Substring(7);
					id = int.Parse(idString, NumberStyles.HexNumber);

					// Make a new vendor class (blanket)
					devices.Add(new(name, id, vendors[vendors.Count - 1].Id));
				}
                else if (IsSubDevice)
				{
					// Some variables
					string name = "";
					int id = 0;
					int subVendorid = 0;

					// Now, parse a subdevice line
					string subVendorIdString = line.Substring(2, 4);
					string idString = line.Substring(7, 4);
					name = line.Substring(13);
					subVendorid = int.Parse(subVendorIdString, NumberStyles.HexNumber);
					id = int.Parse(idString, NumberStyles.HexNumber);

					// Make a new vendor class (blanket)
					subDevices.Add(new(name, id, subVendorid));
				}
            }
            cachedVendors = [.. vendors];
		}

		private static void SerializeClassList()
        {
            // Get the PCI ID lines and parse all the classes
            var lines = GetPciIdsLines();
            List<PciDeviceClassInfo> classes = [];
            List<PciDeviceSubclassInfo> subclasses = [];
            List<PciDeviceInterfaceInfo> interfaces = [];
            bool skipped = false;
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Start parsing if we've reached the classes section
                if (line.StartsWith("C 00") && !skipped)
                    skipped = true;
                else if (!skipped)
                    continue;

                // Count the number of tabs to indicate either a class, a subclass, or an interface
                bool IsInterface = line[0] == '\t' && line[1] == '\t';
                bool IsSubclass = line[0] == '\t' && !IsInterface;
                bool IsClass = !IsSubclass && !IsInterface;
                if (IsClass)
                {
                    // Save the changes if we have a class
                    if (classes.Count > 0)
                    {
						classes[classes.Count - 1].subclasses = [.. subclasses];
                        var finalSubclasses = classes[classes.Count - 1].subclasses;
                        if (finalSubclasses.Length > 0 && interfaces.Count > 0)
							finalSubclasses[finalSubclasses.Length - 1].interfaces = [.. interfaces];
					}

                    // Clear the subclasses and the interfaces since we have a new class
                    subclasses.Clear();
					interfaces.Clear();

                    // Some variables
                    string name = "";
                    int classId = 0;

                    // Now, parse a class line
                    string classIdString = line.Substring(2, 2);
                    name = line.Substring(6);
                    classId = int.Parse(classIdString, NumberStyles.HexNumber);

                    // Make a new class class (blanket)
                    classes.Add(new(name, classId));
                }
                else if (IsSubclass)
				{
					// Save the changes if we have a subclass
					if (subclasses.Count > 0 && interfaces.Count > 0)
						subclasses[subclasses.Count - 1].interfaces = [.. interfaces];

					// Clear the interfaces since we have a new subclass
					interfaces.Clear();

					// Some variables
					string name = "";
					int subclassId = 0;

					// Now, parse a subclass line
					string subclassIdString = line.Substring(1, 2);
					name = line.Substring(5);
					subclassId = int.Parse(subclassIdString, NumberStyles.HexNumber);

					// Make a new class class (blanket)
					subclasses.Add(new(name, subclassId));
				}
                else if (IsInterface)
				{
					// Some variables
					string name = "";
					int interfaceId = 0;

					// Now, parse a interface line
					string interfaceIdString = line.Substring(2, 4);
					name = line.Substring(6);
					interfaceId = int.Parse(interfaceIdString, NumberStyles.HexNumber);

					// Make a new class class (blanket)
					interfaces.Add(new(name, interfaceId));
				}
            }
            cachedClasses = [.. classes];
		}

        private static string[] GetPciIdsLines()
		{
			// Open the PCI ID list stream (source: https://pci-ids.ucw.cz/)
			var stream = typeof(PciListParser).Assembly.GetManifestResourceStream("SpecProbe.Pci.List.pci.ids");
			var reader = new StreamReader(stream);

			// Get the lines
			string[] lines = reader.ReadToEnd().SplitNewLines();
            return lines;
		}

        static PciListParser()
        {
            if (cachedVendors.Length == 0)
                SerializePciList();
            if (cachedClasses.Length == 0)
                SerializeClassList();
        }
    }
}
