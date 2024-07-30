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

namespace SpecProbe.Loader
{
    /// <summary>
    /// A class to store information about a native library file or a stream.
    /// </summary>
    public class LibraryFile
    {
        internal IntPtr handle = IntPtr.Zero;

        /// <summary>
        /// Path to the native library file.
        /// </summary>
        public string FilePath { get; }

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
