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

namespace SpecProbe.Loader.Interop.Environment
{
    internal static partial class EnvironmentManager
    {
        [DllImport("UCRTBASE.DLL", EntryPoint = "getenv_s", SetLastError = true)]
        internal static extern int Win_getenv_s(ref int requiredSize, IntPtr buffer, int bufferSize, IntPtr varname);

        [DllImport("UCRTBASE.DLL", EntryPoint = "_putenv_s", SetLastError = true)]
        internal static extern int Win_putenv_s(string e, string v);
    }
}
