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
using SpecProbe.Loader.Native.Helpers;
using SpecProbe.Loader.Native.Structs;
using SpecProbe.Parts.Types;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SpecProbe.Probers.Types.Video
{
    internal static partial class VideoProber
    {
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
