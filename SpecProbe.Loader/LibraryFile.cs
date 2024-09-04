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
using System.Runtime.InteropServices;

namespace SpecProbe.Loader
{
    /// <summary>
    /// A class to store information about a native library file or a stream.
    /// </summary>
    public class LibraryFile
    {
        const int RTLD_LAZY = 1;
        const int RTLD_GLOBAL = 8;

        internal IntPtr handle = IntPtr.Zero;

        /// <summary>
        /// Path to the native library file.
        /// </summary>
        public string FilePath { get; }

        internal void LoadItem()
        {
            // Load a library
            if (PlatformHelper.IsOnWindows())
                handle = LoadWindowsLibrary(FilePath);
            else if (PlatformHelper.IsOnMacOS())
                handle = LoadMacOSLibrary(FilePath);
            else if (PlatformHelper.IsOnUnix())
                handle = LoadLinuxLibrary(FilePath);
            else
                throw new PlatformNotSupportedException("Unsupported platform.");
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("This library or one of its dependencies failed to load: " + FilePath);
        }

        internal IntPtr LoadSymbol(string symbolName)
        {
            IntPtr result = IntPtr.Zero;
            bool found = false;

            // Check handle
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException($"Library {FilePath} must be loaded with exported symbol {symbolName}.");

            // Try to find a symbol
            if (PlatformHelper.IsOnWindows())
            {
                string resultingSymbol = symbolName;
                result = Windows.GetProcAddress(handle, resultingSymbol);
                if (result == IntPtr.Zero)
                {
                    resultingSymbol = "_" + resultingSymbol + "@";
                    for (int stackSize = 0; stackSize < 128; stackSize += 4)
                    {
                        IntPtr candidate = Windows.GetProcAddress(handle, resultingSymbol + stackSize);
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
                result = MacOSX.dlsym(handle, symbolName);
                found = result != IntPtr.Zero;
            }
            else if (PlatformHelper.IsOnUnix())
            {
                if (PlatformHelper.IsRunningFromMono())
                    result = Mono.dlsym(handle, symbolName);
                else
                {
                    try
                    {
                        result = Linux.dlsym(handle, symbolName);
                    }
                    catch
                    {
                        result = Linux.dlsym_new(handle, symbolName);
                    }
                }
                found = result != IntPtr.Zero;
            }
            else
                throw new PlatformNotSupportedException("Unsupported platform.");

            // If we found a symbol, bail
            if (found)
                return result;
            return result;
        }

        internal bool NativeMethodExists(string methodName, out IntPtr ptr)
        {
            ptr = LoadSymbol(methodName);
            return ptr != IntPtr.Zero;
        }

        internal T? GetNativeMethodDelegate<T>(IntPtr ptr)
            where T : class =>
            Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;

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
        /// Initializes the library file class instance
        /// </summary>
        /// <param name="filePath">Path to the native library file.</param>
        public LibraryFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath), "Path to the native library file is not specified");
            FilePath = filePath;
        }
    }
}
