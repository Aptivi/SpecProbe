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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Shouldly;
using SpecProbe.Parts;

namespace SpecProbe.Tests.Hardware
{
    [TestClass]
    public class PartsMockTests
    {
        [TestMethod]
        [DataRow(HardwarePartType.HardDisk)]
        [DataRow(HardwarePartType.Memory)]
        [DataRow(HardwarePartType.Processor)]
        [DataRow(HardwarePartType.Unknown)]
        [DataRow(HardwarePartType.Video)]
        public void PartMock(HardwarePartType type)
        {
            // [Aaa] Arrange for our mock hardware part info base
            var partMocked = Substitute.For<BaseHardwarePartInfo>();

            // [aAa] Act on our mocked hardware part
            partMocked.Type.Returns(type);

            // [aaA] Assert to ensure that our mock correctly returns the desired hardware type
            partMocked.Type.ShouldBe(type);
            _ = partMocked.Received().Type;
        }
    }
}
