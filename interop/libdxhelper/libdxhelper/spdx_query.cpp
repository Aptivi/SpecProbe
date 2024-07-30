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
#include <comdef.h>

/* -------------------------------------------------------------------- */

struct
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
    UINT vendorId = 0x0;
    UINT deviceId = 0x0;
    const char* name = "";
};

/* -------------------------------------------------------------------- */

extern "C" __declspec(dllexport) bool
    spdx_get_gpus
    (
        spdx_gpu_info* devices
    )
    /*
    * -----------------------------------------------------------------------
    * Name        : spdx_get_gpus
    * Description : Gets a list of graphics cards according to DirectX
    * -----------------------------------------------------------------------
    * Arguments   : An output array of devices (passed by reference)
    * Returning   : true if succeeded; false if failed.
    * -----------------------------------------------------------------------
    * Exposure    : Exposed to the SpecProbe managed world
    * -----------------------------------------------------------------------
    */
{
    // Create a list
    std::vector<spdx_gpu_info>* devicesList = {};
    
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
        spdx_gpu_info device;
        _bstr_t b(desc.Description);
        device.vendorId = desc.VendorId;
        device.deviceId = desc.DeviceId;
        device.name = b + "\0";

        // Add this device to the array vector
        devicesList->push_back(device);
        adapter->Release();
    }

    // Indicate success
    factory->Release();
    devices = devicesList->data();
    return (i > 0);
}

/* -------------------------------------------------------------------- */
