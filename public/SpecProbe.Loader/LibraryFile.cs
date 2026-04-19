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

using SpecProbe.Loader.Interop.LoadLibrary;
using SpecProbe.Loader.Languages;
using SpecProbe.Software.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace SpecProbe.Loader
{
    /// <summary>
    /// A class to store information about a native library file or a stream.
    /// </summary>
    public class LibraryFile
    {
        internal IntPtr handle = IntPtr.Zero;
        private bool usesNewLibdl = false;
        private bool dlChecked = false;

        /// <summary>
        /// Paths or file names of the native library.
        /// </summary>
        public string[] FilePaths { get; } = [];

        internal void LoadItem()
        {
            // Populate exceptions
            List<(string, Exception)> exceptions = [];

            // Load a library from all specified paths
            foreach (var path in FilePaths)
            {
                try
                {
                    string loadError = "";
                    if (PlatformHelper.IsOnWindows())
                        handle = LoadWindowsLibrary(path, out loadError);
                    else if (PlatformHelper.IsOnMacOS())
                        handle = LoadMacOSLibrary(path, out loadError);
                    else if (PlatformHelper.IsOnFreeBSD())
                        handle = LoadFreeBSDLibrary(path, out loadError);
                    else if (PlatformHelper.IsOnUnix())
                        handle = LoadLinuxLibrary(path, out loadError);
                    else
                        throw new PlatformNotSupportedException(LanguageTools.GetLocalized("SPECPROBE_LOADER_EXCEPTION_UNSUPPORTEDPLATFORM"));
                    if (handle == IntPtr.Zero)
                        throw new InvalidOperationException(LanguageTools.GetLocalized("SPECPROBE_LOADER_EXCEPTION_LIBLOADFAILED") + $" [{loadError}]");
                }
                catch (Exception ex)
                {
                    exceptions.Add((path, ex));
                }
                if (handle != IntPtr.Zero)
                    return;
            }

            // Throw an exception if nothing is loaded
            string[] renderedExceptions = exceptions.Select((tuple) => $"{tuple.Item1}: {tuple.Item2}").ToArray();
            throw new InvalidOperationException(LanguageTools.GetLocalized("SPECPROBE_LOADER_EXCEPTION_LIBSLOADFAILED") + $"\n\n{string.Join("\n", exceptions)}");
        }

        internal IntPtr LoadSymbol(string symbolName)
        {
            IntPtr result;

            // Check handle
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException(string.Format(LanguageTools.GetLocalized("SPECPROBE_LOADER_EXCEPTION_SYMBOLLIBLOADREQUIRED"), symbolName));

            // Try to find a symbol
            if (PlatformHelper.IsOnWindows())
            {
                string resultingSymbol = symbolName;
                result = LibraryLoader.Win_GetProcAddress(handle, resultingSymbol);
                if (result == IntPtr.Zero)
                {
                    resultingSymbol = "_" + resultingSymbol + "@";
                    for (int stackSize = 0; stackSize < 128; stackSize += 4)
                    {
                        IntPtr candidate = LibraryLoader.Win_GetProcAddress(handle, resultingSymbol + stackSize);
                        if (candidate != IntPtr.Zero)
                        {
                            result = candidate;
                            break;
                        }
                    }
                }
            }
            else if (PlatformHelper.IsOnMacOS())
            {
                result = LibraryLoader.Mac_dlsym(handle, symbolName);
            }
            else if (PlatformHelper.IsOnFreeBSD())
            {
                if (PlatformHelper.IsRunningFromMono())
                    result = LibraryLoader.Mono_dlsym(handle, symbolName);
                else
                    result = LoadFreeBSDLibrarySymbolDl(symbolName);
            }
            else if (PlatformHelper.IsOnUnix())
            {
                if (PlatformHelper.IsRunningFromMono())
                    result = LibraryLoader.Mono_dlsym(handle, symbolName);
                else
                    result = LoadLinuxLibrarySymbolDl(symbolName);
            }
            else
                throw new PlatformNotSupportedException(LanguageTools.GetLocalized("SPECPROBE_LOADER_EXCEPTION_UNSUPPORTEDPLATFORM"));
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

        private IntPtr LoadLinuxLibrary(string path, out string loadError)
        {
            IntPtr result;
            if (PlatformHelper.IsRunningFromMono())
            {
                result = LibraryLoader.Mono_dlopen(path, LibraryLoader.LAZY_GLOBAL);
                loadError = Marshal.PtrToStringAnsi(LibraryLoader.Mono_dlerror());
            }
            else
                result = LoadLinuxLibraryDl(path, out loadError);
            return result;
        }

        private IntPtr LoadFreeBSDLibrary(string path, out string loadError)
        {
            IntPtr result;
            if (PlatformHelper.IsRunningFromMono())
            {
                result = LibraryLoader.Mono_dlopen(path, LibraryLoader.LAZY_GLOBAL);
                loadError = Marshal.PtrToStringAnsi(LibraryLoader.Mono_dlerror());
            }
            else
                result = LoadFreeBSDLibraryDl(path, out loadError);
            return result;
        }

        private IntPtr LoadWindowsLibrary(string path, out string loadError)
        {
            var result = LibraryLoader.Win_LoadLibrary(path);
            int error = Marshal.GetLastWin32Error();
            int hresult = Marshal.GetHRForLastWin32Error();
            var exception = Marshal.GetExceptionForHR(hresult);
            loadError = $"0x{error:X8} , {hresult}: {exception.Message}";
            return result;
        }

        private IntPtr LoadMacOSLibrary(string path, out string loadError)
        {
            var result = LibraryLoader.Mac_dlopen(path, LibraryLoader.LAZY_GLOBAL);
            loadError = Marshal.PtrToStringAnsi(LibraryLoader.Mac_dlerror());
            return result;
        }

        private IntPtr LoadLinuxLibraryDl(string path, out string loadError)
        {
            IntPtr libPtr;
            if (dlChecked)
            {
                // We've already checked for libdl. Use appropriate path.
                if (usesNewLibdl)
                {
                    libPtr = LibraryLoader.Linux_dlopen_new(path, LibraryLoader.LAZY_GLOBAL);
                    loadError = Marshal.PtrToStringAnsi(LibraryLoader.Linux_dlerror_new());
                }
                else
                {
                    libPtr = LibraryLoader.Linux_dlopen(path, LibraryLoader.LAZY_GLOBAL);
                    loadError = Marshal.PtrToStringAnsi(LibraryLoader.Linux_dlerror());
                }
            }
            else
            {
                // Now, we need to check for libdl.
                try
                {
                    libPtr = LibraryLoader.Linux_dlopen(path, LibraryLoader.LAZY_GLOBAL);
                    loadError = Marshal.PtrToStringAnsi(LibraryLoader.Linux_dlerror());
                    dlChecked = true;
                }
                catch
                {
                    usesNewLibdl = true;
                    libPtr = LibraryLoader.Linux_dlopen_new(path, LibraryLoader.LAZY_GLOBAL);
                    loadError = Marshal.PtrToStringAnsi(LibraryLoader.Linux_dlerror_new());
                    dlChecked = true;
                }
            }
            return libPtr;
        }

        private IntPtr LoadFreeBSDLibraryDl(string path, out string loadError)
        {
            // Use dlopen from libc
            IntPtr libPtr = LibraryLoader.FreeBSD_dlopen(path, LibraryLoader.LAZY_GLOBAL);
            loadError = Marshal.PtrToStringAnsi(LibraryLoader.FreeBSD_dlerror());
            return libPtr;
        }

        private IntPtr LoadLinuxLibrarySymbolDl(string symbolName)
        {
            if (dlChecked)
            {
                // We've already checked for libdl. Use appropriate path.
                if (usesNewLibdl)
                    return LibraryLoader.Linux_dlsym_new(handle, symbolName);
                else
                    return LibraryLoader.Linux_dlsym(handle, symbolName);
            }
            else
            {
                // Now, we need to check for libdl.
                IntPtr libPtr;
                try
                {
                    libPtr = LibraryLoader.Linux_dlsym(handle, symbolName);
                    dlChecked = true;
                }
                catch
                {
                    usesNewLibdl = true;
                    libPtr = LibraryLoader.Linux_dlsym_new(handle, symbolName);
                    dlChecked = true;
                }
                return libPtr;
            }
        }

        private IntPtr LoadFreeBSDLibrarySymbolDl(string symbolName)
        {
            // Use dlsym from libc
            IntPtr libPtr = LibraryLoader.FreeBSD_dlsym(handle, symbolName);
            return libPtr;
        }

        /// <summary>
        /// Initializes the library file class instance
        /// </summary>
        /// <param name="filePaths">Path to the native library file.</param>
        public LibraryFile(params string[] filePaths)
        {
            if (filePaths.Length < 1)
                throw new ArgumentNullException(nameof(filePaths), LanguageTools.GetLocalized("SPECPROBE_LOADER_EXCEPTION_LIBPATHNEEDED"));
            FilePaths = filePaths;
        }
    }
}
