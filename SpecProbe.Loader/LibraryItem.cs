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

using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SpecProbe.Loader
{
    /// <summary>
    /// Library binaries for specified platform and bitness.
    /// </summary>
    public class LibraryItem
    {
        const int RTLD_LAZY = 1;
        const int RTLD_GLOBAL = 8;

        /// <summary>
        /// Library files.
        /// </summary>
        public LibraryFile[] Files { get; }

        /// <summary>
        /// Platform for which this binary is used.
        /// </summary>
        public Platform Platform { get; }

        /// <summary>
        /// Bitness for which this binary is used.
        /// </summary>
        public Architecture Bitness { get; }

        internal void LoadItem()
        {
            // Load all the libraries
            List<string> failedFiles = [];
            foreach (var file in Files)
            {
                if (PlatformHelper.IsOnWindows())
                    file.handle = LoadWindowsLibrary(file.FilePath);
                else if (PlatformHelper.IsOnMacOS())
                    file.handle = LoadMacOSLibrary(file.FilePath);
                else if (PlatformHelper.IsOnUnix())
                    file.handle = LoadLinuxLibrary(file.FilePath);
                else
                    throw new PlatformNotSupportedException("Unsupported platform.");
                if (file.handle == IntPtr.Zero)
                    failedFiles.Add(file.FilePath);
            }
            if (failedFiles.Count > 0)
                throw new InvalidOperationException("The following libraries failed to load:\n\n  - " + string.Join("\n  - ", failedFiles));
        }

        internal IntPtr LoadSymbol(string symbolName)
        {
            IntPtr result = IntPtr.Zero;
            bool found = false;
            foreach (var file in Files)
            {
                // Check handle
                if (file.handle == IntPtr.Zero)
                    throw new InvalidOperationException($"Library {file.FilePath} must be loaded with exported symbol {symbolName}.");

                // Try to find a symbol
                if (PlatformHelper.IsOnWindows())
                {
                    result = Windows.GetProcAddress(file.handle, symbolName);
                    if (result == IntPtr.Zero)
                    {
                        symbolName = "_" + symbolName + "@";
                        for (int stackSize = 0; stackSize < 128; stackSize += 4)
                        {
                            IntPtr candidate = Windows.GetProcAddress(file.handle, symbolName + stackSize);
                            if (candidate != IntPtr.Zero)
                            {
                                result = candidate;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                            result = IntPtr.Zero;
                    }
                    else
                        found = true;
                }
                else if (PlatformHelper.IsOnMacOS())
                {
                    result = MacOSX.dlsym(file.handle, symbolName);
                    found = result != IntPtr.Zero;
                }
                else if (PlatformHelper.IsOnUnix())
                {
                    if (PlatformHelper.IsRunningFromMono())
                        result = Mono.dlsym(file.handle, symbolName);
                    else
                    {
                        try
                        {
                            result = Linux.dlsym(file.handle, symbolName);
                        }
                        catch
                        {
                            result = Linux.dlsym_new(file.handle, symbolName);
                        }
                    }
                    found = result != IntPtr.Zero;
                }
                else
                    throw new PlatformNotSupportedException("Unsupported platform.");

                // If we found a symbol, bail
                if (found)
                    return result;
            }
            return result;
        }

        internal T GetNativeMethodDelegate<T>(string methodName)
            where T : class
        {
            var ptr = LoadSymbol(methodName);
            if (ptr == IntPtr.Zero)
                throw new MissingMethodException($"The native method \"{methodName}\" does not exist");
            return Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
        }

        private IntPtr LoadLinuxLibrary(string path)
        {
            IntPtr result;
            if (PlatformHelper.IsRunningFromMono())
                result = Mono.dlopen(path, RTLD_LAZY | RTLD_GLOBAL);
            else
            {
                try
                {
                    result = Linux.dlopen(path, RTLD_LAZY | RTLD_GLOBAL);
                }
                catch
                {
                    result = Linux.dlopen_new(path, RTLD_LAZY | RTLD_GLOBAL);
                }
            }
            return result;
        }

        private IntPtr LoadWindowsLibrary(string path)
        {
            var result = Windows.LoadLibrary(path);
            return result;
        }

        private IntPtr LoadMacOSLibrary(string path)
        {
            var result = MacOSX.dlopen(path, RTLD_LAZY | RTLD_GLOBAL);
            return result;
        }

        private static class Windows
        {
            [DllImport("kernel32.dll")]
            internal static extern IntPtr LoadLibrary(string filename);
            [DllImport("kernel32.dll")]
            internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        }

        private static class Linux
        {
            [DllImport("libdl.so")]
            internal static extern IntPtr dlopen(string filename, int flags);
            [DllImport("libdl.so")]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);
            [DllImport("libdl.so.2", EntryPoint = "dlopen")]
            internal static extern IntPtr dlopen_new(string filename, int flags);
            [DllImport("libdl.so.2", EntryPoint = "dlsym")]
            internal static extern IntPtr dlsym_new(IntPtr handle, string symbol);
        }

        private static class MacOSX
        {
            [DllImport("libSystem.dylib")]
            internal static extern IntPtr dlopen(string filename, int flags);
            [DllImport("libSystem.dylib")]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);
        }

        private static class Mono
        {
            [DllImport("__Internal")]
            internal static extern IntPtr dlopen(string filename, int flags);
            [DllImport("__Internal")]
            internal static extern IntPtr dlsym(IntPtr handle, string symbol);
        }

        /// <summary>
        /// Makes a new instance of this class
        /// </summary>
        /// <param name="platform">Binary platform.</param>
        /// <param name="bitness">Binary bitness.</param>
        /// <param name="files">A collection of files for this bitness and platform.</param>
        public LibraryItem(Platform platform, Architecture bitness, params LibraryFile[] files)
        {
            Platform = platform;
            Bitness = bitness;
            Files = files;
        }
    }
}
