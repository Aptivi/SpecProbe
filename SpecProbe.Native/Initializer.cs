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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using SpecProbe.Loader;
using SpecProbe.Software.Platform;

namespace SpecProbe.Native
{
    internal static class Initializer
    {
        internal static LibraryManager libManager;
        private static bool _initialized;

        internal static void InitializeNative()
        {
            if (_initialized)
                return;
            string libPath = GetLibraryPath("libspecprober");
            string libDxPath = GetLibraryPath("libdxhelper");

            // Detect the platform
            var platform = PlatformHelper.GetPlatform();
            var bitness = PlatformHelper.GetArchitecture();
            switch (platform)
            {
                case Platform.Windows:
                    libManager = bitness switch
                    {
                        Architecture.X64 =>
                            new LibraryManager(new LibraryFile(libPath), new LibraryFile(libDxPath)),
                        Architecture.Arm64 =>
                            new LibraryManager(new LibraryFile(libDxPath)),
                        _ =>
                            throw new PlatformNotSupportedException("32-bit systems are no longer supported. See https://officialaptivi.wordpress.com/2024/08/03/final-word-regarding-32-bit-support/ for more info."),
                    };
                    break;
                case Platform.Linux:
                case Platform.MacOS:
                    switch (bitness)
                    {
                        case Architecture.X64:
                            libManager = new LibraryManager(
                                new LibraryFile(libPath));
                            break;
                        case Architecture.Arm:
                        case Architecture.X86:
                            throw new PlatformNotSupportedException("32-bit systems are no longer supported. See https://officialaptivi.wordpress.com/2024/08/03/final-word-regarding-32-bit-support/ for more info.");
                    }
                    break;
            }

            // Load the native library
            libManager.LoadNativeLibrary();
            _initialized = true;
        }

        private static string GetLibraryPath(string libraryName)
        {
            string execPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/";
            string nonSpecificRid =
                    (PlatformHelper.IsOnWindows() ? "win-" :
                     PlatformHelper.IsOnMacOS() ? "osx-" :
                     PlatformHelper.IsOnUnix() ? "linux-" :
                     "freebsd-") + RuntimeInformation.OSArchitecture.ToString().ToLower();
            string directory = $"runtimes/{nonSpecificRid}/native/";
            string libName = $"{libraryName}{(PlatformHelper.IsOnWindows() ? ".dll" : PlatformHelper.IsOnMacOS() ? ".dylib" : ".so")}";
            string path = $"{execPath}{directory}{libName}";
            return path;
        }
    }
}
