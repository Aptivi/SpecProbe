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

using System;
using System.Collections.Generic;
using System.Text;

namespace SpecProbe.Probers.Platform
{
    internal static class ProcessorVariables
    {
        internal static readonly string[] eax1EdxFeatures =
        [
            "fpu", "vme", "de", "pse", "tsc", "msr", "pae", "mce", "cx8", "apic", "mtrr_reserved", "sep", "mtrr",
            "pge", "mca", "cmov", "pat", "pse-36", "psn", "clfsh", "nx", "ds", "acpi", "mmx", "fxsr", "sse", "sse2",
            "ss", "htt", "tm", "ia64", "pbe"
        ];

        internal static readonly string[] eax1EcxFeatures =
        [
            "sse3", "pclmulqdq", "dtes64", "monitor", "ds-cpl", "vmx", "smx", "est", "tm2", "ssse3", "cnxt-id", "sdbg", "fma",
            "cx16", "xtpr", "pdcm", "pchnl", "pcid", "dca", "sse4.1", "sse4.2", "x2apic", "movbe", "popcnt", "tsc-deadline",
            "aes-ni", "xsave", "osxsave", "avx", "f16c", "rdrnd", "hypervisor"
        ];

        internal static readonly string[] eax1EdxAmdFeatures =
        [
            "fpu", "vme", "de", "pse", "tsc", "msr", "pae", "mce", "cx8", "apic", "syscall_k6", "syscall", "mtrr",
            "pge", "mca", "cmov", "pat", "pse-36", "a1d_reserved18", "ecc", "nx", "a1d_reserved18", "mmxext", "mmx", "fxsr", "fxsr_opt", "pdpe1gb",
            "rdtscp", "a1d_reserved28", "lm", "3dnowext", "3dnow"
        ];

        internal static readonly string[] eax1EcxAmdFeatures =
        [
            "lahf_lm", "cmp_legacy", "svm", "extapic", "cr8_legacy", "abm/lzcnt", "sse4a", "misalignsse", "3dnowprefetch", "osvw",
            "ibs", "xop", "skinit", "wdt", "a1c_reserved14", "lwp", "fma4", "tce", "a1c_reserved18", "nodeid_msr", "a1c_reserved20",
            "tbm", "topoext", "perfctr_core", "perfctr_nb", "streamperfmon", "dbx", "perftsc", "pcx_l2i", "monitorx", "addr_mask_ext"
        ];

        internal static readonly string[] eax7Ecx0EbxFeatures =
        [
            "fsgsbase", "ia32_tsc_adjust_msr", "sgx", "bmi1", "hle", "avx2", "fdp-excptn-only", "smep", "bmi2", "erms", "invpcid",
            "rtm", "rdt-m/pqm", "fpucsds", "mpx", "rdt-a/pqe", "avx512-f", "avx512-dq", "rdseed", "adx", "smap", "avx512-ifma",
            "pcommit", "clflushopt", "clwb", "pt", "avx512-pf", "avx512-er", "avx512-cd", "sha", "avx512-bw", "avx512-vl"
        ];

        internal static readonly string[] eax7Ecx0EcxFeatures =
        [
            "prefetchwt1", "avx512-vbmi", "umip", "pku", "ospke", "waitpkg", "avx512-vbmi2", "cet_ss/shstk", "gfni", "vaes",
            "vpclmulqdq", "avx512-vnni", "avx512-bitalg", "tme_en", "avx512-vpopcntdq", "fzm", "la57", "mawau", "rdpid", "kl",
            "bus-lock-detect", "cldemote", "mprr", "movdiri", "movdir64b", "enqcmd", "sgx-lc", "pks"
        ];

        internal static readonly string[] eax7Ecx0EdxFeatures =
        [
            "sgx-tem", "sgx-keys", "avx512-4vnniw", "avx512-4fmaps", "fsrm", "uintr", "a7c0d_reserved6", "a7c0d_reserved7", "avx512-vp2intersect",
            "srbds-ctrl", "md-clear", "rtm-always-abort", "a7c0d_reserved12", "rtm-force-abort", "serialize", "hybrid", "tsxldtrk", "a7c0d_reserved17",
            "pconfig", "lbr", "cet-ibt", "a7c0d_reserved21", "amx-bf16", "avx512-fp16", "amx-tile", "amx-int8", "ibrs/spec_ctrl", "stibp",
            "l1d_flush", "ia32_arch_capabilities_msr", "is32_core_capabilities_msr", "ssbd"
        ];

        internal static readonly string[] eax7Ecx1EaxFeatures =
        [
            "sha512", "sm3", "sm4", "rao-int", "avx-vnni", "avx512-bf16", "lass", "cmpccxadd", "archperfmonext", "dedup", "fzrm",
            "fsrs", "rsrcs", "a7c1a_reserved13", "a7c1a_reserved14", "a7c1a_reserved15", "a7c1a_reserved16", "fred", "lkgs", "wrmsrns", "nmi_src", "amx-fp16",
            "hreset", "avx-ifma", "a7c1a_reserved24", "a7c1a_reserved25", "lam", "msrlist", "a7c1a_reserved28", "a7c1a_reserved29", "invd_disable_­post_bios_done"
        ];

        internal static readonly string[] eax7Ecx1EbxFeatures =
        [
            "pbn", "pbndkb"
        ];

        internal static readonly string[] eax7Ecx1EcxFeatures =
        [
            "a7c1c_reserved0", "a7c1c_reserved1", "legacy_reduced_isa", "a7c1c_reserved3", "sipi64"
        ];

        internal static readonly string[] eax7Ecx1EdxFeatures =
        [
            "a7c1d_reserved0", "a7c1d_reserved1", "a7c1d_reserved2", "a7c1d_reserved3", "avx-vnni-int8", "avx-ne-convert", "a7c1d_reserved6", "a7c1d_reserved7",
            "amx-complex", "a7c1d_reserved9", "avx-vnni-int16", "a7c1d_reserved11", "a7c1d_reserved12", "utmr", "prefetchi", "user_msr", "a7c1d_reserved16",
            "uiret-uif-from-rflags", "cet-sss", "avx10", "a7c1d_reserved20", "apx_f", "a7c1d_reserved22", "mwait"
        ];

        internal static readonly string[] eax7Ecx2EdxFeatures =
        [
            "psfd", "ipred_ctrl", "rrsba_ctrl", "ddpd_u", "bhi_ctrl", "mcdt_no", "uc_lock_no", "monitor_mitg_no"
        ];

        internal static readonly string[] knownHypervisorBrands =
        [
            "Microsoft Hv",
            "KVMKVMKVM\0\0\0",
            " KVMKVMKVM  ",
            "Linux KVM Hv",
            "BHyVE BHyVE ",
            "bhyve bhyve ",
            "XenVMMXenVMM",
            "TCGTCGTCGTCG",
            " lrpepyh  vr",
            " prl hyperv ",
            " lrpepyh vr ",
            " QNXQVMBSQG ",
            "VMwareVMware",
            "ACRNACRNACRN",
            "VBoxVBoxVBox",
            "QXNQSBMV",
            "___ NVMM ___",
            "OpenBSDVMM58",
            "Jailhouse\0\0\0",
            "HAXMHAXMHAXM",
            "EVMMEVMMEVMM",
            "UnisysSpar64",
            "SRESRESRESRE",
        ];

        internal static readonly Dictionary<string, string> vendorMappings = new()
        {
            { "AuthenticAMD", "AMD" },
            { "GenuineIntel", "Intel" },
        };
    }
}
