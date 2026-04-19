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
        [DllImport("libSystem.dylib", EntryPoint = "dlopen", SetLastError = true)]
        internal static extern IntPtr Mac_dlopen(string filename, int flags);

        [DllImport("libSystem.dylib", EntryPoint = "dlsym", SetLastError = true)]
        internal static extern IntPtr Mac_dlsym(IntPtr handle, string symbol);

        [DllImport("libSystem.dylib", EntryPoint = "dlerror", SetLastError = true)]
        internal static extern IntPtr Mac_dlerror();
    }
}
