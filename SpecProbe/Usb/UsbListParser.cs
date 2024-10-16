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

using SpecProbe.Usb.Elements;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Textify.General;

namespace SpecProbe.Usb
{
    /// <summary>
    /// USB ID list parser
    /// </summary>
    public static class UsbListParser
    {
        private static UsbVendorInfo[] cachedVendors = [];
        private static UsbDeviceClassInfo[] cachedClasses = [];

        /// <summary>
        /// Lists all the vendors
        /// </summary>
        /// <returns>List of vendors</returns>
        public static UsbVendorInfo[] ListVendors() =>
            cachedVendors;

        /// <summary>
        /// Lists all the classes
        /// </summary>
        /// <returns>List of classes</returns>
        public static UsbDeviceClassInfo[] ListClasses() =>
            cachedClasses;

        /// <summary>
        /// Gets a vendor
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <returns>Vendor information</returns>
        public static UsbVendorInfo GetVendor(int vendorId)
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
        public static UsbDeviceInfo[] ListDevices(int vendorId) =>
            GetVendor(vendorId).Devices;

        /// <summary>
        /// Get a device made by a specified vendor
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <param name="deviceId">Device ID</param>
        /// <returns>Device information</returns>
        public static UsbDeviceInfo GetDevice(int vendorId, int deviceId)
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
        public static UsbDeviceInfo[] ListSubDevices(int vendorId, int deviceId) =>
            GetDevice(vendorId, deviceId).SubDevices;

        /// <summary>
        /// Get a sub-device made by a specified vendor
        /// </summary>
        /// <param name="vendorId">Vendor ID</param>
        /// <param name="deviceId">Device ID</param>
        /// <param name="subVendorId">Sub-vendor ID</param>
        /// <param name="subDeviceId">Sub-device ID</param>
        /// <returns>Sub-device information</returns>
        public static UsbDeviceInfo GetSubDevice(int vendorId, int deviceId, int subVendorId, int subDeviceId)
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

        /// <summary>
        /// Gets a class
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <returns>Class information</returns>
        public static UsbDeviceClassInfo GetClass(int classId)
        {
            var classes = ListClasses();
            foreach (var classType in classes)
                if (classType.Id == classId)
                    return classType;
            throw new ArgumentException($"Class ID {classId} not found.");
        }

        /// <summary>
        /// Checks to see if a class is registered
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsClassRegistered(int classId)
        {
            var classes = ListClasses();
            foreach (var classType in classes)
                if (classType.Id == classId)
                    return true;
            return false;
        }

        /// <summary>
        /// Lists all the subclasses from a class
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <returns>List of subclasses</returns>
        public static UsbDeviceSubclassInfo[] ListSubclasses(int classId) =>
            GetClass(classId).Subclasses;

        /// <summary>
        /// Gets a subclass from a class
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <param name="subclassId">Subclass ID</param>
        /// <returns>Subclass information</returns>
        public static UsbDeviceSubclassInfo GetSubclass(int classId, int subclassId)
        {
            var subclasses = ListSubclasses(classId);
            if (subclasses.Length == 0)
                throw new ArgumentException($"Class ID {classId} doesn't have any subclasses.");
            foreach (var subclass in subclasses)
                if (subclass.Id == subclassId)
                    return subclass;
            throw new ArgumentException($"Subclass ID {subclassId} not found.");
        }

        /// <summary>
        /// Checks to see if a subclass from a class is registered
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <param name="subclassId">Subclass ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsSubclassRegistered(int classId, int subclassId)
        {
            var subclasses = ListSubclasses(classId);
            if (subclasses.Length == 0)
                return false;
            foreach (var subclass in subclasses)
                if (subclass.Id == subclassId)
                    return true;
            return false;
        }

        /// <summary>
        /// Lists all the protocols from a subclass of a class
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <param name="subclassId">Subclass ID</param>
        /// <returns>List of protocols</returns>
        public static UsbDeviceProtocolInfo[] ListProtocols(int classId, int subclassId) =>
            GetSubclass(classId, subclassId).Protocols;

