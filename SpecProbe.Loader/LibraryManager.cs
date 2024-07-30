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
using System.Linq;
using System.Runtime.InteropServices;
using SpecProbe.Software.Platform;

namespace SpecProbe.Loader
{
    /// <summary>
    /// Library managment class
    /// </summary>
    public class LibraryManager
    {
        private bool _libLoaded = false;
        private readonly object _resourceLocker = new();
        private readonly LibraryItem[] _items;

        /// <summary>
        /// Extract and load native library based on current platform and process bitness.
        /// Throws an exception if current platform is not supported.
        /// </summary>
        public void LoadNativeLibrary()
        {
            lock (_resourceLocker)
            {
                // We need not to do anything if the library is loaded
                if (_libLoaded)
                    return;

                // Load the native library when we find an item that is compatible with our system
                // architecture and type
                var item = FindItem();
                item.LoadItem();

                // Set internal flag to let applications know
                _libLoaded = true;
            }
        }

        /// <summary>
        /// Finds a library item based on current platform and bitness.
        /// </summary>
        /// <returns>Library item based on platform and bitness.</returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public LibraryItem FindItem()
        {
            // Get the platform and the bitness
            var platform = GetPlatform();
            var bitness = RuntimeInformation.OSArchitecture;
            bitness = bitness == Architecture.X64 ? RuntimeInformation.ProcessArchitecture : bitness;

            // Now, try to get a library item
            var item = _items.SingleOrDefault(x => x.Platform == platform && x.Bitness == bitness) ??
                throw new PlatformNotSupportedException($"There is no supported native library for platform '{platform}' and bitness '{bitness}'");
            return item;
        }

        /// <summary>
        /// Gets the native method delegate
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="methodName">Native method name</param>
        /// <returns></returns>
        public T GetNativeMethodDelegate<T>(string methodName)
            where T : class
        {
            var item = FindItem();
            var @delegate = item.GetNativeMethodDelegate<T>(methodName);
            return @delegate;
        }

        private static Platform GetPlatform()
        {
            if (PlatformHelper.IsOnWindows())
                return Platform.Windows;
            else if (PlatformHelper.IsOnMacOS())
                return Platform.MacOS;
            else if (PlatformHelper.IsOnUnix())
                return Platform.Linux;
            else
                throw new PlatformNotSupportedException("This operating system is not supported.");
        }

        /// <summary>
        /// Creates a new library manager.
        /// </summary>
        /// <param name="items">Library binaries for different platforms.</param>
        public LibraryManager(params LibraryItem[] items)
        {
            // Check the items
            if (items is null || items.Length == 0)
                throw new ArgumentNullException(nameof(items), "Provide library items.");
            List<(Platform, Architecture)> processed = [];
            foreach (var item in items)
            {
                var platform = item.Platform;
                var architecture = item.Bitness;
                if (processed.Contains((platform, architecture)))
                    throw new Exception($"Duplicate library items found. [{nameof(platform)}: {platform} | {nameof(architecture)}: {architecture}]");
                processed.Add((platform, architecture));
            }
            _items = items;
        }
    }
}
