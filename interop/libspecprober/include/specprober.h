
// SpecProbe  Copyright (C) 2023  Aptivi
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

/* -------------------------------------------------------------------- */

#include <stdint.h>

/* -------------------------------------------------------------------- */

uint32_t
    specprobe_get_max
    (
    );

uint32_t
    specprobe_get_max_ext
    (
    );

uint32_t*
specprobe_get_values
(
    uint32_t eax,
    uint32_t ecx
);

char*
    specprobe_get_vendor
    (
    );

char*
specprobe_get_cpu_name
    (
    );

/* -------------------------------------------------------------------- */

