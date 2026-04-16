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
using System.Runtime.InteropServices;

namespace SpecProbe.Loader.Interop.LoadLibrary
{
    internal static partial class LibraryLoader
    {
        internal const int RTLD_LAZY = 1;
        internal const int RTLD_GLOBAL = 8;
        internal const int LAZY_GLOBAL = RTLD_LAZY | RTLD_GLOBAL;

        // Older distros
        [DllImport("libdl.so", EntryPoint = "dlopen", SetLastError = true)]
        internal static extern IntPtr Linux_dlopen(string filename, int flags);

        [DllImport("libdl.so", EntryPoint = "dlsym", SetLastError = true)]
        internal static extern IntPtr Linux_dlsym(IntPtr handle, string symbol);

        // Modern distros
        [DllImport("libdl.so.2", EntryPoint = "dlopen", SetLastError = true)]
        internal static extern IntPtr Linux_dlopen_new(string filename, int flags);

        [DllImport("libdl.so.2", EntryPoint = "dlsym", SetLastError = true)]
        internal static extern IntPtr Linux_dlsym_new(IntPtr handle, string symbol);

        // Mono
        [DllImport("__Internal", EntryPoint = "dlopen", SetLastError = true)]
        internal static extern IntPtr Mono_dlopen(string filename, int flags);

        [DllImport("__Internal", EntryPoint = "dlsym", SetLastError = true)]
        internal static extern IntPtr Mono_dlsym(IntPtr handle, string symbol);
    }
}
