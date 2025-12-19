
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

#include <stdio.h>
#include <cpuid.h>
#include <string.h>
#include <stdlib.h>
#include <stdint.h>

/* -------------------------------------------------------------------- */

#define EAX_VENDOR     0x0
#define EAX_NAME_PART1 0x80000002
#define EAX_NAME_PART2 0x80000003
#define EAX_NAME_PART3 0x80000004

/* -------------------------------------------------------------------- */

uint32_t
    specprobe_get_max
    (
    )
/*
 * -----------------------------------------------------------------------
 * Name        : specprobe_get_max
 * Description : Gets the maximum CPUID index for base
 * -----------------------------------------------------------------------
 * Arguments   : Nothing
 * Returning   : An integer that returns the maximum index from base
 * -----------------------------------------------------------------------
 * Exposure    : Exposed to the SpecProbe managed world
 * -----------------------------------------------------------------------
 */
{
    return __get_cpuid_max(0x0, NULL);
}

uint32_t
    specprobe_get_max_ext
    (
    )
/*
 * -----------------------------------------------------------------------
 * Name        : specprobe_get_max_ext
 * Description : Gets the maximum CPUID index for extended
 * -----------------------------------------------------------------------
 * Arguments   : Nothing
 * Returning   : An integer that returns the maximum index from extended
 * -----------------------------------------------------------------------
 * Exposure    : Exposed to the SpecProbe managed world
 * -----------------------------------------------------------------------
 */
{
    return __get_cpuid_max(0x80000000, NULL);
}

uint32_t*
    specprobe_get_values
    (
        uint32_t eax,
        uint32_t ecx
    )
/*
 * -----------------------------------------------------------------------
 * Name        : specprobe_get_values
 * Description : Gets the values from a specified leaf
 * -----------------------------------------------------------------------
 * Arguments   : eax: EAX value
 *               ecx: ECX value
 * Returning   : An integer array of values
 * -----------------------------------------------------------------------
 * Exposure    : Exposed to the SpecProbe managed world
 * -----------------------------------------------------------------------
 */
{
    static uint32_t values[4];
    uint32_t ebx = 0, edx = 0;

    // Execute the CPUID using a specified leaf
#if defined(__i386__) && defined(__PIC__)
    __asm__("movl %%ebx, %%edi;"
            "cpuid;"
            "xchgl %%ebx, %%edi;"
            : "=D"(ebx), "+a"(eax), "+c"(ecx), "=d"(edx));
#else
    __asm__("cpuid;" : "+b"(ebx), "+a"(eax), "+c"(ecx), "=d"(edx));
#endif

    values[0] = eax;
    values[1] = ebx;
    values[2] = ecx;
    values[3] = edx;
    return values;
}

char*
    specprobe_get_vendor
    (
    )
/*
 * -----------------------------------------------------------------------
 * Name        : specprobe_get_vendor
 * Description : Gets the processor vendor string from the CPUID values
 * -----------------------------------------------------------------------
 * Arguments   : Nothing
 * Returning   : A NULL-terminated string containing vendor information
 * -----------------------------------------------------------------------
 * Exposure    : Exposed to the SpecProbe managed world
 * -----------------------------------------------------------------------
 */
{
    // Vendor (AuthenticAMD, GenuineIntel, ...)
    unsigned int eax, ebx, ecx, edx;
    __get_cpuid(EAX_VENDOR, &eax, &ebx, &ecx, &edx);

    // Create a new string containing vendor information
    char* vendor = (char*)malloc(13);
    memcpy(vendor, &ebx, 4);
    memcpy(vendor + 4, &edx, 4);
    memcpy(vendor + 8, &ecx, 4);

    // Terminate the string with NULL so that we can read it from the
    // managed world of SpecProbe
    vendor[12] = '\0';
    return vendor;
}

char*
specprobe_get_cpu_name
    (
    )
/*
 * -----------------------------------------------------------------------
 * Name        : specprobe_get_cpu_name
 * Description : Gets the processor name string from the CPUID values
 * -----------------------------------------------------------------------
 * Arguments   : Nothing
 * Returning   : A NULL-terminated string containing processor name
 * -----------------------------------------------------------------------
 * Exposure    : Exposed to the SpecProbe managed world
 * -----------------------------------------------------------------------
 */
{
    // CPU name (AMD Athlon(tm) XP 1500+, ...)
    unsigned int eax, ebx, ecx, edx;
    __get_cpuid(EAX_NAME_PART1, &eax, &ebx, &ecx, &edx);

    // Do the allocation work, part 1...
    char* name = (char*)malloc(49);
    memcpy(name     , &eax, 4);
    memcpy(name + 4 , &ebx, 4);
    memcpy(name + 8 , &ecx, 4);
    memcpy(name + 12, &edx, 4);

    // Part 2...
    __get_cpuid(EAX_NAME_PART2, &eax, &ebx, &ecx, &edx);
    memcpy(name + 16, &eax, 4);
    memcpy(name + 20, &ebx, 4);
    memcpy(name + 24, &ecx, 4);
    memcpy(name + 28, &edx, 4);

    // Part 3...
    __get_cpuid(EAX_NAME_PART3, &eax, &ebx, &ecx, &edx);
    memcpy(name + 32, &eax, 4);
    memcpy(name + 36, &ebx, 4);
    memcpy(name + 40, &ecx, 4);
    memcpy(name + 44, &edx, 4);

    // Terminate the string with NULL. See above comment.
    name[48] = '\0';
    return name;
}

