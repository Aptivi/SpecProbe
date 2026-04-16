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

using SpecProbe.Parts.Types;
using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SpecProbe.Probers.Types.Video
{
    internal static partial class VideoProber
    {
        public static VideoPart[] ProbeUnix(out Exception[] errors)
        {
            // Some variables to install.
            List<Exception> exceptions = [];
            string videoCardName = "";
            uint vendorId = 0;
            uint modelId = 0;

            try
            {
                string glxInfoPath = File.Exists("/usr/local/bin/glxinfo") ? "/usr/local/bin/glxinfo" : "/usr/bin/glxinfo";
                string glxinfoOutput = PlatformHelper.ExecuteProcessToString(glxInfoPath, "-B");
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
    }
}
