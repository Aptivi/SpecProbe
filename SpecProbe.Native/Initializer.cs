
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

using SpecProbe.Native.Kernel;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SpecProbe.Native
{
    internal static class Initializer
    {
        private static bool _initialized;
        private const string LibraryName = "libspecprober";

        internal static void InitializeNative()
        {
            if (_initialized)
                return;
            string libPath = GetLibraryPath();
            if (!File.Exists(libPath))
                throw new Exception("Can't load specprober library.");
            NativeLibrary.SetDllImportResolver(typeof(Initializer).Assembly, ResolveLibrary);
            _initialized = true;
        }

        private static string GetLibraryPath()
        {
            string execPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/";
            string nonSpecificRid =
                    (IsOnWindows() ? "win-" :
                     IsOnMacOS() ? "osx-" :
                     IsOnUnix() ? "linux-" :
                     "freebsd-") + RuntimeInformation.OSArchitecture.ToString().ToLower();
            string directory = $"runtimes/{nonSpecificRid}/native/";
            string libName = $"{LibraryName}{(IsOnWindows() ? ".dll" : IsOnMacOS() ? ".dylib" : ".so")}";
            string path = $"{execPath}{directory}{libName}";
            return path;
        }

        private static nint ResolveLibrary(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            IntPtr libHandle = IntPtr.Zero;
            if (libraryName == LibraryName)
            {
                string path = GetLibraryPath();
                libHandle = NativeLibrary.Load(path);
            }
            return libHandle;
        }

        /// <summary>
        /// Is this system a Windows system?
        /// </summary>
        /// <returns>True if running on Windows (Windows 10, Windows 11, etc.). Otherwise, false.</returns>
        internal static bool IsOnWindows() =>
            Environment.OSVersion.Platform == PlatformID.Win32NT;

        /// <summary>
        /// Is this system a Unix system? True for macOS, too!
        /// </summary>
        /// <returns>True if running on Unix (Linux, *nix, etc.). Otherwise, false.</returns>
        internal static bool IsOnUnix() =>
            Environment.OSVersion.Platform == PlatformID.Unix;

        /// <summary>
        /// Is this system a macOS system?
        /// </summary>
        /// <returns>True if running on macOS (MacBook, iMac, etc.). Otherwise, false.</returns>
        internal static bool IsOnMacOS()
        {
            if (IsOnUnix())
            {
                string System = UnameManager.GetUname(UnameTypes.KernelName);
                return System.Contains("Darwin");
            }
            else
                return false;
        }
    }
}
