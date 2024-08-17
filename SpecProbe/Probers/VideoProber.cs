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

using SpecProbe.Native.Helpers;
using SpecProbe.Native.Structs;
using SpecProbe.Parts.Types;
using SpecProbe.Probers.Platform;
using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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

            try
            {
                // Check to see if we have a /proc/driver/nvidia folder for systems running NVIDIA graphics
                string nvidiaGpuListDirectory = $"/proc/driver/nvidia/gpus";
                if (Directory.Exists(nvidiaGpuListDirectory))
                {
                    // The system is running proprietary NVIDIA drivers. Use the interface for performance
                    string[] nvidiaGpuFolders = Directory.GetDirectories(nvidiaGpuListDirectory).Where((dir) => !dir.EndsWith("power")).ToArray();
                    List<VideoPart> videos = [];
                    foreach (string nvidiaGpuFolder in nvidiaGpuFolders)
                    {
                        // Get information
                        string nvidiaGpuStateFile = $"{nvidiaGpuFolder}/information";
                        string[] nvidiaGpuStateStrs = File.ReadAllLines(nvidiaGpuStateFile);
                        foreach (string gpuStateStr in nvidiaGpuStateStrs)
                        {
                            string modelTag = "Model:       ";
                            if (gpuStateStr.StartsWith(modelTag))
                            {
                                // Trim the tag to get the GPU name, such as "GeForce GTX 680", "Tesla P100-PCIE-12GB", or "GeForce RTX 4090"
                                videoCardName = gpuStateStr.Substring(modelTag.Length);
                            }
                        }

                        // Add a GPU
                        videos.Add(new VideoPart()
                        {
                            VideoCardName = videoCardName,
                        });
                    }
                    errors = [.. exceptions];
                    return videos.ToArray();
                }
                else
                {
                    // Either the system is not running NVIDIA graphics card, or the system is not using proprietary NVIDIA driver.
                    // Roll back to glxinfo -B
                    string glxinfoOutput = PlatformHelper.ExecuteProcessToString("/usr/bin/glxinfo", "-B");
                    string[] glxinfoOutputLines = glxinfoOutput.Replace("\r", "").Split('\n');
                    foreach (string glxinfoOutputLine in glxinfoOutputLines)
                    {
                        string rendererTag = "OpenGL renderer string: ";
                        if (glxinfoOutputLine.StartsWith(rendererTag))
                        {
                            // Trim the tag to get the GPU name.
                            videoCardName = glxinfoOutputLine.Substring(rendererTag.Length);
                        }
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

            // Finally, return a single item array containing information
            videos.Add(new VideoPart
            {
                VideoCardName = videoCardName
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
                        $"CGGetOnlineDisplayList() probing part from Quartz failed: {status}\n" +
                        $"Check out https://developer.apple.com/documentation/coregraphics/cgerror/{status.ToString().ToLower()} for more info."
                    );

                // Probe the screens
                uint[] screens = new uint[displays];
                status = PlatformMacInterop.CGGetOnlineDisplayList(uint.MaxValue, ref screens, out displays);
                if (status != PlatformMacInterop.CGError.kCGErrorSuccess)
                    throw new Exception(
                        $"CGGetOnlineDisplayList() screen listing part from Quartz failed: {status}\n" +
                        $"Check out https://developer.apple.com/documentation/coregraphics/cgerror/{status.ToString().ToLower()} for more info."
                    );

                // Probe the model and the vendor number as the video card name
                foreach (var screen in screens)
                {
                    videoCardName =
                        $"V: {PlatformMacInterop.CGDisplayVendorNumber(screen)} " +
                        $"M: {PlatformMacInterop.CGDisplayModelNumber(screen)}";

                    VideoPart part = new()
                    {
                        VideoCardName = videoCardName,
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
                    throw new Exception("Can't parse video cards.");

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
                        VideoCardName = builder.ToString()
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
