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

using SpecProbe.Software.Languages;
using SpecProbe.Software.Platform;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SpecProbe.Software.Kernel
{
    /// <summary>
    /// Kernel information probing manager
    /// </summary>
    public static class UnameManager
    {
        /// <summary>
        /// Gets details from the uname process
        /// </summary>
        /// <param name="unameTypes">All the uname types the high-level APIs passed</param>
        /// <returns>Uname string output</returns>
        public static string GetUname(UnameTypes unameTypes)
        {
            // Check the platform
            if (!PlatformHelper.IsOnUnix())
                throw new PlatformNotSupportedException(LanguageTools.GetLocalized("SPECPROBE_SOFTWARE_KERNEL_EXCEPTION_NEEDSUNIX"));

            // Check the uname executable paths
            string UnameExecutable = File.Exists("/usr/bin/uname") ? "/usr/bin/uname" : "/bin/uname";
            UnameExecutable = File.Exists("/system/xbin/uname") ? "/system/xbin/uname" : UnameExecutable;

            // Select arguments according to the types
            StringBuilder argsBuilder = new();
            if (unameTypes.HasFlag(UnameTypes.KernelName))
                argsBuilder.Append("-s ");
            if (unameTypes.HasFlag(UnameTypes.KernelRelease))
                argsBuilder.Append("-r ");
            if (unameTypes.HasFlag(UnameTypes.KernelVersion))
                argsBuilder.Append("-v ");
            if (unameTypes.HasFlag(UnameTypes.NetworkNode))
                argsBuilder.Append("-n ");
            if (unameTypes.HasFlag(UnameTypes.Machine))
                argsBuilder.Append("-m ");
            if (unameTypes.HasFlag(UnameTypes.OperatingSystem))
                argsBuilder.Append("-o ");
            string Args = argsBuilder.ToString().Trim();

            // Make a new instance of process
            Process UnameS = new();
            ProcessStartInfo UnameSInfo = new()
            {
                FileName = UnameExecutable,
                Arguments = Args,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true
            };
            UnameS.StartInfo = UnameSInfo;

            // Start the process and wait for the output to flow
            UnameS.Start();
            UnameS.WaitForExit();

            // Return the output
            return UnameS.StandardOutput.ReadToEnd().Trim(['\n']);
        }
    }
}
