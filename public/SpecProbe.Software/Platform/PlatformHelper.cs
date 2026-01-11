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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using SpecProbe.Software.Kernel;
using SpecProbe.Software.Languages;

namespace SpecProbe.Software.Platform
{
    /// <summary>
    /// Platform helper
    /// </summary>
    public static class PlatformHelper
    {
        private static Platform platform = Platform.Unknown;
        private static bool firstTimeMuslDetection = true;
        private static bool isMuslLinux = false;
        private static bool firstTimeAndroidDetection = true;
        private static bool isAndroid = false;
        private static bool firstTimeWslDetection = true;
        private static bool isWsl = false;

        /// <summary>
        /// Is this system a Windows system?
        /// </summary>
        /// <returns>True if running on Windows (Windows 10, Windows 11, etc.). Otherwise, false.</returns>
        public static bool IsOnWindows()
        {
            if (platform == Platform.Windows)
                return true;
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }

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
        public static bool IsOnUnix()
        {
            if (platform == Platform.Linux)
                return true;
            return Environment.OSVersion.Platform == PlatformID.Unix;
        }

        /// <summary>
        /// Is this system a macOS system?
        /// </summary>
        /// <returns>True if running on macOS (MacBook, iMac, etc.). Otherwise, false.</returns>
        public static bool IsOnMacOS()
        {
            if (platform == Platform.MacOS)
                return true;
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
            if (IsOnUnix() && !IsOnMacOS() && firstTimeAndroidDetection)
                isAndroid = File.Exists("/system/build.prop");
            firstTimeAndroidDetection = false;
            return isAndroid;
        }

        /// <summary>
        /// Is this system an ARM or ARM64 system?
        /// </summary>
        /// <returns>True if running on ARM or ARM64 systems. Otherwise, false.</returns>
        public static bool IsOnArmOrArm64() =>
            IsOnArm() ||
            IsOnArm64();

        /// <summary>
        /// Is this system an x86 or AMD64 system?
        /// </summary>
        /// <returns>True if running on x86 or AMD64 systems. Otherwise, false.</returns>
        public static bool IsOnX86OrAmd64() =>
            IsOnX86() ||
            IsOnAmd64();

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
        /// Is this system an AMD64 system?
        /// </summary>
        /// <returns>True if running on AMD64 systems. Otherwise, false.</returns>
        public static bool IsOnAmd64() =>
            RuntimeInformation.OSArchitecture == Architecture.X64;

        /// <summary>
        /// Is this system an x86 system?
        /// </summary>
        /// <returns>True if running on x86 systems. Otherwise, false.</returns>
        public static bool IsOnX86() =>
            RuntimeInformation.OSArchitecture == Architecture.X86;

        /// <summary>
        /// Is this system a Unix system that contains musl libc?
        /// </summary>
        /// <returns>True if running on Unix systems that use musl libc. Otherwise, false.</returns>
        public static bool IsOnUnixMusl()
        {
            if (!firstTimeMuslDetection)
                return isMuslLinux;
            try
            {
                if (!IsOnUnix() || IsOnMacOS() || IsOnWindows())
                    return false;
                var gnuRel = gnuGetLibcVersion();
                isMuslLinux = false;
            }
            catch
            {
                // Android uses Bionic instead of GNU and MUSL.
                isMuslLinux = !IsOnAndroid();
            }
            firstTimeMuslDetection = false;
            return isMuslLinux;
        }

