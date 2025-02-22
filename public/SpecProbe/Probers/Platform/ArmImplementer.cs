﻿//
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

namespace SpecProbe.Probers.Platform
{
    internal class ArmImplementer
    {
        internal int id = -1;
        internal string name = "";
        internal ArmPart[] parts = [];

        internal ArmImplementer(int id, string name, ArmPart[] parts)
        {
            this.id = id;
            this.name = name;
            this.parts = parts;
        }
    }

    internal class ArmPart
    {
        internal int id = -1;
        internal string name = "";

        internal ArmPart(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}
