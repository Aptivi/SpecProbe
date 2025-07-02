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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;

namespace SpecProbe.Software.Platform
{
    /// <summary>
    /// Tools for reading the RID graph
    /// </summary>
    public static class RidGraphReader
    {
        /// <summary>
        /// Gets the graph from the current RID
        /// </summary>
        /// <returns>List of RIDs from the current RID to the base RID</returns>
        public static string[] GetGraphFromRid() =>
            GetGraphFromRid(PlatformHelper.GetCurrentGenericRid());

        /// <summary>
        /// Gets the graph from the specified RID
        /// </summary>
        /// <returns>List of RIDs from the specified RID to the base RID</returns>
        public static string[] GetGraphFromRid(string rid)
        {
            // Sync with this source: https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.NETCore.Platforms/src/runtime.json
            string graphJson = GetRidGraphJson();
            var fullGraph = JsonNode.Parse(graphJson) ??
                throw new Exception(LanguageTools.GetLocalized("SPECPROBE_SOFTWARE_PLATFORM_EXCEPTION_RIDGRAPHFETCH"));
            var graphInstance = fullGraph["runtimes"] ??
                throw new Exception(LanguageTools.GetLocalized("SPECPROBE_SOFTWARE_PLATFORM_EXCEPTION_RIDRUNTIMELIST"));
            List<string> finalGraph = [];
            foreach (var ridGraph in graphInstance.AsObject())
            {
                if (ridGraph.Key == rid)
                {
                    finalGraph.Add(ridGraph.Key);
                    var currentGraph = ridGraph.Value ??
                        throw new Exception(string.Format(LanguageTools.GetLocalized("SPECPROBE_SOFTWARE_PLATFORM_EXCEPTION_CURRENTGRAPH"), ridGraph.Key));
                    var graphImport = (JsonArray?)currentGraph["#import"] ??
                        throw new Exception(string.Format(LanguageTools.GetLocalized("SPECPROBE_SOFTWARE_PLATFORM_EXCEPTION_CURRENTGRAPHIMPORT"), ridGraph.Key));
                    while (graphImport.Count > 0)
                    {
                        foreach (var element in graphImport)
                        {
                            if (element is not null)
                                finalGraph.Add(element.GetValue<string>());
                        }
                        currentGraph = graphInstance[finalGraph[finalGraph.Count - 1]] ??
                            throw new Exception(string.Format(LanguageTools.GetLocalized("SPECPROBE_SOFTWARE_PLATFORM_EXCEPTION_CURRENTGRAPH"), ridGraph.Key));
                        graphImport = (JsonArray?)currentGraph["#import"] ??
                            throw new Exception(string.Format(LanguageTools.GetLocalized("SPECPROBE_SOFTWARE_PLATFORM_EXCEPTION_CURRENTGRAPHIMPORT"), ridGraph.Key));
                    }
                }
            }
            return [.. finalGraph];
        }

        private static string GetRidGraphJson()
        {
            var ridGraphStream = typeof(RidGraphReader).Assembly.GetManifestResourceStream("SpecProbe.Software.Resources.ridgraph.json");
            var reader = new StreamReader(ridGraphStream);
            return reader.ReadToEnd();
        }
    }
}