        /// <summary>
        /// Is this system a Unix system that is from WSL (Windows Subsystem for Linux)?
        /// </summary>
        /// <returns>True if running on WSL. Otherwise, false.</returns>
        public static bool IsOnUnixWsl()
        {
            if (!firstTimeWslDetection)
                return isWsl;

            // Get a path that allows us to detect WSL using WSLInterop.
            string wslInteropPath = "/proc/sys/fs/binfmt_misc/WSLInterop";
            string wslInteropMagic = "4d5a";

            // Check to see if we have this file
            if (File.Exists(wslInteropPath))
            {
                string stream = File.ReadAllText(wslInteropPath);
                isWsl = stream.Contains(wslInteropMagic);
            }
            else
                isWsl = false;
            firstTimeWslDetection = false;
            return isWsl;
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
        /// Is this application running from GRILO?
        /// </summary>
        public static bool IsRunningFromGrilo() =>
            (Assembly.GetEntryAssembly()?.GetName()?.Name?.StartsWith("GRILO")) ?? false;

        /// <summary>
        /// Is this application running from Nitrocid KS bootloader?
        /// </summary>
        public static bool IsRunningFromNitrocid() =>
            (Assembly.GetEntryAssembly()?.GetName()?.Name?.StartsWith("Nitrocid")) ?? false;

        /// <summary>
        /// Is this application running from TMUX?
        /// </summary>
        public static bool IsRunningFromTmux() =>
            Environment.GetEnvironmentVariable("TMUX") is not null;

        /// <summary>
        /// Is this application running from GNU Screen?
        /// </summary>
        public static bool IsRunningFromScreen() =>
            Environment.GetEnvironmentVariable("STY") is not null;

        /// <summary>
        /// Is this application running from Mono?
        /// </summary>
        public static bool IsRunningFromMono() =>
            Type.GetType("Mono.Runtime") is not null;

        /// <summary>
        /// Gets the current runtime identifier
        /// </summary>
        /// <param name="includeMusl">Whether to detect MUSL libc</param>
        /// <returns>Returns a runtime identifier (win-x64 for example).</returns>
        public static string GetCurrentGenericRid(bool includeMusl = true) =>
            $"{(IsOnWindows() ? "win" : IsOnMacOS() ? "osx" : IsOnUnix() ? "linux" : "freebsd")}-" +
            $"{(includeMusl && IsOnUnixMusl() ? "musl-" : "")}" +
            $"{RuntimeInformation.OSArchitecture.ToString().ToLower()}";

        /// <summary>
        /// Gets the platform enumeration from the current platform
        /// </summary>
        /// <returns>Platform enumeration from the current platform</returns>
        /// <exception cref="PlatformNotSupportedException"></exception>
        public static Platform GetPlatform()
        {
            if (platform == Platform.Unknown)
            {
                if (IsOnWindows())
                    platform = Platform.Windows;
                else if (IsOnMacOS())
                    platform = Platform.MacOS;
                else if (IsOnUnix())
                    platform = Platform.Linux;
                else
                    throw new PlatformNotSupportedException(LanguageTools.GetLocalized("SPECPROBE_SOFTWARE_PLATFORM_EXCEPTION_OSUNSUPPORTED"));
            }
            return platform;
        }

        /// <summary>
        /// Gets the architecture
        /// </summary>
        /// <returns>The current architecture</returns>
        public static Architecture GetArchitecture()
        {
            var bitness = RuntimeInformation.OSArchitecture;
            bitness = bitness == Architecture.X64 ? RuntimeInformation.ProcessArchitecture : bitness;
            return bitness;
        }

        /// <summary>
        /// Checks to see if the program is running under .NET Framework
        /// </summary>
        /// <returns>True if running under .NET Framework; False if otherwise.</returns>
        public static bool IsDotNetFx() =>
            RuntimeInformation.FrameworkDescription.Contains("Framework");

        /// <summary>
        /// Is this application running on X11?
        /// </summary>
        public static bool IsOnX11() =>
            IsOnUnix() && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY"));

        /// <summary>
        /// Is this application running on Wayland?
        /// </summary>
        public static bool IsOnWayland() =>
            IsOnUnix() && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));

        /// <summary>
        /// Is this application running in the GUI?
        /// </summary>
        public static bool IsOnGui() =>
            !IsOnUnix() || IsOnX11() || IsOnWayland();

