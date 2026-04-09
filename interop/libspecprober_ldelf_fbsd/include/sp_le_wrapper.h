
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

void*
sp_le_dlopen
(
    const char* path,
    int flags
);

void*
sp_le_dlsym
(
    void* handle,
    const char* symbol
);

const char*
sp_le_dlerror
(
    void
);

/* -------------------------------------------------------------------- */

