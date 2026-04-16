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
using SpecProbe.Parts.Types;
using SpecProbe.Probers.Platform;
using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using Textify.General;

namespace SpecProbe.Probers.Types.Video
{
    internal static partial class VideoProber
    {
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
    }
}