        /// <summary>
        /// Get an protocol from a subclass of a class
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <param name="subclassId">Subclass ID</param>
        /// <param name="protocolId">Protocol ID</param>
        /// <returns>Protocol information</returns>
        public static UsbDeviceProtocolInfo GetProtocol(int classId, int subclassId, int protocolId)
        {
            var protocols = ListProtocols(classId, subclassId);
            if (protocols.Length == 0)
                throw new ArgumentException($"Class ID {classId} doesn't have any protocols that subclass ID {subclassId} uses.");
            foreach (var protocolInfo in protocols)
                if (protocolInfo.Id == protocolId)
                    return protocolInfo;
            throw new ArgumentException($"Protocol ID {protocolId} not found.");
        }

        /// <summary>
        /// Checks to see if an protocol from a subclass of a class is registered
        /// </summary>
        /// <param name="classId">Class ID</param>
        /// <param name="subclassId">Subclass ID</param>
        /// <param name="protocolId">Protocol ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsProtocolRegistered(int classId, int subclassId, int protocolId)
        {
            var protocols = ListProtocols(classId, subclassId);
            if (protocols.Length == 0)
                return false;
            foreach (var protocolInfo in protocols)
                if (protocolInfo.Id == protocolId)
                    return true;
            return false;
        }

        private static void SerializeUsbList()
        {
            // Get the USB ID lines and parse all the vendors
            var lines = GetUsbIdsLines();
            List<UsbVendorInfo> vendors = [];
            List<UsbDeviceInfo> devices = [];
            List<UsbDeviceInfo> subDevices = [];
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
            // Get the USB ID lines and parse all the classes
            var lines = GetUsbIdsLines();
            List<UsbDeviceClassInfo> classes = [];
            List<UsbDeviceSubclassInfo> subclasses = [];
            List<UsbDeviceProtocolInfo> protocols = [];
            bool skipped = false;
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Break if we've reached audio class terminal types
                if (line.StartsWith("AT 0100"))
                    break;

                // Start parsing if we've reached the classes section
                if (line.StartsWith("C 00") && !skipped)
                    skipped = true;
                else if (!skipped)
                    continue;

                // Count the number of tabs to indicate either a class, a subclass, or an protocol
                bool IsProtocol = line[0] == '\t' && line[1] == '\t';
                bool IsSubclass = line[0] == '\t' && !IsProtocol;
                bool IsClass = !IsSubclass && !IsProtocol;
                if (IsClass)
                {
                    // Save the changes if we have a class
                    if (classes.Count > 0)
                    {
                        classes[classes.Count - 1].subclasses = [.. subclasses];
                        var finalSubclasses = classes[classes.Count - 1].subclasses;
                        if (finalSubclasses.Length > 0 && protocols.Count > 0)
                            finalSubclasses[finalSubclasses.Length - 1].protocols = [.. protocols];
                    }

                    // Clear the subclasses and the protocols since we have a new class
                    subclasses.Clear();
                    protocols.Clear();

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
                    if (subclasses.Count > 0 && protocols.Count > 0)
                        subclasses[subclasses.Count - 1].protocols = [.. protocols];

                    // Clear the protocols since we have a new subclass
                    protocols.Clear();

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
                else if (IsProtocol)
                {
                    // Some variables
                    string name = "";
                    int protocolId = 0;

                    // Now, parse a protocol line
                    string protocolIdString = line.Substring(2, 4);
                    name = line.Substring(6);
                    protocolId = int.Parse(protocolIdString, NumberStyles.HexNumber);

                    // Make a new class class (blanket)
                    protocols.Add(new(name, protocolId));
                }
            }
            cachedClasses = [.. classes];
        }

        private static string[] GetUsbIdsLines()
        {
            // Open the USB ID list stream (source: http://www.linux-usb.org/usb-ids.html)
            var stream = typeof(UsbListParser).Assembly.GetManifestResourceStream("SpecProbe.Usb.List.usb.ids");
            var reader = new StreamReader(stream);

            // Get the lines
            string[] lines = reader.ReadToEnd().SplitNewLines();
            return lines;
        }

        static UsbListParser()
        {
            if (cachedVendors.Length == 0)
                SerializeUsbList();
            if (cachedClasses.Length == 0)
                SerializeClassList();
        }
    }
}
