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

#include "../spdx_query.h"

/* -------------------------------------------------------------------- */

int main()
{
    printf("spdx_query test...\n");
    spdx_gpu_info* gpus = nullptr;
    UINT length = 0;
    BOOL result = spdx_get_gpus(gpus, length);
    if (!result || length == 0)
    {
        printf("none.\n");
        return 1;
    }
    for (int i = 0; i < length; i++)
    {
        spdx_gpu_info info = gpus[i];
        printf("info->vendorId: %i\n", info.vendorId);
        printf("info->deviceId: %i\n", info.deviceId);
        printf("info->name: %ws\n", info.name);
    }
}

/* -------------------------------------------------------------------- */