        /// <summary>
        /// Gets a list of PATH directories
        /// </summary>
        /// <returns>A list of directories specified by the PATH environment variable</returns>
        public static string[] GetPaths()
        {
            char separator = IsOnWindows() ? ';' : ':';
            return (Environment.GetEnvironmentVariable("PATH") ?? "").Split([separator], StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Gets the possible paths that this file is found on
        /// </summary>
        /// <param name="file">Target file name with extension</param>
        /// <returns>Possible list of paths</returns>
        public static string[] GetPossiblePaths(string file) =>
            GetPossiblePaths(file, GetPaths());

        /// <summary>
        /// Gets the possible paths that this file is found on
        /// </summary>
        /// <param name="file">Target file name with extension</param>
        /// <param name="lookupPaths">Lookup paths with a directory path for each item</param>
        /// <returns>Possible list of paths</returns>
        public static string[] GetPossiblePaths(string file, string[] lookupPaths)
        {
            if (Path.IsPathRooted(file))
                file = Path.GetFileName(file);
            List<string> finalPaths = [];
            foreach (string path in lookupPaths)
            {
                string finalPath = Path.Combine(path, file);
                if (File.Exists(finalPath))
                    finalPaths.Add(finalPath);
            }
            return [.. finalPaths];
        }

        /// <summary>
        /// Tries to parse the file name
        /// </summary>
        /// <param name="name">File name to parse</param>
        /// <returns>True if valid; else false.</returns>
        public static bool TryParseFileName(string name)
        {
            try
            {
                return !(name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to parse file name {0}: {1}", name, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Tries to parse the path
        /// </summary>
        /// <param name="path">Path to parse</param>
        /// <returns>True if valid; else false.</returns>
        public static bool TryParsePath(string path)
        {
            try
            {
                return !(path.IndexOfAny(GetInvalidPathChars()) >= 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to parse path {0}: {1}", path, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Gets the invalid path chars, taking some of Windows' invalid path characters into account
        /// </summary>
        /// <returns>A list of invalid path characters</returns>
        public static char[] GetInvalidPathChars()
        {
            var FinalInvalidPathChars = Path.GetInvalidPathChars();
            var WindowsInvalidPathChars = new[] { '"', '<', '>' };
            if (IsOnWindows())
            {
                // It's weird of .NET 6.0 to not consider the above three Windows invalid directory chars to be invalid,
                // so make them invalid as in .NET Framework.
                Array.Resize(ref FinalInvalidPathChars, 36);
                WindowsInvalidPathChars.CopyTo(FinalInvalidPathChars, FinalInvalidPathChars.Length - 3);
            }
            return FinalInvalidPathChars;
        }

        /// <summary>
        /// Opens a URL or a file using xdg-open in Linux or its equivalent in other operating systems
        /// </summary>
        /// <param name="path">Path to open using the OS</param>
        public static void PlatformOpen(string path)
        {
            if (IsOnWindows())
                ExecuteProcessToString("cmd.exe", $"/c \"start {path}\"");
            else if (IsOnMacOS())
                ExecuteProcessToString("open", path);
            else
                ExecuteProcessToString("xdg-open", path);
        }

        /// <summary>
        /// Executes a file with specified arguments and puts the output to the string
        /// </summary>
        /// <param name="file">Full path to file</param>
        /// <param name="args">Arguments, if any</param>
        /// <returns>Output of a command from stdout</returns>
        public static string ExecuteProcessToString(string file, string args)
        {
            var CommandProcess = new Process();
            var CommandProcessStart = new ProcessStartInfo()
            {
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = file,
                Arguments = args,
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

        static PlatformHelper()
        {
            GetPlatform();
        }

        #region Interop
        [DllImport("libc", EntryPoint = "gnu_get_libc_version")]
        private static extern IntPtr gnuGetLibcVersion();
        #endregion
    }
}
