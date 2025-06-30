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
        private static UsbAudioTerminalInfo[] cachedAudioTerminals = [];
        private static UsbHidInfo[] cachedHids = [];
        private static UsbHidItemInfo[] cachedHidItems = [];
        private static UsbPhysicalBiasInfo[] cachedPhysicalBiases = [];
        private static UsbPhysicalDescriptorInfo[] cachedPhysicalDescriptors = [];
        private static UsbHidUsagePageInfo[] cachedHidUsagePages = [];
        private static UsbLanguageInfo[] cachedLanguages = [];
        private static UsbCountryCodeInfo[] cachedCountryCodes = [];
        private static UsbVideoTerminalInfo[] cachedVideoTerminals = [];

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
        /// Lists all the audio terminals
        /// </summary>
        /// <returns>List of audio terminals</returns>
        public static UsbAudioTerminalInfo[] ListAudioTerminals() =>
            cachedAudioTerminals;

        /// <summary>
        /// Lists all the HIDs
        /// </summary>
        /// <returns>List of HIDs</returns>
        public static UsbHidInfo[] ListHids() =>
            cachedHids;

        /// <summary>
        /// Lists all the HID items
        /// </summary>
        /// <returns>List of HID items</returns>
        public static UsbHidItemInfo[] ListHidItems() =>
            cachedHidItems;

        /// <summary>
        /// Lists all the physical biases
        /// </summary>
        /// <returns>List of physical biases</returns>
        public static UsbPhysicalBiasInfo[] ListPhysicalBiases() =>
            cachedPhysicalBiases;

        /// <summary>
        /// Lists all the physical descriptors
        /// </summary>
        /// <returns>List of physical descriptors</returns>
        public static UsbPhysicalDescriptorInfo[] ListPhysicalDescriptors() =>
            cachedPhysicalDescriptors;

        /// <summary>
        /// Lists all the HID usage pages
        /// </summary>
        /// <returns>List of HID usage pages</returns>
        public static UsbHidUsagePageInfo[] ListHidUsagePages() =>
            cachedHidUsagePages;

        /// <summary>
        /// Lists all the languages
        /// </summary>
        /// <returns>List of languages</returns>
        public static UsbLanguageInfo[] ListLanguages() =>
            cachedLanguages;

        /// <summary>
        /// Lists all the country codes
        /// </summary>
        /// <returns>List of country codes</returns>
        public static UsbCountryCodeInfo[] ListCountryCodes() =>
            cachedCountryCodes;

        /// <summary>
        /// Lists all the video terminals
        /// </summary>
        /// <returns>List of video terminals</returns>
        public static UsbVideoTerminalInfo[] ListVideoTerminals() =>
            cachedVideoTerminals;

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
            throw new ArgumentException("Vendor ID {0} not found.".FormatString(vendorId));
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
                throw new ArgumentException("Vendor ID {0} doesn't have any devices.".FormatString(vendorId));
            foreach (var device in devices)
                if (device.Id == deviceId)
                    return device;
            throw new ArgumentException("Device ID {0} not found.".FormatString(deviceId));
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
                throw new ArgumentException("Vendor ID {0} doesn't have any sub-devices that device ID {1} uses.".FormatString(vendorId, deviceId));
            foreach (var device in devices)
                if (device.VendorId == subVendorId && device.Id == subDeviceId)
                    return device;
            throw new ArgumentException("Device ID {0} not found.".FormatString(deviceId));
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
            throw new ArgumentException("Class ID {0} not found.".FormatString(classId));
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
                throw new ArgumentException("Class ID {0} doesn't have any subclasses.".FormatString(classId));
            foreach (var subclass in subclasses)
                if (subclass.Id == subclassId)
                    return subclass;
            throw new ArgumentException("Subclass ID {0} not found.".FormatString(subclassId));
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
                throw new ArgumentException("Class ID {0} doesn't have any protocols that subclass ID {1} uses.".FormatString(classId, subclassId));
            foreach (var protocolInfo in protocols)
                if (protocolInfo.Id == protocolId)
                    return protocolInfo;
            throw new ArgumentException("Protocol ID {0} not found.".FormatString(protocolId));
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

        /// <summary>
        /// Gets an audio terminal
        /// </summary>
        /// <param name="audioTerminalId">Audio terminal ID</param>
        /// <returns>Audio terminal information</returns>
        public static UsbAudioTerminalInfo GetAudioTerminal(int audioTerminalId)
        {
            var audioTerminals = ListAudioTerminals();
            foreach (var audioTerminalType in audioTerminals)
                if (audioTerminalType.Id == audioTerminalId)
                    return audioTerminalType;
            throw new ArgumentException("Audio terminal ID {0} not found.".FormatString(audioTerminalId));
        }

        /// <summary>
        /// Checks to see if an audio terminal is registered
        /// </summary>
        /// <param name="audioTerminalId">Audio terminal ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsAudioTerminalRegistered(int audioTerminalId)
        {
            var audioTerminals = ListAudioTerminals();
            foreach (var audioTerminalType in audioTerminals)
                if (audioTerminalType.Id == audioTerminalId)
                    return true;
            return false;
        }

        /// <summary>
        /// Gets an HID
        /// </summary>
        /// <param name="hidId">HID ID</param>
        /// <returns>HID information</returns>
        public static UsbHidInfo GetHid(int hidId)
        {
            var hids = ListHids();
            foreach (var hidType in hids)
                if (hidType.Id == hidId)
                    return hidType;
            throw new ArgumentException("HID ID {0} not found.".FormatString(hidId));
        }

        /// <summary>
        /// Checks to see if an HID is registered
        /// </summary>
        /// <param name="hidId">HID ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsHidRegistered(int hidId)
        {
            var hids = ListHids();
            foreach (var hidType in hids)
                if (hidType.Id == hidId)
                    return true;
            return false;
        }

        /// <summary>
        /// Gets an HID item
        /// </summary>
        /// <param name="hidItemId">HID item ID</param>
        /// <returns>HID item information</returns>
        public static UsbHidItemInfo GetHidItem(int hidItemId)
        {
            var hidItems = ListHidItems();
            foreach (var hidItemType in hidItems)
                if (hidItemType.Id == hidItemId)
                    return hidItemType;
            throw new ArgumentException("HID item ID {0} not found.".FormatString(hidItemId));
        }

        /// <summary>
        /// Checks to see if an HID item is registered
        /// </summary>
        /// <param name="hidItemId">HID item ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsHidItemRegistered(int hidItemId)
        {
            var hidItems = ListHidItems();
            foreach (var hidItemType in hidItems)
                if (hidItemType.Id == hidItemId)
                    return true;
            return false;
        }

        /// <summary>
        /// Gets a physical bias
        /// </summary>
        /// <param name="physicalBiasId">physical bias ID</param>
        /// <returns>physical bias information</returns>
        public static UsbPhysicalBiasInfo GetPhysicalBias(int physicalBiasId)
        {
            var physicalBiass = ListPhysicalBiases();
            foreach (var physicalBiasType in physicalBiass)
                if (physicalBiasType.Id == physicalBiasId)
                    return physicalBiasType;
            throw new ArgumentException("HID physical bias ID {0} not found.".FormatString(physicalBiasId));
        }

        /// <summary>
        /// Checks to see if a physical bias is registered
        /// </summary>
        /// <param name="physicalBiasId">physical bias ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsPhysicalBiasRegistered(int physicalBiasId)
        {
            var physicalBiass = ListPhysicalBiases();
            foreach (var physicalBiasType in physicalBiass)
                if (physicalBiasType.Id == physicalBiasId)
                    return true;
            return false;
        }

        /// <summary>
        /// Gets a physical descriptor
        /// </summary>
        /// <param name="physicalDescriptorId">physical descriptor ID</param>
        /// <returns>Physical descriptor information</returns>
        public static UsbPhysicalDescriptorInfo GetPhysicalDescriptor(int physicalDescriptorId)
        {
            var physicalDescriptors = ListPhysicalDescriptors();
            foreach (var physicalDescriptorType in physicalDescriptors)
                if (physicalDescriptorType.Id == physicalDescriptorId)
                    return physicalDescriptorType;
            throw new ArgumentException("HID physical descriptor ID {0} not found.".FormatString(physicalDescriptorId));
        }

        /// <summary>
        /// Checks to see if a physical descriptor is registered
        /// </summary>
        /// <param name="physicalDescriptorId">Physical descriptor ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsPhysicalDescriptorRegistered(int physicalDescriptorId)
        {
            var physicalDescriptors = ListPhysicalDescriptors();
            foreach (var physicalDescriptorType in physicalDescriptors)
                if (physicalDescriptorType.Id == physicalDescriptorId)
                    return true;
            return false;
        }

        /// <summary>
        /// Gets a HID usage page
        /// </summary>
        /// <param name="hidUsagePageId">HID usage page ID</param>
        /// <returns>HidUsagePage information</returns>
        public static UsbHidUsagePageInfo GetHidUsagePage(int hidUsagePageId)
        {
            var hidUsagePages = ListHidUsagePages();
            foreach (var hidUsagePage in hidUsagePages)
                if (hidUsagePage.Id == hidUsagePageId)
                    return hidUsagePage;
            throw new ArgumentException("HID usage page ID {0} not found.".FormatString(hidUsagePageId));
        }

        /// <summary>
        /// Checks to see if a HID usage page is registered
        /// </summary>
        /// <param name="hidUsagePageId">HID usage page ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsHidUsagePageRegistered(int hidUsagePageId)
        {
            var hidUsagePages = ListHidUsagePages();
            foreach (var hidUsagePage in hidUsagePages)
                if (hidUsagePage.Id == hidUsagePageId)
                    return true;
            return false;
        }

        /// <summary>
        /// Lists all the HidUsage from a HID usage page
        /// </summary>
        /// <param name="hidUsagePageId">HID usage page ID</param>
        /// <returns>List of HidUsage</returns>
        public static UsbHidUsageInfo[] ListHidUsages(int hidUsagePageId) =>
            GetHidUsagePage(hidUsagePageId).Usages;

        /// <summary>
        /// Gets an HID usage from a HID usage page
        /// </summary>
        /// <param name="hidUsagePageId">HID usage page ID</param>
        /// <param name="hidUsageId">HID usage ID</param>
        /// <returns>hidUsage information</returns>
        public static UsbHidUsageInfo GetHidUsage(int hidUsagePageId, int hidUsageId)
        {
            var hidUsages = ListHidUsages(hidUsagePageId);
            if (hidUsages.Length == 0)
                throw new ArgumentException("HID usage page ID {0} doesn't have any HID Usage.".FormatString(hidUsagePageId));
            foreach (var hidUsage in hidUsages)
                if (hidUsage.Id == hidUsageId)
                    return hidUsage;
            throw new ArgumentException("HID usage ID {0} not found.".FormatString(hidUsageId));
        }

        /// <summary>
        /// Checks to see if an HID usage from a HID usage page is registered
        /// </summary>
        /// <param name="hidUsagePageId">HID usage page ID</param>
        /// <param name="hidUsageId">HID usage ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IshidUsageRegistered(int hidUsagePageId, int hidUsageId)
        {
            var HidUsage = ListHidUsages(hidUsagePageId);
            if (HidUsage.Length == 0)
                return false;
            foreach (var hidUsage in HidUsage)
                if (hidUsage.Id == hidUsageId)
                    return true;
            return false;
        }

        /// <summary>
        /// Gets a language
        /// </summary>
        /// <param name="languageId">Language ID</param>
        /// <returns>Language information</returns>
        public static UsbLanguageInfo GetLanguage(int languageId)
        {
            var languages = ListLanguages();
            foreach (var language in languages)
                if (language.Id == languageId)
                    return language;
            throw new ArgumentException("Language ID {0} not found.".FormatString(languageId));
        }

        /// <summary>
        /// Checks to see if a language is registered
        /// </summary>
        /// <param name="languageId">Language ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsLanguageRegistered(int languageId)
        {
            var languages = ListLanguages();
            foreach (var language in languages)
                if (language.Id == languageId)
                    return true;
            return false;
        }

        /// <summary>
        /// Lists all the dialects from a language
        /// </summary>
        /// <param name="languageId">Language ID</param>
        /// <returns>List of Dialect</returns>
        public static UsbLanguageDialectInfo[] ListDialects(int languageId) =>
            GetLanguage(languageId).Dialects;

        /// <summary>
        /// Gets an HID usage from a language
        /// </summary>
        /// <param name="languageId">Language ID</param>
        /// <param name="dialectId">HID usage ID</param>
        /// <returns>dialect information</returns>
        public static UsbLanguageDialectInfo GetDialect(int languageId, int dialectId)
        {
            var dialects = ListDialects(languageId);
            if (dialects.Length == 0)
                throw new ArgumentException("Language ID {0} doesn't have any dialects.".FormatString(languageId));
            foreach (var dialect in dialects)
                if (dialect.Id == dialectId)
                    return dialect;
            throw new ArgumentException("Dialect ID {0} not found.".FormatString(dialectId));
        }

        /// <summary>
        /// Checks to see if an HID usage from a language is registered
        /// </summary>
        /// <param name="languageId">Language ID</param>
        /// <param name="dialectId">HID usage ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsDialectRegistered(int languageId, int dialectId)
        {
            var Dialect = ListDialects(languageId);
            if (Dialect.Length == 0)
                return false;
            foreach (var dialect in Dialect)
                if (dialect.Id == dialectId)
                    return true;
            return false;
        }

        /// <summary>
        /// Gets a country code
        /// </summary>
        /// <param name="countryCodeId">country code ID</param>
        /// <returns>Country code information</returns>
        public static UsbCountryCodeInfo GetCountryCode(int countryCodeId)
        {
            var countryCodes = ListCountryCodes();
            foreach (var countryCodeType in countryCodes)
                if (countryCodeType.Id == countryCodeId)
                    return countryCodeType;
            throw new ArgumentException("Country code ID {0} not found.".FormatString(countryCodeId));
        }

        /// <summary>
        /// Checks to see if a country code is registered
        /// </summary>
        /// <param name="countryCodeId">Country code ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsCountryCodeRegistered(int countryCodeId)
        {
            var countryCodes = ListCountryCodes();
            foreach (var countryCodeType in countryCodes)
                if (countryCodeType.Id == countryCodeId)
                    return true;
            return false;
        }

        /// <summary>
        /// Gets a video terminal
        /// </summary>
        /// <param name="videoTerminalId">Video terminal ID</param>
        /// <returns>Video terminal information</returns>
        public static UsbVideoTerminalInfo GetVideoTerminal(int videoTerminalId)
        {
            var videoTerminals = ListVideoTerminals();
            foreach (var videoTerminalType in videoTerminals)
                if (videoTerminalType.Id == videoTerminalId)
                    return videoTerminalType;
            throw new ArgumentException("Video terminal ID {0} not found.".FormatString(videoTerminalId));
        }

        /// <summary>
        /// Checks to see if a video terminal is registered
        /// </summary>
        /// <param name="videoTerminalId">Video terminal ID</param>
        /// <returns>True if registered; false otherwise.</returns>
        public static bool IsVideoTerminalRegistered(int videoTerminalId)
        {
            var videoTerminals = ListVideoTerminals();
            foreach (var videoTerminalType in videoTerminals)
                if (videoTerminalType.Id == videoTerminalId)
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

                    // Now, parse a vendor line
                    string idString = line.Substring(0, 4);
                    string name = line.Substring(6);
                    int id = int.Parse(idString, NumberStyles.HexNumber);

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

                    // Now, parse a device line
                    string idString = line.Substring(1, 4);
                    string name = line.Substring(7);
                    int id = int.Parse(idString, NumberStyles.HexNumber);

                    // Make a new vendor class (blanket)
                    devices.Add(new(name, id, vendors[vendors.Count - 1].Id));
                }
                else if (IsSubDevice)
                {
                    // Now, parse a subdevice line
                    string subVendorIdString = line.Substring(2, 4);
                    string idString = line.Substring(7, 4);
                    string name = line.Substring(13);
                    int subVendorId = int.Parse(subVendorIdString, NumberStyles.HexNumber);
                    int id = int.Parse(idString, NumberStyles.HexNumber);

                    // Make a new vendor class (blanket)
                    subDevices.Add(new(name, id, subVendorId));
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

                // Break if we've reached audio terminal types
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

                    // Now, parse a class line
                    string classIdString = line.Substring(2, 2);
                    string name = line.Substring(6);
                    int classId = int.Parse(classIdString, NumberStyles.HexNumber);

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

                    // Now, parse a subclass line
                    string subclassIdString = line.Substring(1, 2);
                    string name = line.Substring(5);
                    int subclassId = int.Parse(subclassIdString, NumberStyles.HexNumber);

                    // Make a new class class (blanket)
                    subclasses.Add(new(name, subclassId));
                }
                else if (IsProtocol)
                {
                    // Now, parse a protocol line
                    string protocolIdString = line.Substring(2, 4);
                    string name = line.Substring(6);
                    int protocolId = int.Parse(protocolIdString, NumberStyles.HexNumber);

                    // Make a new class class (blanket)
                    protocols.Add(new(name, protocolId));
                }
            }
            cachedClasses = [.. classes];
        }

        private static void SerializeAudioTerminalList()
        {
            // Get the USB ID lines and parse all the audio terminals
            var lines = GetUsbIdsLines();
            List<UsbAudioTerminalInfo> terminals = [];
            bool skipped = false;
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Break if we've reached human interface device descriptor types
                if (line.StartsWith("HID 21"))
                    break;

                // Start parsing if we've reached the audio terminal section
                if (line.StartsWith("AT 0100") && !skipped)
                    skipped = true;
                else if (!skipped)
                    continue;

                // Now, parse an audio terminal line
                string classIdString = line.Substring(3, 4);
                string name = line.Substring(9);
                int classId = int.Parse(classIdString, NumberStyles.HexNumber);

                // Make a new terminal class (blanket)
                terminals.Add(new(name, classId));
            }
            cachedAudioTerminals = [.. terminals];
        }

        private static void SerializeHidList()
        {
            // Get the USB ID lines and parse all the HIDs
            var lines = GetUsbIdsLines();
            List<UsbHidInfo> hids = [];
            bool skipped = false;
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Break if we've reached HID descriptor item types
                if (line.StartsWith("R 04"))
                    break;

                // Start parsing if we've reached the HID section
                if (line.StartsWith("HID 21") && !skipped)
                    skipped = true;
                else if (!skipped)
                    continue;

                // Now, parse an HID line
                string classIdString = line.Substring(4, 2);
                string name = line.Substring(8);
                int classId = int.Parse(classIdString, NumberStyles.HexNumber);

                // Make a new HID class (blanket)
                hids.Add(new(name, classId));
            }
            cachedHids = [.. hids];
        }

        private static void SerializeHidItemList()
        {
            // Get the USB ID lines and parse all the HID items
            var lines = GetUsbIdsLines();
            List<UsbHidItemInfo> hidItems = [];
            bool skipped = false;
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Break if we've reached HID descriptor item types
                if (line.StartsWith("BIAS 0"))
                    break;

                // Start parsing if we've reached the HID item section
                if (line.StartsWith("R 04") && !skipped)
                    skipped = true;
                else if (!skipped)
                    continue;

                // Now, parse an HID item line
                string idString = line.Substring(2, 2);
                string name = line.Substring(6);
                int id = int.Parse(idString, NumberStyles.HexNumber);

                // Make a new HID class (blanket)
                hidItems.Add(new(name, id));
            }
            cachedHidItems = [.. hidItems];
        }

        private static void SerializePhysicalBiasList()
        {
            // Get the USB ID lines and parse all the physical biases
            var lines = GetUsbIdsLines();
            List<UsbPhysicalBiasInfo> physicalBiases = [];
            bool skipped = false;
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Break if we've reached HID descriptor item types
                if (line.StartsWith("PHY 00"))
                    break;

                // Start parsing if we've reached the physical bias section
                if (line.StartsWith("BIAS 0") && !skipped)
                    skipped = true;
                else if (!skipped)
                    continue;

                // Now, parse a physical bias line
                string idString = line.Substring(5, 1);
                string name = line.Substring(8);
                int id = int.Parse(idString, NumberStyles.HexNumber);

                // Make a new physical bias class (blanket)
                physicalBiases.Add(new(name, id));
            }
            cachedPhysicalBiases = [.. physicalBiases];
        }

        private static void SerializePhysicalDescriptorList()
        {
            // Get the USB ID lines and parse all the physical descriptors
            var lines = GetUsbIdsLines();
            List<UsbPhysicalDescriptorInfo> physicalDescriptors = [];
            bool skipped = false;
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Break if we've reached HID usage types
                if (line.StartsWith("HUT 00"))
                    break;

                // Start parsing if we've reached the physical descriptor section
                if (line.StartsWith("PHY 00") && !skipped)
                    skipped = true;
                else if (!skipped)
                    continue;

                // Now, parse a physical descriptor line
                string idString = line.Substring(4, 2);
                string name = line.Substring(8);
                int id = int.Parse(idString, NumberStyles.HexNumber);

                // Make a new physical descriptor class (blanket)
                physicalDescriptors.Add(new(name, id));
            }
            cachedPhysicalDescriptors = [.. physicalDescriptors];
        }

        private static void SerializeHidUsageIdList()
        {
            // Get the USB ID lines and parse all the hidUsagePageIds
            var lines = GetUsbIdsLines();
            List<UsbHidUsagePageInfo> hidUsagePageIds = [];
            List<UsbHidUsageInfo> hidUsageIds = [];
            bool skipped = false;
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Break if we've reached audio terminal types
                if (line.StartsWith("L 0001"))
                    break;

                // Start parsing if we've reached the hidUsagePageIds section
                if (line.StartsWith("HUT 00") && !skipped)
                    skipped = true;
                else if (!skipped)
                    continue;

                // Count the number of tabs to indicate either an HID usage or an HID usage page ID
                bool isHidUsageId = line[0] == '\t';
                bool isHidUsagePageId = !isHidUsageId;
                if (isHidUsagePageId)
                {
                    // Save the changes if we have an HID usage ID
                    if (hidUsagePageIds.Count > 0)
                        hidUsagePageIds[hidUsagePageIds.Count - 1].usages = [.. hidUsageIds];

                    // Clear HID usage IDs since we have a new HID usage page ID
                    hidUsageIds.Clear();

                    // Now, parse a HID usage page ID line
                    string hidUsageIdIdString = line.Substring(4, 2);
                    string name = line.Substring(8);
                    int hidUsageId = int.Parse(hidUsageIdIdString, NumberStyles.HexNumber);

                    // Make a new HID usage page ID class (blanket)
                    hidUsagePageIds.Add(new(name, hidUsageId));
                }
                else if (isHidUsageId)
                {
                    // Now, parse a hidusageid line
                    string hidusageidIdString = line.Substring(1, 3);
                    string name = line.Substring(6);
                    int hidUsageId = int.Parse(hidusageidIdString, NumberStyles.HexNumber);

                    // Make a new hidusageid hidusageid (blanket)
                    hidUsageIds.Add(new(name, hidUsageId));
                }
            }
            cachedHidUsagePages = [.. hidUsagePageIds];
        }

        private static void SerializeLanguageList()
        {
            // Get the USB ID lines and parse all the languages
            var lines = GetUsbIdsLines();
            List<UsbLanguageInfo> languages = [];
            List<UsbLanguageDialectInfo> dialects = [];
            bool skipped = false;
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Break if we've reached country codes
                if (line.StartsWith("HCC 00"))
                    break;

                // Start parsing if we've reached the languages section
                if (line.StartsWith("L 0001") && !skipped)
                    skipped = true;
                else if (!skipped)
                    continue;

                // Count the number of tabs to indicate either a lanaguage or a dialect
                bool isDialect = line[0] == '\t';
                bool isLanguage = !isDialect;
                if (isLanguage)
                {
                    // Save the changes if we have dialects
                    if (languages.Count > 0)
                        languages[languages.Count - 1].dialects = [.. dialects];

                    // Clear HID usage IDs since we have a new HID usage page ID
                    dialects.Clear();

                    // Now, parse a HID usage page ID line
                    string languageIdString = line.Substring(2, 4);
                    string name = line.Substring(8);
                    int language = int.Parse(languageIdString, NumberStyles.HexNumber);

                    // Make a new HID usage page ID class (blanket)
                    languages.Add(new(name, language));
                }
                else if (isDialect)
                {
                    // Now, parse a dialect line
                    string dialectIdString = line.Substring(1, 2);
                    string name = line.Substring(5);
                    int dialect = int.Parse(dialectIdString, NumberStyles.HexNumber);

                    // Make a new dialect dialect (blanket)
                    dialects.Add(new(name, dialect));
                }
            }
            cachedLanguages = [.. languages];
        }

        private static void SerializeCountryCodeList()
        {
            // Get the USB ID lines and parse all the country codes
            var lines = GetUsbIdsLines();
            List<UsbCountryCodeInfo> countryCodes = [];
            bool skipped = false;
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Break if we've reached video class terminal types
                if (line.StartsWith("VT 0100"))
                    break;

                // Start parsing if we've reached the country code section
                if (line.StartsWith("HCC 00") && !skipped)
                    skipped = true;
                else if (!skipped)
                    continue;

                // Now, parse a country code line
                string idString = line.Substring(4, 2);
                string name = line.Substring(8);
                int id = int.Parse(idString, NumberStyles.HexNumber);

                // Make a new country code class (blanket)
                countryCodes.Add(new(name, id));
            }
            cachedCountryCodes = [.. countryCodes];
        }

        private static void SerializeVideoTerminalList()
        {
            // Get the USB ID lines and parse all the video terminals
            var lines = GetUsbIdsLines();
            List<UsbVideoTerminalInfo> terminals = [];
            bool skipped = false;
            foreach (string line in lines)
            {
                // Ignore comments and empty lines
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                // Start parsing if we've reached the video terminal section
                if (line.StartsWith("VT 0100") && !skipped)
                    skipped = true;
                else if (!skipped)
                    continue;

                // Now, parse an video terminal line
                string classIdString = line.Substring(3, 4);
                string name = line.Substring(9);
                int classId = int.Parse(classIdString, NumberStyles.HexNumber);

                // Make a new terminal class (blanket)
                terminals.Add(new(name, classId));
            }
            cachedVideoTerminals = [.. terminals];
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
            if (cachedAudioTerminals.Length == 0)
                SerializeAudioTerminalList();
            if (cachedHids.Length == 0)
                SerializeHidList();
            if (cachedHidItems.Length == 0)
                SerializeHidItemList();
            if (cachedPhysicalBiases.Length == 0)
                SerializePhysicalBiasList();
            if (cachedPhysicalDescriptors.Length == 0)
                SerializePhysicalDescriptorList();
            if (cachedHidUsagePages.Length == 0)
                SerializeHidUsageIdList();
            if (cachedLanguages.Length == 0)
                SerializeLanguageList();
            if (cachedCountryCodes.Length == 0)
                SerializeCountryCodeList();
            if (cachedVideoTerminals.Length == 0)
                SerializeVideoTerminalList();
        }
    }
}
