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

namespace SpecProbe.Platform
{
    internal static unsafe class PlatformMacInterop
    {
        #region System Framework Paths
        const string cgFrameworkPath = "/System/Library/Frameworks/ApplicationServices.framework/Frameworks/CoreGraphics.framework/CoreGraphics";
        #endregion

        #region Video adapter macOS API pinvokes
        /// <summary>
        /// CGError CGGetOnlineDisplayList(uint32_t maxDisplays, CGDirectDisplayID *onlineDisplays, uint32_t *displayCount);
        /// </summary>
        [DllImport(cgFrameworkPath)]
        public static extern CGError CGGetOnlineDisplayList(uint maxDisplays, uint[] onlineDisplays, out uint displayCount);

        /// <summary>
        /// CGError CGGetOnlineDisplayList(uint32_t maxDisplays, CGDirectDisplayID *onlineDisplays, uint32_t *displayCount);
        /// </summary>
        [DllImport(cgFrameworkPath)]
        public static extern CGError CGGetOnlineDisplayList(uint maxDisplays, ref uint[] onlineDisplays, out uint displayCount);

        /// <summary>
        /// uint32_t CGDisplayModelNumber(CGDirectDisplayID display);
        /// </summary>
        [DllImport(cgFrameworkPath)]
        public static extern uint CGDisplayModelNumber(uint display);

        /// <summary>
        /// uint32_t CGDisplayVendorNumber(CGDirectDisplayID display);
        /// </summary>
        [DllImport(cgFrameworkPath)]
        public static extern uint CGDisplayVendorNumber(uint display);
        #endregion

        #region Common
        internal enum CGError
        {
            /// <summary>
            /// The requested operation is inappropriate for the parameters passed in, or the current system state.
            /// </summary>
            kCGErrorCannotComplete = 1004,
            /// <summary>
            /// A general failure occurred.
            /// </summary>
            kCGErrorFailure = 1000,
            /// <summary>
            /// One or more of the parameters passed to a function is invalid. Check for <see cref="IntPtr.Zero"/> pointers.
            /// </summary>
            kCGErrorIllegalArgument = 1001,
            /// <summary>
            /// The parameter representing a connection to the window server is invalid.
            /// </summary>
            kCGErrorInvalidConnection = 1002,
            /// <summary>
            /// The <c>CPSProcessSerNum</c> or context identifier parameter is not valid.
            /// </summary>
            kCGErrorInvalidContext = 1003,
            /// <summary>
            /// The requested operation is not valid for the parameters passed in, or the current system state.
            /// </summary>
            kCGErrorInvalidOperation = 1010,
            /// <summary>
            /// The requested operation could not be completed as the indicated resources were not found.
            /// </summary>
            kCGErrorNoneAvailable = 1011,
            /// <summary>
            /// A parameter passed in has a value that is inappropriate, or which does not map to a useful operation or value.
            /// </summary>
            kCGErrorRangeCheck = 1007,
            /// <summary>
            /// The requested operation was completed successfully.
            /// </summary>
            kCGErrorSuccess = 0,
            /// <summary>
            /// A data type or token was encountered that did not match the expected type or token.
            /// </summary>
            kCGTypeCheck = 1008,
        }
        #endregion
    }
}
