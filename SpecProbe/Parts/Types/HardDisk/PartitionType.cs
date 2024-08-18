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

namespace SpecProbe.Parts.Types.HardDisk
{
    /// <summary>
    /// Partition type
    /// </summary>
    public enum PartitionType
    {
        /// <summary>
        /// Unallocated partition
        /// </summary>
        Unallocated = 0x00,
        /// <summary>
        /// FAT12 partition
        /// </summary>
        FAT12 = 0x01,
        /// <summary>
        /// Xenix Root partition
        /// </summary>
        XenixRoot = 0x02,
        /// <summary>
        /// Xenix /usr partition
        /// </summary>
        XenixUsr = 0x03,
        /// <summary>
        /// FAT16 partition &lt; 32 MB
        /// </summary>
        FAT16 = 0x04,
        /// <summary>
        /// Extended partition
        /// </summary>
        Extended = 0x05,
        /// <summary>
        /// FAT16 partition >= 32 MB
        /// </summary>
        FAT16Large = 0x06,
        /// <summary>
        /// NTFS/HPFS/exFAT partition
        /// </summary>
        NTFS = 0x07,
        /// <summary>
        /// AIX partition
        /// </summary>
        Aix = 0x08,
        /// <summary>
        /// AIX boot partition
        /// </summary>
        AixBoot = 0x09,
        /// <summary>
        /// IBM OS/2 Warp Boot Manager Software
        /// </summary>
        OS2BootManager = 0x0A,
        /// <summary>
        /// FAT32 partition
        /// </summary>
        FAT32 = 0x0B,
        /// <summary>
        /// FAT32 partition with Large Block Allocation (LBA) support
        /// </summary>
        FAT32LBA = 0x0C,
        /// <summary>
        /// FAT16 partition with Large Block Allocation (LBA) support
        /// </summary>
        FAT16LBA = 0x0E,
        /// <summary>
        /// Windows 95 extended partition with Large Block Allocation (LBA) support
        /// </summary>
        ExtendedLBA = 0x0F,
        /// <summary>
        /// OPUS partition
        /// </summary>
        OPUS = 0x10,
        /// <summary>
        /// Hidden FAT32 partition
        /// </summary>
        FAT32Hidden = 0x11,
        /// <summary>
        /// COMPAQ Diagnostics partition
        /// </summary>
        CompaqDiagnostics = 0x12,
        /// <summary>
        /// Hidden FAT16 partition &lt; 32 MB
        /// </summary>
        HiddenFAT16 = 0x14,
        /// <summary>
        /// Hidden FAT16 partition >= 32 MB
        /// </summary>
        HiddenFAT16Large = 0x16,
        /// <summary>
        /// Hidden NTFS/HPFS/exFAT partition
        /// </summary>
        HiddenNtfs = 0x17,
        /// <summary>
        /// AST SmartSleep / Zero Volt Suspend partition for AST Ascentia '96 laptops
        /// </summary>
        AstSmartSleep = 0x18,
        /// <summary>
        /// Willowtech Photon COS
        /// </summary>
        WillowtechCOS = 0x19,
        /// <summary>
        /// Hidden FAT32 partition
        /// </summary>
        HiddenFAT32 = 0x1B,
        /// <summary>
        /// Hidden FAT32 partition with Large Block Allocation (LBA) support
        /// </summary>
        HiddenFAT32LBA = 0x1C,
        /// <summary>
        /// Hidden FAT16 partition with Large Block Allocation (LBA) support
        /// </summary>
        HiddenFAT16LBA = 0x1E,
        /// <summary>
        /// Willowtech Overture File System (OFS1)
        /// </summary>
        WillowtechOFS = 0x20,
        /// <summary>
        /// Oxygen File System
        /// </summary>
        OxygenFS = 0x21,
        /// <summary>
        /// Oxygen Extended Partition Table
        /// </summary>
        OxygenExtendedPartitionTable = 0x22,
        /// <summary>
        /// NEC DOS 3.x
        /// </summary>
        NecDos3 = 0x24,
        /// <summary>
        /// Windows Recovery Environment Hidden Partition
        /// </summary>
        WinReHidden = 0x27,
        /// <summary>
        /// Open source AtheOS File System partition for Syllable OS
        /// </summary>
        SyllableOS = 0x2A,
        /// <summary>
        /// Open source AtheOS File System partition with added security for Syllable OS
        /// </summary>
        SyllableSecure = 0x2B,
        /// <summary>
        /// Alien Internet Services' NOS, an operating system made in Melbourne, Australia
        /// </summary>
        NOS = 0x32,
        /// <summary>
        /// IBM AIX Journaling File System for use with eComStation and IBM OS/2 Warp Server for eBusiness
        /// </summary>
        JFS = 0x35,
        /// <summary>
        /// THEOS version 3.2, 2 GB partition
        /// </summary>
        TheOS = 0x38,
        /// <summary>
        /// Bell Labs Plan 9 system partition
        /// </summary>
        Plan9 = 0x39,
        /// <summary>
        /// THEOS version 4.0, 4 GB partition
        /// </summary>
        TheOSVer4 = 0x3a,
        /// <summary>
        /// THEOS version 4.0, extended partition
        /// </summary>
        TheOSVer4Extended = 0x3b,
        /// <summary>
        /// Partition Magic Recovery partition that marks the recovery process as incomplete due to failure or power outage
        /// </summary>
        PartitionMagicRecovery = 0x3c,
        /// <summary>
        /// Novell NetWare hidden partition
        /// </summary>
        NovellNetwareHidden = 0x3d,
        /// <summary>
        /// VenturCom VENIX 80286 partition for DEC and IBM PCs and their compatibles
        /// </summary>
        Venix = 0x40,
        /// <summary>
        /// PowerPC PReP (PowerPC Reference Platform) boot partition
        /// </summary>
        PowerPCReference = 0x41,
        /// <summary>
        /// SFS/LDM partition
        /// </summary>
        SFS = 0x42,
        /// <summary>
        /// Linux Native partition that is shared with Caldera DR-DOS
        /// </summary>
        LinuxNativeDrDos = 0x43,
        /// <summary>
        /// GoBack (time machine for hard disks) partition
        /// </summary>
        GoBack = 0x44,
        /// <summary>
        /// Boot-Us Boot Manager partition
        /// </summary>
        BootUs = 0x45,
        /// <summary>
        /// EUMEL or Elan partition
        /// </summary>
        Eumel1 = 0x46,
        /// <summary>
        /// EUMEL or Elan partition
        /// </summary>
        Eumel2 = 0x47,
        /// <summary>
        /// EUMEL or Elan partition
        /// </summary>
        Eumel3 = 0x48,
        /// <summary>
        /// Mark Aitchison's ALFS/THIN lightweight filesystem for DOS
        /// </summary>
        Alfs = 0x4A,
        /// <summary>
        /// Oberon partition
        /// </summary>
        Oberon = 0x4C,
        /// <summary>
        /// QNX 4.x partition (Part 1 of 3)
        /// </summary>
        Qnx4Part1 = 0x4D,
        /// <summary>
        /// QNX 4.x partition (Part 2 of 3)
        /// </summary>
        Qnx4Part2 = 0x4E,
        /// <summary>
        /// QNX 4.x partition (Part 3 of 3)
        /// </summary>
        Qnx4Part3 = 0x4F,
        /// <summary>
        /// OnTrack Disk Manager Legacy Versions (read-only)
        /// </summary>
        OnTrackLegacy = 0x50,
        /// <summary>
        /// OnTrack Disk Manager version 6 (read-write, auxiliary 1)
        /// </summary>
        OnTrack6 = 0x51,
        /// <summary>
        /// CP/M or Microport SysV/AT
        /// </summary>
        CPM = 0x52,
        /// <summary>
        /// OnTrack Disk Manager version 6 (read-write, auxiliary 3)
        /// </summary>
        OnTrack6Aux3 = 0x53,
        /// <summary>
        /// OnTrack Disk Manager version 6 (read-write, dynamic drive overlay)
        /// </summary>
        OnTrack6DDO = 0x54,
        /// <summary>
        /// StorageSoft EZ-Drive Disk Manager
        /// </summary>
        EZDrive = 0x55,
        /// <summary>
        /// Golden Bow VFeature partitioned volume
        /// </summary>
        GoldenBow = 0x56,
        /// <summary>
        /// StorageSoft DrivePro
        /// </summary>
        DrivePro = 0x57,
        /// <summary>
        /// Priam EDisk partitioned volume
        /// </summary>
        PriamEDisk = 0x5C,
        /// <summary>
        /// Storage Dimensions SpeedStor volume
        /// </summary>
        SpeedStor = 0x61,
        /// <summary>
        /// Unix System V and its derivatives, GNU Hurd, or Mach
        /// </summary>
        UnixSystemV = 0x63,
        /// <summary>
        /// Novell NetWare 286
        /// </summary>
        Netware286 = 0x64,
        /// <summary>
        /// Novell NetWare 386
        /// </summary>
        Netware386 = 0x65,
        /// <summary>
        /// Novell NetWare Storage Management Services (SMS)
        /// </summary>
        NetwareSms = 0x66,
        /// <summary>
        /// Wolf Mountain or Novell partition
        /// </summary>
        WolfMountain = 0x67,
        /// <summary>
        /// Novell partition
        /// </summary>
        Novell = 0x68,
        /// <summary>
        /// Novell NetWare 5 partition
        /// </summary>
        Netware5 = 0x69,
        /// <summary>
        /// DiskSecure MultiBoot partition
        /// </summary>
        MultiBoot = 0x70,
        /// <summary>
        /// V7/x86 (Unix version 7 for PCs) partition
        /// </summary>
        V7 = 0x72,
        /// <summary>
        /// Scramdisk partition for the disk encryption software
        /// </summary>
        Scramdisk = 0x74,
        /// <summary>
        /// IBM PC/IX partition
        /// </summary>
        PCIX = 0x75,
        /// <summary>
        /// M2FS/M2CS partition
        /// </summary>
        M2FS = 0x77,
        /// <summary>
        /// XOSL Bootloader File System partition
        /// </summary>
        XOSL = 0x78,
        /// <summary>
        /// F.I.X. partition
        /// </summary>
        FIX = 0x7e,
        /// <summary>
        /// Alt-OS-Development Partition Standard for ADAOS
        /// </summary>
        AltOs = 0x7f,
        /// <summary>
        /// Old Minix or NTFT partition
        /// </summary>
        OldMinix = 0x80,
        /// <summary>
        /// New Minix partition
        /// </summary>
        NewMinix = 0x81,
        /// <summary>
        /// Linux Swap / Solaris partition
        /// </summary>
        SwapOrSolaris = 0x82,
        /// <summary>
        /// Linux partition
        /// </summary>
        Linux = 0x83,
        /// <summary>
        /// IBM OS/2 Warp Hidden C: partition
        /// </summary>
        OS2HiddenC = 0x84,
        /// <summary>
        /// Linux extended partition
        /// </summary>
        LinuxExtended = 0x85,
        /// <summary>
        /// FAT16 Volume Set for fault tolerant storage partitions
        /// </summary>
        FAT16VolumeSet = 0x86,
        /// <summary>
        /// NTFS Volume Set for fault tolerant storage partitions
        /// </summary>
        NTFSVolumeSet = 0x87,
        /// <summary>
        /// Linux plain text partition table
        /// </summary>
        LinuxPlainText = 0x88,
        /// <summary>
        /// Linux kernel partition for the AiR-BOOT bootloader
        /// </summary>
        LinuxKernelAirboot = 0x8a,
        /// <summary>
        /// FAT32 Volume Set for fault tolerant storage partitions
        /// </summary>
        FAT32VolumeSet = 0x8b,
        /// <summary>
        /// FAT32 Volume Set for fault tolerant storage partitions using INT 13h
        /// </summary>
        FAT32VolumeSetExtended = 0x8c,
        /// <summary>
        /// Free FDISK 0.96+ hidden primary FAT12 partition
        /// </summary>
        FreeFdiskFAT12 = 0x8d,
        /// <summary>
        /// Linux Logical Volume Manager (LVM) partition
        /// </summary>
        LinuxLVM = 0x8e,
        /// <summary>
        /// Free FDISK 0.96+ hidden primary FAT16 partition
        /// </summary>
        FreeFdiskFAT16 = 0x90,
        /// <summary>
        /// Free FDISK 0.96+ hidden primary DOS extended partition
        /// </summary>
        FreeFdiskExtended = 0x91,
        /// <summary>
        /// Free FDISK 0.96+ hidden primary large FAT16 partition
        /// </summary>
        FreeFdiskFAT16Large = 0x92,
        /// <summary>
        /// Amoeba system partition
        /// </summary>
        Amoeba = 0x93,
        /// <summary>
        /// Amoeba Bad Block Table (BBT) partition
        /// </summary>
        AmoebaBBT = 0x94,
        /// <summary>
        /// MIT ExoPC partition
        /// </summary>
        ExoPC = 0x95,
        /// <summary>
        /// CHRP ISO-9660 partition
        /// </summary>
        ChrpIso9660 = 0x96,
        /// <summary>
        /// Free FDISK 0.96+ hidden primary FAT32 partition
        /// </summary>
        FreeFdiskFAT32 = 0x97,
        /// <summary>
        /// Free FDISK 0.96+ hidden primary FAT32 partition with Large Block Allocation (LBA) support
        /// </summary>
        FreeFdiskFAT32LBA = 0x98,
        /// <summary>
        /// DCE376 logical drive exclusive to the Mylex DCE376 EISA SCSI adapter
        /// </summary>
        DCE376 = 0x99,
        /// <summary>
        /// Free FDISK 0.96+ hidden primary FAT16 partition with Large Block Allocation (LBA) support
        /// </summary>
        FreeFdiskFAT16LBA = 0x9a,
        /// <summary>
        /// Free FDISK 0.96+ hidden primary extended partition with Large Block Allocation (LBA) support
        /// </summary>
        FreeFdiskExtendedLBA = 0x9b,
        /// <summary>
        /// ForthOS, eForth, VSTA system partition
        /// </summary>
        ForthOS = 0x9e,
        /// <summary>
        /// BSD/OS v4.1 partition
        /// </summary>
        BSDOS = 0x9f,
        /// <summary>
        /// IBM ThinkPad, Phoenix ThinkBIOS, Toshiba laptop hibernation partition
        /// </summary>
        LaptopHibernationIBM = 0xa0,
        /// <summary>
        /// NEC Versa 6000H laptop hibernation partition
        /// </summary>
        LaptopHibernationNEC = 0xa1,
        /// <summary>
        /// HP Volume Expansion (SpeedStor variant) partition
        /// </summary>
        HPVolumeExpansion1 = 0xa3,
        /// <summary>
        /// HP Volume Expansion (SpeedStor variant) partition
        /// </summary>
        HPVolumeExpansion2 = 0xa4,
        /// <summary>
        /// BSD/386, 386BSD, NetBSD, FreeBSD system partition
        /// </summary>
        BSD386 = 0xa5,
        /// <summary>
        /// OpenBSD system partition
        /// </summary>
        OpenBSD = 0xa6,
        /// <summary>
        /// NeXTSTEP system partition
        /// </summary>
        NextStep = 0xa7,
        /// <summary>
        /// UFS File System for old versions of Mac OS X
        /// </summary>
        MacOSX = 0xa8,
        /// <summary>
        /// NetBSD system partition
        /// </summary>
        NetBSD = 0xa9,
        /// <summary>
        /// Olivetti FAT12 1.44MB Service Partition
        /// </summary>
        Olivetti = 0xaa,
        /// <summary>
        /// Boot partition for old versions of Mac OS X
        /// </summary>
        MacOSXBoot = 0xab,
        /// <summary>
        /// ADFS File System for RISC OS
        /// </summary>
        ADFS = 0xad,
        /// <summary>
        /// ShagOS File System
        /// </summary>
        ShagOS = 0xae,
        /// <summary>
        /// HFS or HFS+ for Apple macOS
        /// </summary>
        HFS = 0xaf,
        /// <summary>
        /// BootStar Boot Manager, dummy partition
        /// </summary>
        BootStarDummy = 0xb0,
        /// <summary>
        /// QNX Neutrino PowerSafe filesystem
        /// </summary>
        NeutrinoPowerSafe1 = 0xb1,
        /// <summary>
        /// QNX Neutrino PowerSafe filesystem
        /// </summary>
        NeutrinoPowerSafe2 = 0xb2,
        /// <summary>
        /// QNX Neutrino PowerSafe filesystem
        /// </summary>
        NeutrinoPowerSafe3 = 0xb3,
        /// <summary>
        /// HP Volume Expansion (SpeedStor variant) partition
        /// </summary>
        HPVolumeExpansion3 = 0xb4,
        /// <summary>
        /// HP Volume Expansion (SpeedStor variant) partition
        /// </summary>
        HPVolumeExpansion4 = 0xb6,
        /// <summary>
        /// BSDI BSD/386 filesystem partition
        /// </summary>
        BsdiBSD = 0xb7,
        /// <summary>
        /// BSDI BSD/386 swap partition
        /// </summary>
        BsdiBSDSwap = 0xb8,
        /// <summary>
        /// PhysTechSoft (PTS) BootWizard 4.x or Acronis OS Selector 5.x partition
        /// </summary>
        BootWizard = 0xbb,
        /// <summary>
        /// Acronis Backup partition
        /// </summary>
        AcronisBackup = 0xbc,
        /// <summary>
        /// BonnyDOS/286 partition
        /// </summary>
        BonnyDos = 0xbd,
        /// <summary>
        /// Oracle Solaris 8 boot partition
        /// </summary>
        Solaris8Boot = 0xbe,
        /// <summary>
        /// Oracle Solaris new system partition
        /// </summary>
        SolarisNew = 0xbf,
        /// <summary>
        /// Valid NTFT partition, DR-DOS 7.02+ / OpenDOS 7.01 / Novell DOS 7 secured partition, or REAL/32 secure small partition
        /// </summary>
        NTFT = 0xC0,
        /// <summary>
        /// Caldera DR/DOS secured FAT12 partition
        /// </summary>
        DrDosSecured = 0xC1,
        /// <summary>
        /// Hidden Linux filesystem partition
        /// </summary>
        HiddenLinux = 0xC2,
        /// <summary>
        /// Hidden Linux swap partition
        /// </summary>
        HiddenLinuxSwap = 0xC3,
        /// <summary>
        /// Caldera DR-DOS secured FAT16 partition &lt; 32 MB
        /// </summary>
        DrDosSecuredFAT16 = 0xC4,
        /// <summary>
        /// Caldera DR-DOS secured extended partition
        /// </summary>
        DrDosSecuredExtended = 0xC5,
        /// <summary>
        /// Caldera DR-DOS secured FAT16 partition >= 32 MB
        /// </summary>
        DrDosSecuredFAT16Large = 0xC6,
        /// <summary>
        /// Syrinx boot partition
        /// </summary>
        SyrinxBoot = 0xC7,
        /// <summary>
        /// Caldera DR-DOS 8+ reserved partition
        /// </summary>
        DrDos8_c8 = 0xC8,
        /// <summary>
        /// Caldera DR-DOS 8+ reserved partition
        /// </summary>
        DrDos8_c9 = 0xC9,
        /// <summary>
        /// Caldera DR-DOS 8+ reserved partition
        /// </summary>
        DrDos8_ca = 0xCA,
        /// <summary>
        /// Caldera DR-DOS 7.04+ secured FAT32 partition
        /// </summary>
        DrDosSecuredFAT32 = 0xCB,
        /// <summary>
        /// Caldera DR-DOS 7.04+ secured FAT32 partition with Large Block Allocation (LBA) support
        /// </summary>
        DrDosSecuredFAT32LBA = 0xCC,
        /// <summary>
        /// CTOS Memory Dump partition
        /// </summary>
        CTOSMemdump = 0xCD,
        /// <summary>
        /// Caldera DR-DOS 7.04+ FAT16X partition with Large Block Allocation (LBA) support
        /// </summary>
        DrDosFAT16X = 0xCE,
        /// <summary>
        /// Caldera DR-DOS 7.04+ secured extended partition with Large Block Allocation (LBA) support
        /// </summary>
        DrDosSecuredExtendedLBA = 0xCF,
        /// <summary>
        /// REAL/32 secure big partition (FAT12, FAT16, or FAT32)
        /// </summary>
        Real32SecureBig = 0xD0,
        /// <summary>
        /// Old Multiuser DOS Secured FAT12 partition
        /// </summary>
        MultiuserDosSecuredFAT12 = 0xD1,
        /// <summary>
        /// Old Multiuser DOS Secured FAT16 partition &lt; 32 MB
        /// </summary>
        MultiuserDosSecuredFAT16 = 0xD4,
        /// <summary>
        /// Old Multiuser DOS Secured extended partition
        /// </summary>
        MultiuserDosSecuredExtended = 0xD5,
        /// <summary>
        /// Old Multiuser DOS Secured FAT16 partition >= 32 MB
        /// </summary>
        MultiuserDosSecuredFAT16Large = 0xD6,
        /// <summary>
        /// CP/M for x86 PCs
        /// </summary>
        CPM86 = 0xD8,
        /// <summary>
        /// Non-filesystem raw data partition
        /// </summary>
        Data = 0xDA,
        /// <summary>
        /// Digital Research CP/M, Concurrent CP/M, or Concurrent DOS
        /// </summary>
        ConcurrentDos = 0xDB,
        /// <summary>
        /// CTOS Memory Dump hidden partition
        /// </summary>
        CTOSMemdumpHidden = 0xDD,
        /// <summary>
        /// Dell PowerEdge server utility partition
        /// </summary>
        DellPowerEdgeServerUtil = 0xDE,
        /// <summary>
        /// BootIt EMBRM partition table
        /// </summary>
        BootIt = 0xDF,
        /// <summary>
        /// STMicroelectronics AVFS partition
        /// </summary>
        AVFS = 0xE0,
        /// <summary>
        /// FAT12 SpeedStor extended partition
        /// </summary>
        FAT12SpeedStorExtended = 0xE1,
        /// <summary>
        /// DOS R/O
        /// </summary>
        DosRo = 0xE3,
        /// <summary>
        /// FAT16 SpeedStor extended partition
        /// </summary>
        FAT16SpeedStorExtended = 0xE4,
        /// <summary>
        /// Tandy MS-DOS with logically sectored FAT
        /// </summary>
        TandyMSDOS = 0xE5,
        /// <summary>
        /// Storage Dimensions SpeedStor
        /// </summary>
        SDSpeedStor = 0xE6,
        /// <summary>
        /// Linux Unified Key Setup (LUKS) partition
        /// </summary>
        Luks = 0xE8,
        /// <summary>
        /// Unaligned unallocated space for Rufus
        /// </summary>
        RufusBoot = 0xEA,
        /// <summary>
        /// BeOS BFS partition
        /// </summary>
        BFS = 0xEB,
        /// <summary>
        /// SkyOS SkyFS partition
        /// </summary>
        SkyFS = 0xEC,
        /// <summary>
        /// EFI indicator
        /// </summary>
        EFI = 0xEE,
        /// <summary>
        /// EFI system partition
        /// </summary>
        EFISystem = 0xEF,
        /// <summary>
        /// Linux/PA-RISC bootloader partition
        /// </summary>
        PaRiscBootloader = 0xF0,
        /// <summary>
        /// Storage Dimensions SpeedStor
        /// </summary>
        SDSpeedStor1 = 0xF1,
        /// <summary>
        /// DOS 3.3+ Secondary partition
        /// </summary>
        Dos33Secondary = 0xF2,
        /// <summary>
        /// Storage Dimensions SpeedStor
        /// </summary>
        SDSpeedStor2 = 0xF3,
        /// <summary>
        /// SpeedStor large partition
        /// </summary>
        SpeedStorLarge = 0xF4,
        /// <summary>
        /// Prologue multi-volume partition
        /// </summary>
        PrologueMultiVolume = 0xF5,
        /// <summary>
        /// Storage Dimensions SpeedStor
        /// </summary>
        SDSpeedStor3 = 0xF6,
        /// <summary>
        /// DDRDrive Solid State Filesystem
        /// </summary>
        SSFS = 0xF7,
        /// <summary>
        /// pCache ext2/3 persistent cache partition
        /// </summary>
        PCache = 0xF9,
        /// <summary>
        /// Bochs partition
        /// </summary>
        Bochs = 0xFA,
        /// <summary>
        /// VMware File System partition
        /// </summary>
        VmwareFS = 0xFB,
        /// <summary>
        /// VMware Swap partition
        /// </summary>
        VmwareSwap = 0xFC,
        /// <summary>
        /// Linux raid partition with autodetect using persistent superblock
        /// </summary>
        LinuxRaid = 0xFD,
        /// <summary>
        /// LANstep or SpeedStor &gt; 1024 cylinders
        /// </summary>
        LANstep = 0xFE,
        /// <summary>
        /// Xenix Bad Block Table (BBT)
        /// </summary>
        XenixBBT = 0xFF,
        /// <summary>
        /// [GPT] BIOS boot partition
        /// </summary>
        GptBiosBoot = 0x10000000,
        /// <summary>
        /// [GPT] MBR partition scheme
        /// </summary>
        GptMbrScheme = 0x10000001,
        /// <summary>
        /// [GPT] HP/UX Data
        /// </summary>
        GptHpUxData = 0x10000002,
        /// <summary>
        /// [GPT] HP/UX Service
        /// </summary>
        GptHpUxService = 0x10000003,
        /// <summary>
        /// [GPT] Linux dm-crypt
        /// </summary>
        GptLinuxDmCrypt = 0x10000004,
        /// <summary>
        /// [GPT] FreeBSD Boot
        /// </summary>
        GptFreeBsdBoot = 0x10000005,
        /// <summary>
        /// [GPT] FreeBSD Swap
        /// </summary>
        GptFreeBsdSwap = 0x10000006,
        /// <summary>
        /// [GPT] FreeBSD UFS
        /// </summary>
        GptFreeBsdUfs = 0x10000007,
        /// <summary>
        /// [GPT] FreeBSD Vinum
        /// </summary>
        GptFreeBsdVinum = 0x10000008,
        /// <summary>
        /// [GPT] FreeBSD ZFS
        /// </summary>
        GptFreeBsdZfs = 0x10000009,
        /// <summary>
        /// [GPT] macOS ZFS
        /// </summary>
        GptMacOSZfs = 0x1000000A,
        /// <summary>
        /// [GPT] macOS Online RAID
        /// </summary>
        GptMacOSOnlineRaid = 0x1000000B,
        /// <summary>
        /// [GPT] macOS Offline RAID
        /// </summary>
        GptMacOSOfflineRaid = 0x1000000C,
        /// <summary>
        /// [GPT] macOS Label
        /// </summary>
        GptMacOSLabel = 0x1000000D,
        /// <summary>
        /// [GPT] macOS Apple TV Recovery
        /// </summary>
        GptMacOSAppleTVRecovery = 0x1000000E,
        /// <summary>
        /// [GPT] Oracle Solaris Backup
        /// </summary>
        GptSolarisBackup = 0x1000000F,
        /// <summary>
        /// [GPT] Oracle Solaris EFI_ALTSCTR
        /// </summary>
        GptSolarisAltsctr = 0x10000010,
        /// <summary>
        /// [GPT] Oracle Solaris Reserved
        /// </summary>
        GptSolarisReserved1 = 0x10000011,
        /// <summary>
        /// [GPT] Oracle Solaris Reserved
        /// </summary>
        GptSolarisReserved2 = 0x10000012,
        /// <summary>
        /// [GPT] Oracle Solaris Reserved
        /// </summary>
        GptSolarisReserved3 = 0x10000013,
        /// <summary>
        /// [GPT] Oracle Solaris Reserved
        /// </summary>
        GptSolarisReserved4 = 0x10000014,
        /// <summary>
        /// [GPT] Oracle Solaris Reserved
        /// </summary>
        GptSolarisReserved5 = 0x10000015,
        /// <summary>
        /// [GPT] NetBSD Swap
        /// </summary>
        GptNetBSDSwap = 0x10000016,
        /// <summary>
        /// [GPT] NetBSD FFS
        /// </summary>
        GptNetBSDFFS = 0x10000017,
        /// <summary>
        /// [GPT] NetBSD LFS
        /// </summary>
        GptNetBSDLFS = 0x10000018,
        /// <summary>
        /// [GPT] NetBSD RAID
        /// </summary>
        GptNetBSDRAID = 0x10000019,
        /// <summary>
        /// [GPT] NetBSD Concatenated
        /// </summary>
        GptNetBSDConcatenated = 0x1000001A,
        /// <summary>
        /// [GPT] NetBSD Encrypted
        /// </summary>
        GptNetBSDEncrypted = 0x1000001B,
        /// <summary>
        /// Unknown partition type
        /// </summary>
        Unknown = -1,
    }
}
