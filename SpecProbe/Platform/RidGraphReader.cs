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

using SpecProbe.Properties;
using SpecProbe.Software.Platform;
using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SpecProbe.Platform
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
#if NET
            GetGraphFromRid(RuntimeInformation.RuntimeIdentifier);
#else
            GetGraphFromRid(PlatformHelper.GetCurrentGenericRid());
#endif

        /// <summary>
        /// Gets the graph from the specified RID
        /// </summary>
        /// <returns>List of RIDs from the specified RID to the base RID</returns>
        public static string[] GetGraphFromRid(string rid)
        {
            string graphJson = Resources.RidGraph;
            var graphInstance = JsonNode.Parse(graphJson);
            foreach (var ridGraph in graphInstance.AsObject())
            {
                if (ridGraph.Key == rid)
                {
                    var graph = ridGraph.Value;
                    var graphArray = graph.Deserialize<string[]>();
                    return graphArray;
                }
            }
            return Array.Empty<string>();
        }
    }
}
