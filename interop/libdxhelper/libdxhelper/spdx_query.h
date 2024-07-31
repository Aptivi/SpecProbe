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

/* -------------------------------------------------------------------- */

#pragma once
#include <vector>
#include <wtypes.h>

/* -------------------------------------------------------------------- */

struct
    spdx_gpu_info
{
public:
    UINT vendorId = 0x0;
    UINT deviceId = 0x0;
    WCHAR name[128] = {};
};

/* -------------------------------------------------------------------- */

extern "C" __declspec(dllexport) BOOL
    spdx_get_gpus
    (
        spdx_gpu_info*& devices,
        UINT& length
    );

/* -------------------------------------------------------------------- */
