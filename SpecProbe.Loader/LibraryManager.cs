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
using System.Collections.Generic;

namespace SpecProbe.Loader
{
    /// <summary>
    /// Library managment class
    /// </summary>
    public class LibraryManager
    {
        private bool _libLoaded = false;
        private readonly object _resourceLocker = new();
        private readonly LibraryFile[] _files;

        /// <summary>
        /// Loads native libraries.
        /// </summary>
        public void LoadNativeLibrary()
        {
            lock (_resourceLocker)
            {
                // We need not to do anything if the library is loaded
                if (_libLoaded)
                    return;

                // Load the native libraries
                foreach (var libraryFile in _files)
                    libraryFile.LoadItem();

                // Set internal flag to let applications know
                _libLoaded = true;
            }
        }

        /// <summary>
        /// Gets the native method delegate
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="methodName">Native method name</param>
        /// <returns></returns>
        public T? GetNativeMethodDelegate<T>(string methodName)
            where T : class
        {
            T? nativeDelegate = null;
            foreach (var libraryFile in _files)
            {
                if (libraryFile.NativeMethodExists(methodName, out IntPtr ptr))
                    nativeDelegate = libraryFile.GetNativeMethodDelegate<T>(ptr);
            }
            return nativeDelegate;
        }

        /// <summary>
        /// Creates a new library manager.
        /// </summary>
        /// <param name="files">Library binaries for different platforms.</param>
        public LibraryManager(params LibraryFile[] files)
        {
            // Check the items
            files ??= [];
            List<string> processed = [];
            foreach (var item in files)
            {
                if (processed.Contains(item.FilePath))
                    throw new Exception($"Duplicate library files found. [{item.FilePath}]");
                processed.Add(item.FilePath);
            }
            _files = files;
        }
    }
}
