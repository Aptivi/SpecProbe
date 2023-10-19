﻿
// SpecProbe  Copyright (C) 2020-2021  Aptivi
// 
// This file is part of SpecProbe
// 
// SpecProbe is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SpecProbe is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using SpecProbe.Hardware.Parts;
using SpecProbe.Hardware.Parts.Types;
using SpecProbe.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace SpecProbe.Hardware.Probers
{
    internal class VideoProber : IHardwareProber
    {
        public BaseHardwarePartInfo[] GetBaseHardwareParts()
        {
            if (PlatformHelper.IsOnWindows())
                return GetBaseHardwarePartsWindows();
            else if (PlatformHelper.IsOnMacOS())
                return GetBaseHardwarePartsMacOS();
            else
                return GetBaseHardwarePartsLinux();
        }

        public BaseHardwarePartInfo[] GetBaseHardwarePartsLinux()
        {
            // Some variables to install.
            string videoCardName = "";

            try
            {
                // Check to see if we have a /proc/driver/nvidia folder for systems running NVIDIA graphics
                string nvidiaGpuListDirectory = $"/proc/driver/nvidia/gpus";
                if (Directory.Exists(nvidiaGpuListDirectory))
                {
                    // The system is running proprietary NVIDIA drivers. Use the interface for performance
                    string[] nvidiaGpuFolders = Directory.GetDirectories(nvidiaGpuListDirectory).Where((dir) => !dir.EndsWith("power")).ToArray();
                    List<VideoPart> videos = new();
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
                                videoCardName = gpuStateStr[modelTag.Length..];
                            }
                        }

                        // Add a GPU
                        videos.Add(new VideoPart()
                        {
                            VideoCardName = videoCardName,
                        });
                    }
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
                            videoCardName = glxinfoOutputLine[rendererTag.Length..];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HardwareProber.errors.Add(ex);
            }

            // Finally, return a single item array containing information
            VideoPart part = new()
            {
                VideoCardName = videoCardName,
            };
            return new[] { part };
        }

        public BaseHardwarePartInfo[] GetBaseHardwarePartsMacOS()
        {
            throw new NotImplementedException();
        }

        public BaseHardwarePartInfo[] GetBaseHardwarePartsWindows()
        {
            List<VideoPart> parts = new();

            try
            {
                // Employ EnumDisplayDevices() and keep enumerating until we find the last device
                var devInfo = new PlatformWindowsInterop.DISPLAY_DEVICE();
                devInfo.cb = Marshal.SizeOf(devInfo);
                uint devNum = 0;
                while (PlatformWindowsInterop.EnumDisplayDevices(null, devNum, ref devInfo, 1))
                {
                    parts.Add(new VideoPart()
                    {
                        VideoCardName = devInfo.DeviceString
                    });
                    devNum++;
                    devInfo.cb = Marshal.SizeOf(devInfo);
                }
            }
            catch (Exception ex)
            {
                HardwareProber.errors.Add(ex);
            }

            // Finally, return an array containing information
            return parts.ToArray();
        }
    }
}
