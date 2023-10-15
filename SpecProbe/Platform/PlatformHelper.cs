
// SpecProbe  Copyright (C) 2020-2021  Aptivi
// 
// This file is part of SpecProbe
// 
// SpecProbe is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// SpecProbe is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using SpecProbe.Kernel;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpecProbe.Platform
{
    internal static class PlatformHelper
    {
        /// <summary>
        /// Is this system a Windows system?
        /// </summary>
        /// <returns>True if running on Windows (Windows 10, Windows 11, etc.). Otherwise, false.</returns>
        public static bool IsOnWindows() =>
            Environment.OSVersion.Platform == PlatformID.Win32NT;

        /// <summary>
        /// Is this system a Unix system? True for macOS, too!
        /// </summary>
        /// <returns>True if running on Unix (Linux, *nix, etc.). Otherwise, false.</returns>
        public static bool IsOnUnix() =>
            Environment.OSVersion.Platform == PlatformID.Unix;

        /// <summary>
        /// Is this system a macOS system?
        /// </summary>
        /// <returns>True if running on macOS (MacBook, iMac, etc.). Otherwise, false.</returns>
        public static bool IsOnMacOS()
        {
            if (IsOnUnix())
            {
                string System = UnameManager.GetUname(UnameTypes.KernelName);
                return System.Contains("Darwin");
            }
            else
                return false;
        }

        /// <summary>
        /// Is this system an ARM or ARM64 system?
        /// </summary>
        /// <returns>True if running on ARM or ARM64 systems. Otherwise, false.</returns>
        public static bool IsOnArmOrArm64() =>
            IsOnArm() ||
            IsOnArm64();

        /// <summary>
        /// Is this system an ARM system?
        /// </summary>
        /// <returns>True if running on ARM systems. Otherwise, false.</returns>
        public static bool IsOnArm() =>
            RuntimeInformation.OSArchitecture == Architecture.Arm;

        /// <summary>
        /// Is this system an ARM64 system?
        /// </summary>
        /// <returns>True if running on ARM64 systems. Otherwise, false.</returns>
        public static bool IsOnArm64() =>
            RuntimeInformation.OSArchitecture == Architecture.Arm64;

        /// <summary>
        /// Executes a file with specified arguments and puts the output to the string
        /// </summary>
        /// <param name="File">Full path to file</param>
        /// <param name="Args">Arguments, if any</param>
        /// <returns>Output of a command from stdout</returns>
        public static string ExecuteProcessToString(string File, string Args)
        {
            var CommandProcess = new Process();
            var CommandProcessStart = new ProcessStartInfo()
            {
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = File,
                Arguments = Args,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
            };
            CommandProcess.StartInfo = CommandProcessStart;

            // Start the process
            CommandProcess.Start();
            CommandProcess.WaitForExit();
            return CommandProcess.StandardOutput.ReadToEnd();
        }
    }
}
