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
using SpecProbe.Native.Helpers;
using SpecProbe.Native.Structs;
using SpecProbe.Parts.Types;
using SpecProbe.Probers.Platform;
using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Textify.General;

namespace SpecProbe.Probers
{
    internal static class VideoProber
    {
        public static VideoPart[] Probe(out Exception[] errors)
        {
            if (PlatformHelper.IsOnWindows())
                return ProbeWindows(out errors);
            else if (PlatformHelper.IsOnMacOS())
                return ProbeMacOS(out errors);
            else
                return ProbeLinux(out errors);
        }

        public static VideoPart[] ProbeLinux(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            string videoCardName = "";
            uint vendorId = 0;
            uint modelId = 0;

            try
            {
                string glxinfoOutput = PlatformHelper.ExecuteProcessToString("/usr/bin/glxinfo", "-B");
                string[] glxinfoOutputLines = glxinfoOutput.Replace("\r", "").Split('\n');
                foreach (string glxinfoOutputLine in glxinfoOutputLines)
                {
                    string rendererTag = "OpenGL renderer string: ";
                    string vendorTag = "    Vendor: ";
                    string deviceTag = "    Device: ";
                    if (glxinfoOutputLine.StartsWith(rendererTag))
                    {
                        // Trim the tag to get the GPU name.
                        videoCardName = glxinfoOutputLine.Substring(rendererTag.Length);
                    }
                    if (glxinfoOutputLine.StartsWith(vendorTag))
                    {
                        // Trim the tag to get the GPU vendor name to get the ID.
                        string vendorName = glxinfoOutputLine.Substring(vendorTag.Length);
                        vendorName = vendorName.Substring(vendorName.LastIndexOf('(') + 1);
                        vendorName = vendorName.Substring(0, vendorName.IndexOf(')'));
                        vendorId = uint.Parse(vendorName.Substring(2), NumberStyles.HexNumber);
                    }
                    if (glxinfoOutputLine.StartsWith(deviceTag))
                    {
                        // Trim the tag to get the GPU device name to get the ID.
                        string deviceName = glxinfoOutputLine.Substring(deviceTag.Length);
                        deviceName = deviceName.Substring(deviceName.LastIndexOf('(') + 1);
                        deviceName = deviceName.Substring(0, deviceName.IndexOf(')'));
                        modelId = uint.Parse(deviceName.Substring(2), NumberStyles.HexNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            // Finally, return a single item array containing information
            VideoPart part = new()
            {
                VideoCardName = videoCardName,
                VendorId = vendorId,
                ModelId = modelId,
            };
            errors = [.. exceptions];
            return new[] { part };
        }

        public static VideoPart[] ProbeMacOS(out Exception[] errors)
        {
            // Video card list
            List<Exception> exceptions = [];
            List<VideoPart> videos = [];

            // Some tags
            string videoCardNameTag = "Device ID:";
            string videoCardVendorTag = "Vendor ID:";

            // Some variables to install.
            string videoCardName = "";
            string videoCardDevName = "";
            string videoCardVendor = "";

            try
            {
                // Check notarization status
                if (HardwareProber.notarized)
                    return ProbeMacOSNotarized(out errors);

                // Probe the video cards
                string sysctlOutput = PlatformHelper.ExecuteProcessToString("/usr/sbin/system_profiler", "SPDisplaysDataType");
                string[] sysctlOutputLines = sysctlOutput.Replace("\r", "").Split('\n');
                foreach (string sysctlOutputLine in sysctlOutputLines)
                {
                    string line = sysctlOutputLine.Trim();
                    if (line.StartsWith(videoCardNameTag))
                        videoCardDevName = line.Substring(videoCardNameTag.Length).Trim();
                    if (line.StartsWith(videoCardVendorTag))
                        videoCardVendor = line.Substring(videoCardVendorTag.Length).Trim();
                }
                videoCardName =
                    $"V: {videoCardVendor} " +
                    $"M: {videoCardDevName}";
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
            uint vendorId = uint.Parse(videoCardVendor.Substring(2), NumberStyles.HexNumber);
            uint modelId = uint.Parse(videoCardDevName.Substring(2), NumberStyles.HexNumber);

            // Finally, return a single item array containing information
            videos.Add(new VideoPart
            {
                VideoCardName = videoCardName,
                VendorId = vendorId,
                ModelId = modelId,
            });
            errors = [.. exceptions];
            return videos.ToArray();
        }

        public static VideoPart[] ProbeMacOSNotarized(out Exception[] errors)
        {
            // Video card list
            List<Exception> exceptions = [];
            List<VideoPart> videos = [];

            // Some variables to install.
            string videoCardName;

            try
            {
                // Check notarization status
                if (!HardwareProber.notarized)
                    return ProbeMacOS(out errors);

                // Probe the online displays
                var status = PlatformMacInterop.CGGetOnlineDisplayList(uint.MaxValue, null, out uint displays);
                if (status != PlatformMacInterop.CGError.kCGErrorSuccess)
                    throw new Exception(
                        LanguageTools.GetLocalized("SPECPROBE_PROBERS_EXCEPTION_CGQUARTZPROBEFAILED") + $" {status}" + "\n" +
                        "Check out {0} for more info.".FormatString($"https://developer.apple.com/documentation/coregraphics/cgerror/{status.ToString().ToLower()}")
                    );

                // Probe the screens
                uint[] screens = new uint[displays];
                status = PlatformMacInterop.CGGetOnlineDisplayList(uint.MaxValue, ref screens, out displays);
                if (status != PlatformMacInterop.CGError.kCGErrorSuccess)
                    throw new Exception(
                        LanguageTools.GetLocalized("SPECPROBE_PROBERS_EXCEPTION_CGQUARTZLISTFAILED") + $" {status}" + "\n" +
                        "Check out {0} for more info.".FormatString($"https://developer.apple.com/documentation/coregraphics/cgerror/{status.ToString().ToLower()}")
                    );

                // Probe the model and the vendor number as the video card name
                foreach (var screen in screens)
                {
                    uint vendorId = PlatformMacInterop.CGDisplayVendorNumber(screen);
                    uint modelId = PlatformMacInterop.CGDisplayModelNumber(screen);
                    videoCardName =
                        $"V: {vendorId} " +
                        $"M: {modelId}";

                    VideoPart part = new()
                    {
                        VideoCardName = videoCardName,
                        VendorId = vendorId,
                        ModelId = modelId,
                    };
                    videos.Add(part);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            // Finally, return an array containing information
            errors = [.. exceptions];
            return videos.ToArray();
        }

        public static VideoPart[] ProbeWindows(out Exception[] errors)
        {
            List<Exception> exceptions = [];
            List<VideoPart> parts = [];

            try
            {
                // Employ libdxhelper to get info about GPUs
                bool result = VideoHelper.GetGpus().Invoke(out IntPtr gpus, out int length);
                if (!result)
                    throw new Exception(LanguageTools.GetLocalized("SPECPROBE_PROBERS_EXCEPTION_CANTPARSEGPUS"));

                // Enumerate parsed GPUs
                for (int i = 0; i < length - 1; i++)
                {
                    // Get the GPU part
                    int size = Marshal.SizeOf(typeof(GpuInfo));
                    GpuInfo gpuPart = (GpuInfo)Marshal.PtrToStructure(gpus + (size * i), typeof(GpuInfo));

                    // Build the name
                    char[] nameChars = gpuPart.name;
                    StringBuilder builder = new();
                    for (int c = 0; c < nameChars.Length; c += 2)
                    {
                        // Get the character and check for null char
                        char character = nameChars[c];
                        if (character == '\0')
                            break;
                        builder.Append(character);
                    }

                    // Install the part
                    parts.Add(new VideoPart()
                    {
                        VideoCardName = builder.ToString(),
                        VendorId = (uint)gpuPart.vendorId,
                        ModelId = (uint)gpuPart.deviceId,
                    });
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }

            // Finally, return an array containing information
            errors = [.. exceptions];
            return parts.ToArray();
        }
    }
}
