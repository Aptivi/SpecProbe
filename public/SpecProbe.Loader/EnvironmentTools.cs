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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace SpecProbe.Loader
{
    /// <summary>
    /// Environment tools for native libraries
    /// </summary>
    public static class EnvironmentTools
    {
        /// <summary>
        /// Gets all the environment variables detected by .NET
        /// </summary>
        /// <returns>A read only dictionary that contains all parsed environment variables detected by .NET</returns>
        public static ReadOnlyDictionary<string, string> GetEnvironmentVariablesManaged()
        {
            var vars = Environment.GetEnvironmentVariables();
            Dictionary<string, string> processed = [];
            foreach (DictionaryEntry variable in vars)
                processed.Add((string)variable.Key, (string)variable.Value);
            return new(processed);
        }

        /// <summary>
        /// Gets an environment variable that is detected by .NET
        /// </summary>
        /// <param name="variable">A variable to get its value</param>
        /// <returns>A value expressed in a string</returns>
        public static string GetEnvironmentVariableManaged(string variable)
        {
            string value = Environment.GetEnvironmentVariable(variable);
            return value;
        }

        /// <summary>
        /// Gets an environment variable that is detected by UCRT
        /// </summary>
        /// <param name="variable">A variable to get its value</param>
        /// <returns>A value expressed in a string</returns>
        public static string GetEnvironmentVariableUcrt(string variable)
        {
            int size = 0;
            IntPtr varNamePtr = Marshal.StringToHGlobalAnsi(variable);
            int result = getenv_s(ref size, IntPtr.Zero, 0, varNamePtr);
            if (result != 0)
                throw new Exception(string.Format("Environment {0} can't be get.", variable) + $" [0x{Marshal.GetLastWin32Error():X8}]");

            // Check the size
            if (size == 0)
                return "";

            // Allocate the buffer with the returned size
            IntPtr buffer = Marshal.AllocHGlobal(size * sizeof(char));
            result = getenv_s(ref size, buffer, size, varNamePtr);
            if (result != 0)
                throw new Exception(string.Format("Environment {0} can't be get with buffer size {1}.", variable, size) + $" [0x{Marshal.GetLastWin32Error():X8}]");

            // Convert the value to a string
            string value = Marshal.PtrToStringAnsi(buffer);
            return value;
        }

        /// <summary>
        /// Gets an environment variable that is detected by LIBC
        /// </summary>
        /// <param name="variable">A variable to get its value</param>
        /// <returns>A value expressed in a string</returns>
        public static string GetEnvironmentVariableLibc(string variable)
        {
            var valuePtr = getenv(variable);
            if (valuePtr == IntPtr.Zero)
                return "";
            string value = Marshal.PtrToStringAnsi(valuePtr);
            return value;
        }

        /// <summary>
        /// Sets an environment variable that is detected by .NET
        /// </summary>
        /// <param name="variable">A variable to get its value</param>
        /// <param name="value">Value to set (overwrite)</param>
        public static void SetEnvironmentVariableManaged(string variable, string value) =>
            Environment.SetEnvironmentVariable(variable, value);

        /// <summary>
        /// Sets an environment variable that is detected by .NET
        /// </summary>
        /// <param name="variable">A variable to get its value</param>
        /// <param name="value">Value to set (append)</param>
        public static void SetEnvironmentVariableAppendManaged(string variable, string value)
        {
            string oldValue = GetEnvironmentVariableManaged(variable);
            SetEnvironmentVariableManaged(variable, oldValue + value);
        }

        /// <summary>
        /// Sets an environment variable that is detected by .NET (only sets if there is no variable)
        /// </summary>
        /// <param name="variable">A variable to get its value</param>
        /// <param name="value">Value to set (no overwrite)</param>
        public static void SetEnvironmentVariableNoOverwriteManaged(string variable, string value)
        {
            string oldValue = GetEnvironmentVariableManaged(variable);
            if (!string.IsNullOrEmpty(oldValue))
                SetEnvironmentVariableManaged(variable, value);
        }

        /// <summary>
        /// Sets an environment variable that is detected by UCRT
        /// </summary>
        /// <param name="variable">A variable to get its value</param>
        /// <param name="value">Value to set (overwrite)</param>
        public static void SetEnvironmentVariableUcrt(string variable, string value)
        {
            int result = _putenv_s(variable, value);
            if (result != 0)
                throw new Exception(string.Format("Environment {0} can't be set to {1}.", variable, value) + $" [0x{Marshal.GetLastWin32Error():X8}]");
        }

        /// <summary>
        /// Sets an environment variable that is detected by UCRT
        /// </summary>
        /// <param name="variable">A variable to get its value</param>
        /// <param name="value">Value to set (append)</param>
        public static void SetEnvironmentVariableAppendUcrt(string variable, string value)
        {
            string oldValue = GetEnvironmentVariableUcrt(variable);
            int result = _putenv_s(variable, oldValue + value);
            if (result != 0)
                throw new Exception(string.Format("Environment {0} can't be set to {1}.", variable, value) + $" [0x{Marshal.GetLastWin32Error():X8}]");
        }

        /// <summary>
        /// Sets an environment variable that is detected by UCRT (only sets if there is no variable)
        /// </summary>
        /// <param name="variable">A variable to get its value</param>
        /// <param name="value">Value to set (no overwrite)</param>
        public static void SetEnvironmentVariableNoOverwriteUcrt(string variable, string value)
        {
            string oldValue = GetEnvironmentVariableUcrt(variable);
            if (!string.IsNullOrEmpty(oldValue))
            {
                int result = _putenv_s(variable, value);
                if (result != 0)
                    throw new Exception(string.Format("Environment {0} can't be set to {1}.", variable, value) + $" [0x{Marshal.GetLastWin32Error():X8}]");
            }
        }

        /// <summary>
        /// Sets an environment variable that is detected by LIBC
        /// </summary>
        /// <param name="variable">A variable to get its value</param>
        /// <param name="value">Value to set (overwrite)</param>
        public static void SetEnvironmentVariableLibc(string variable, string value)
        {
            int result = setenv(variable, value, 1);
            if (result != 0)
                throw new Exception(string.Format("Environment {0} can't be set to {1}.", variable, value) + $" [0x{Marshal.GetLastWin32Error():X8}]");
        }

        /// <summary>
        /// Sets an environment variable that is detected by LIBC
        /// </summary>
        /// <param name="variable">A variable to get its value</param>
        /// <param name="value">Value to set (append)</param>
        public static void SetEnvironmentVariableAppendLibc(string variable, string value)
        {
            string oldValue = GetEnvironmentVariableLibc(variable);
            int result = setenv(variable, oldValue + value, 1);
            if (result != 0)
                throw new Exception(string.Format("Environment {0} can't be set to {1}.", variable, value) + $" [0x{Marshal.GetLastWin32Error():X8}]");
        }

        /// <summary>
        /// Sets an environment variable that is detected by LIBC (only sets if there is no variable)
        /// </summary>
        /// <param name="variable">A variable to get its value</param>
        /// <param name="value">Value to set (no overwrite)</param>
        public static void SetEnvironmentVariableNoOverwriteLibc(string variable, string value)
        {
            string oldValue = GetEnvironmentVariableLibc(variable);
            int result = setenv(variable, value, 0);
            if (result != 0)
                throw new Exception(string.Format("Environment {0} can't be set to {1}.", variable, value) + $" [0x{Marshal.GetLastWin32Error():X8}]");
        }

        #region Interop
        #region Windows
        [DllImport("UCRTBASE.DLL", SetLastError = true)]
        internal static extern int getenv_s(ref int requiredSize, IntPtr buffer, int bufferSize, IntPtr varname);

        [DllImport("UCRTBASE.DLL", SetLastError = true)]
        internal static extern int _putenv_s(string e, string v);
        #endregion

        #region Unix
        [DllImport("libc", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern IntPtr getenv(string name);

        [DllImport("libc", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern int setenv(string name, string value, int overwrite);
        #endregion
        #endregion
    }
}
