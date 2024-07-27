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

using SpecProbe.Software.Kernel;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using SpecProbe.Platform;



#if !NETCOREAPP
using NativeLand;
#endif

namespace SpecProbe.Native
{
    internal static class Initializer
    {
        internal static LibraryManager libManager;
        private static bool _initialized;
        private const string LibraryName = "libspecprober";

        internal static void InitializeNative()
        {
            if (_initialized)
                return;
            string libPath = GetLibraryPath();
            if (!File.Exists(libPath))
                throw new Exception("Can't load specprober library because it isn't found.");
            libManager = new LibraryManager(
                new LibraryItem(NativeLand.Platform.Windows, Architecture.X86,
                    new LibraryFile(libPath)),
                new LibraryItem(NativeLand.Platform.Windows, Architecture.X64,
                    new LibraryFile(libPath)),
                new LibraryItem(NativeLand.Platform.MacOS, Architecture.X64,
                    new LibraryFile(libPath)),
                new LibraryItem(NativeLand.Platform.Linux, Architecture.X64,
                    new LibraryFile(libPath)),
                new LibraryItem(NativeLand.Platform.Linux, Architecture.X86,
                    new LibraryFile(libPath)));
            libManager.LoadNativeLibrary();
            _initialized = true;
        }

        private static string GetLibraryPath()
        {
            string execPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/";
            string nonSpecificRid =
                    (PlatformHelper.IsOnWindows() ? "win-" :
                     PlatformHelper.IsOnMacOS() ? "osx-" :
                     PlatformHelper.IsOnUnix() ? "linux-" :
                     "freebsd-") + RuntimeInformation.OSArchitecture.ToString().ToLower();
            string directory = $"runtimes/{nonSpecificRid}/native/";
            string libName = $"{LibraryName}{(PlatformHelper.IsOnWindows() ? ".dll" : PlatformHelper.IsOnMacOS() ? ".dylib" : ".so")}";
            string path = $"{execPath}{directory}{libName}";
            return path;
        }
    }
}
