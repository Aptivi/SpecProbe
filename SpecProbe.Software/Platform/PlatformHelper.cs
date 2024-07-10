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

using SpecProbe.Software.Kernel;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SpecProbe.Platform
{
    /// <summary>
    /// Platform helper
    /// </summary>
    public static class PlatformHelper
    {
        /// <summary>
        /// Is this system a Windows system?
        /// </summary>
        /// <returns>True if running on Windows (Windows 10, Windows 11, etc.). Otherwise, false.</returns>
        public static bool IsOnWindows() =>
            Environment.OSVersion.Platform == PlatformID.Win32NT;

        /// <summary>
        /// Is this system a Windows system or a WSL system?
        /// </summary>
        /// <returns>True if running on Windows (Windows 10, Windows 11, etc.) or running on WSL. Otherwise, false.</returns>
        public static bool IsOnWindowsOrWsl() =>
            IsOnWindows() || IsOnUnixWsl();

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
        /// Is this system an Android system?
        /// </summary>
        /// <returns>True if running on Android phones using Termux. Otherwise, false.</returns>
        public static bool IsOnAndroid()
        {
            if (IsOnUnix() && !IsOnMacOS())
                return File.Exists("/system/build.prop");
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
        /// Is this system a Unix system that contains musl libc?
        /// </summary>
        /// <returns>True if running on Unix systems that use musl libc. Otherwise, false.</returns>
        public static bool IsOnUnixMusl()
        {
            try
            {
                if (!IsOnUnix() || IsOnMacOS() || IsOnWindows())
                    return false;
                var gnuRel = gnuGetLibcVersion();
                return false;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Is this system a Unix system that is from WSL (Windows Subsystem for Linux)?
        /// </summary>
        /// <returns>True if running on WSL. Otherwise, false.</returns>
        public static bool IsOnUnixWsl()
        {
            // Now, get a path that allows us to detect WSL using WSLInterop.
            string wslInteropPath = "/proc/sys/fs/binfmt_misc/WSLInterop";
            string wslInteropMagic = "4d5a";

            // Check to see if we have this file
            if (!File.Exists(wslInteropPath))
                return false;
            string stream = File.ReadAllText(wslInteropPath);
            return stream.Contains(wslInteropMagic);
        }

        /// <summary>
        /// Polls $TERM_PROGRAM to get terminal emulator
        /// </summary>
        public static string GetTerminalEmulator() =>
            Environment.GetEnvironmentVariable("TERM_PROGRAM") ?? "";

        /// <summary>
        /// Polls $TERM to get terminal type (vt100, dumb, ...)
        /// </summary>
        public static string GetTerminalType() =>
            Environment.GetEnvironmentVariable("TERM") ?? "";

        /// <summary>
        /// Is Terminaux running from GRILO?
        /// </summary>
        public static bool IsRunningFromGrilo() =>
            (Assembly.GetEntryAssembly()?.GetName()?.Name?.StartsWith("GRILO")) ?? false;

        /// <summary>
        /// Is Terminaux running from TMUX?
        /// </summary>
        public static bool IsRunningFromTmux() =>
            Environment.GetEnvironmentVariable("TMUX") is not null;

        /// <summary>
        /// Is Terminaux running from GNU Screen?
        /// </summary>
        public static bool IsRunningFromScreen() =>
            Environment.GetEnvironmentVariable("STY") is not null;

        /// <summary>
        /// Is Terminaux running from Mono?
        /// </summary>
        public static bool IsRunningFromMono() =>
            Type.GetType("Mono.Runtime") is not null;

        /// <summary>
        /// Gets the current runtime identifier
        /// </summary>
        /// <returns>Returns a runtime identifier (win-x64 for example).</returns>
        public static string GetCurrentGenericRid() =>
            $"{(IsOnWindows() ? "win" : IsOnMacOS() ? "osx" : IsOnUnix() ? "linux" : "freebsd")}-" +
            $"{(IsOnUnixMusl() ? "musl-" : "")}" +
            $"{RuntimeInformation.OSArchitecture.ToString().ToLower()}";

        /// <summary>
        /// Executes a file with specified arguments and puts the output to the string
        /// </summary>
        /// <param name="File">Full path to file</param>
        /// <param name="Args">Arguments, if any</param>
        /// <returns>Output of a command from stdout</returns>
        internal static string ExecuteProcessToString(string File, string Args)
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

        #region Interop
        [DllImport("libc", EntryPoint = "gnu_get_libc_version")]
        private static extern IntPtr gnuGetLibcVersion();
        #endregion
    }
}
