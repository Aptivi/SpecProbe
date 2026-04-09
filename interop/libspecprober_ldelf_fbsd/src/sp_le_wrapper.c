
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

#include <dlfcn.h>
#include <stdlib.h>

/* -------------------------------------------------------------------- */

void*
sp_le_dlopen
(
    const char* path,
    int flags
)
/*
 * -----------------------------------------------------------------------
 * Name        : sp_le_dlopen
 * Description : Opens a library handle (FreeBSD)
 * -----------------------------------------------------------------------
 * Arguments   : path:  Path to library
 *               flags: Library load flags
 * Returning   : A pointer to the loaded library, or NULL if errored
 * -----------------------------------------------------------------------
 * Exposure    : Exposed to the SpecProbe managed world
 * -----------------------------------------------------------------------
 */
{
    return dlopen(path, flags);
}

void*
sp_le_dlsym
(
    void* handle,
    const char* symbol
)
/*
 * -----------------------------------------------------------------------
 * Name        : sp_le_dlsym
 * Description : Gets a symbol from the loaded library handle (FreeBSD)
 * -----------------------------------------------------------------------
 * Arguments   : handle: Pointer to library
 *               symbol: Symbol name to load
 * Returning   : A pointer to the symbol, or NULL if errored
 * -----------------------------------------------------------------------
 * Exposure    : Exposed to the SpecProbe managed world
 * -----------------------------------------------------------------------
 */
{
    return dlsym(handle, symbol);
}

const char*
sp_le_dlerror
(
    void
)
/*
 * -----------------------------------------------------------------------
 * Name        : sp_le_dlerror
 * Description : Gets an error string (FreeBSD)
 * -----------------------------------------------------------------------
 * Arguments   : Nothing
 * Returning   : NULL if no error, string if there is an error
 * -----------------------------------------------------------------------
 * Exposure    : Exposed to the SpecProbe managed world
 * -----------------------------------------------------------------------
 */
{
    return dlerror();
}
