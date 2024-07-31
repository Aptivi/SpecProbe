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

#pragma comment(lib, "dxgi.lib")

/* -------------------------------------------------------------------- */

#include <windows.h>
#include <dxgi.h>
#include <vector>
#include <sstream>

/* -------------------------------------------------------------------- */

extern "C" struct
    spdx_gpu_info
    /*
    * -----------------------------------------------------------------------
    * Name        : spdx_gpu_info
    * Description : A struct that stores minimal info about a GPU
    * -----------------------------------------------------------------------
    * Exposure    : Exposed to the SpecProbe managed world
    * -----------------------------------------------------------------------
    */
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
        spdx_gpu_info**& devices,
        UINT& length
    )
    /*
    * -----------------------------------------------------------------------
    * Name        : spdx_get_gpus
    * Description : Gets a list of graphics cards according to DirectX
    * -----------------------------------------------------------------------
    * Arguments   : An output array of devices (passed by reference)
    *               A length of the array (passed by reference)
    * Returning   : true if succeeded; false if failed.
    * -----------------------------------------------------------------------
    * Exposure    : Exposed to the SpecProbe managed world
    * -----------------------------------------------------------------------
    */
{
    // Create a list
    std::vector<spdx_gpu_info*> devicesList = {};
    
    // Create the DXGI factory
    IDXGIFactory* factory;
    if (!SUCCEEDED(CreateDXGIFactory(__uuidof(IDXGIFactory), reinterpret_cast<void**>(&factory))))
        return false;

    // Enumerate the adapters until there are no more adapters
    UINT i = 0;
    IDXGIAdapter* adapter = nullptr;
    while (factory->EnumAdapters(i++, &adapter) != DXGI_ERROR_NOT_FOUND)
    {
        // Get the needed description
        DXGI_ADAPTER_DESC desc;
        adapter->GetDesc(&desc);

        // Check for support before installing values
        LARGE_INTEGER umdVersion;
        if (adapter->CheckInterfaceSupport(__uuidof(IDXGIDevice), &umdVersion) ==
            DXGI_ERROR_UNSUPPORTED)
        {
            adapter->Release();
            continue;
        }

        // Install info
        spdx_gpu_info* device = new spdx_gpu_info();
        device->vendorId = desc.VendorId;
        device->deviceId = desc.DeviceId;
        wcscpy_s(device->name, desc.Description);

        // Add this device to the array vector
        devicesList.push_back(device);
        adapter->Release();
    }

    // Indicate success
    factory->Release();
    devices = new spdx_gpu_info * [devicesList.size()];
    std::copy(std::begin(devicesList), std::end(devicesList), devices);
    length = i - 1;
    return (i > 0);
}

/* -------------------------------------------------------------------- */
