using System;
using System.Collections.Generic;
using System.Text;

namespace SpecProbe.Parts.Types.HardDisk
{
    /// <summary>
    /// GPT partition type tools
    /// </summary>
    public static class GptPartitionTypeTools
    {
        /// <summary>
        /// Translates the partition type GUID to MBR <see cref="PartitionType">partition type</see>
        /// </summary>
        /// <param name="partitionTypeGptGuid">GPT GUID that describes a partition type</param>
        /// <returns>MBR <see cref="PartitionType">partition type</see></returns>
        public static PartitionType TranslateFromGpt(Guid partitionTypeGptGuid)
        {
            if (partitionTypeGptGuid == Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                // Unused entry
                return PartitionType.Unallocated;
            }
            else if (partitionTypeGptGuid == Guid.Parse("c12a7328-f81f-11d2-ba4b-00a0c93ec93b"))
            {
                // EFI system
                return PartitionType.EFISystem;
            }
            else if (partitionTypeGptGuid == Guid.Parse("21686148-6449-6e6f-744e-656564454649"))
            {
                // BIOS boot system
                return PartitionType.GptBiosBoot;
            }
            else if (partitionTypeGptGuid == Guid.Parse("024DEE41-33E7-11D3-9D69-0008C781F39F"))
            {
                // BIOS boot system
                return PartitionType.GptMbrScheme;
            }
            else if (partitionTypeGptGuid == Guid.Parse("ebd0a0a2-b9e5-4433-87c0-68b6b72699c7"))
            {
                // Basic data (Windows)
                return PartitionType.NTFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("e3c9e316-0b5c-4db8-817d-f92df00215ae"))
            {
                // Microsoft Reserved
                return PartitionType.FAT32;
            }
            else if (partitionTypeGptGuid == Guid.Parse("de94bba4-06d1-4d40-a16a-bfd50179d6ac"))
            {
                // Microsoft Recovery
                return PartitionType.FAT32;
            }
            else if (partitionTypeGptGuid == Guid.Parse("5808c8aa-7e8f-42e0-85d2-e1e90434cfb3"))
            {
                // LDM metadata
                return PartitionType.SFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("af9b60a0-1431-4f62-bc68-3311714a69ad"))
            {
                // LDM metadata
                return PartitionType.SFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("75894C1E-3AEB-11D3-B7C1-7B03A0000000"))
            {
                // HP/UX Data
                return PartitionType.GptHpUxData;
            }
            else if (partitionTypeGptGuid == Guid.Parse("E2A1E728-32E3-11D6-A682-7B03A0000000"))
            {
                // HP/UX Service
                return PartitionType.GptHpUxService;
            }
            else if (partitionTypeGptGuid == Guid.Parse("0FC63DAF-8483-4772-8E79-3D69D8477DE4"))
            {
                // Linux
                return PartitionType.Linux;
            }
            else if (partitionTypeGptGuid == Guid.Parse("4F68BCE3-E8CD-4DB1-96E7-FBCAF984B709"))
            {
                // Linux
                return PartitionType.Linux;
            }
            else if (partitionTypeGptGuid == Guid.Parse("0657FD6D-A4AB-43C4-84E5-0933C84B4F4F"))
            {
                // Linux Swap
                return PartitionType.SwapOrSolaris;
            }
            else if (partitionTypeGptGuid == Guid.Parse("933AC7E1-2EB4-4F13-B844-0E14E2AEF915"))
            {
                // Linux
                return PartitionType.Linux;
            }
            else if (partitionTypeGptGuid == Guid.Parse("3B8F8425-20E0-4F3B-907F-1A25A76F98E8"))
            {
                // Linux
                return PartitionType.Linux;
            }
            else if (partitionTypeGptGuid == Guid.Parse("4D21B016-B534-45C2-A9FB-5C16E091FD2D"))
            {
                // Linux
                return PartitionType.Linux;
            }
            else if (partitionTypeGptGuid == Guid.Parse("7EC6F557-3BC5-4ACA-B293-16EF5DF639D1"))
            {
                // Linux
                return PartitionType.Linux;
            }
            else if (partitionTypeGptGuid == Guid.Parse("E6D6D379-F507-44C2-A23C-238F2A3DF928"))
            {
                // Linux LVM
                return PartitionType.LinuxLVM;
            }
            else if (partitionTypeGptGuid == Guid.Parse("A19D880F-05FC-4D3B-A006-743F0F84911E"))
            {
                // Linux RAID
                return PartitionType.LinuxRaid;
            }
            else if (partitionTypeGptGuid == Guid.Parse("CA7D7CCB-63ED-4C53-861C-1742536059CC"))
            {
                // Linux LUKS
                return PartitionType.Luks;
            }
            else if (partitionTypeGptGuid == Guid.Parse("7FFEC5C9-2D00-49B7-8941-3EA10A5586B7"))
            {
                // Linux dm-crypt
                return PartitionType.GptLinuxDmCrypt;
            }
            else if (partitionTypeGptGuid == Guid.Parse("83BD6B9D-7F41-11DC-BE0B-001560B84F0F"))
            {
                // FreeBSD Boot
                return PartitionType.GptFreeBsdBoot;
            }
            else if (partitionTypeGptGuid == Guid.Parse("516E7CB4-6ECF-11D6-8FF8-00022D09712B"))
            {
                // FreeBSD Data
                return PartitionType.BSD386;
            }
            else if (partitionTypeGptGuid == Guid.Parse("516E7CB5-6ECF-11D6-8FF8-00022D09712B"))
            {
                // FreeBSD Swap
                return PartitionType.GptFreeBsdSwap;
            }
            else if (partitionTypeGptGuid == Guid.Parse("516E7CB6-6ECF-11D6-8FF8-00022D09712B"))
            {
                // FreeBSD UFS
                return PartitionType.GptFreeBsdUfs;
            }
            else if (partitionTypeGptGuid == Guid.Parse("516E7CB8-6ECF-11D6-8FF8-00022D09712B"))
            {
                // FreeBSD Vinum
                return PartitionType.GptFreeBsdVinum;
            }
            else if (partitionTypeGptGuid == Guid.Parse("516E7CBA-6ECF-11D6-8FF8-00022D09712B"))
            {
                // FreeBSD ZFS
                return PartitionType.GptFreeBsdZfs;
            }
            else if (partitionTypeGptGuid == Guid.Parse("48465300-0000-11AA-AA11-00306543ECAC"))
            {
                // macOS HFS+
                return PartitionType.HFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("55465300-0000-11AA-AA11-00306543ECAC"))
            {
                // macOS UFS
                return PartitionType.MacOSX;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A898CC3-1DD2-11B2-99A6-080020736631"))
            {
                // macOS ZFS
                return PartitionType.GptMacOSZfs;
            }
            else if (partitionTypeGptGuid == Guid.Parse("52414944-0000-11AA-AA11-00306543ECAC"))
            {
                // macOS RAID (online)
                return PartitionType.GptMacOSOnlineRaid;
            }
            else if (partitionTypeGptGuid == Guid.Parse("52414944-5F4F-11AA-AA11-00306543ECAC"))
            {
                // macOS RAID (offline)
                return PartitionType.GptMacOSOfflineRaid;
            }
            else if (partitionTypeGptGuid == Guid.Parse("426F6F74-0000-11AA-AA11-00306543ECAC"))
            {
                // macOS Boot
                return PartitionType.MacOSXBoot;
            }
            else if (partitionTypeGptGuid == Guid.Parse("4C616265-6C00-11AA-AA11-00306543ECAC"))
            {
                // macOS Label
                return PartitionType.GptMacOSLabel;
            }
            else if (partitionTypeGptGuid == Guid.Parse("5265636F-7665-11AA-AA11-00306543ECAC"))
            {
                // macOS Apple TV Recovery
                return PartitionType.GptMacOSAppleTVRecovery;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A82CB45-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris boot
                return PartitionType.Solaris8Boot;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A85CF4D-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris root
                return PartitionType.SolarisNew;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A87C46F-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris swap
                return PartitionType.SwapOrSolaris;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A8B642B-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris backup
                return PartitionType.GptSolarisBackup;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A898CC3-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris /usr
                return PartitionType.SolarisNew;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A8EF2E9-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris /var
                return PartitionType.SolarisNew;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A90BA39-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris /home
                return PartitionType.SolarisNew;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A9283A5-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris EFI_ALTSCTR
                return PartitionType.GptSolarisAltsctr;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A945A3B-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris Reserved
                return PartitionType.GptSolarisReserved1;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A9630D1-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris Reserved
                return PartitionType.GptSolarisReserved2;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A980767-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris Reserved
                return PartitionType.GptSolarisReserved3;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A96237F-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris Reserved
                return PartitionType.GptSolarisReserved4;
            }
            else if (partitionTypeGptGuid == Guid.Parse("6A8D2AC7-1DD2-11B2-99A6-080020736631"))
            {
                // Solaris Reserved
                return PartitionType.GptSolarisReserved5;
            }
            else if (partitionTypeGptGuid == Guid.Parse("49F48D32-B10E-11DC-B99B-0019D1879648"))
            {
                // NetBSD Swap
                return PartitionType.GptNetBSDSwap;
            }
            else if (partitionTypeGptGuid == Guid.Parse("49F48D5A-B10E-11DC-B99B-0019D1879648"))
            {
                // NetBSD FFS
                return PartitionType.GptNetBSDFFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("49F48D82-B10E-11DC-B99B-0019D1879648"))
            {
                // NetBSD LFS
                return PartitionType.GptNetBSDLFS;
            }
            else if (partitionTypeGptGuid == Guid.Parse("49F48DAA-B10E-11DC-B99B-0019D1879648"))
            {
                // NetBSD RAID
                return PartitionType.GptNetBSDRAID;
            }
            else if (partitionTypeGptGuid == Guid.Parse("2DB519C4-B10F-11DC-B99B-0019D1879648"))
            {
                // NetBSD Concatenated
                return PartitionType.GptNetBSDConcatenated;
            }
            else if (partitionTypeGptGuid == Guid.Parse("2DB519EC-B10F-11DC-B99B-0019D1879648"))
            {
                // NetBSD Encrypted
                return PartitionType.GptNetBSDEncrypted;
            }
            else if (partitionTypeGptGuid == Guid.Parse("03fedbca-aaaa-aaaa-aaaa-3f19aa5c2bb1"))
            {
                // Aptivi ParelOS Boot
                return PartitionType.ParelOSBoot;
            }
            else if (partitionTypeGptGuid == Guid.Parse("03fedbca-aaaa-aaaa-aaaa-2f19aa5c2bb2"))
            {
                // Aptivi ParelOS Data
                return PartitionType.ParelOSData;
            }
            else if (partitionTypeGptGuid == Guid.Parse("03fedbca-aaaa-aaaa-aaaa-1f19aa5c2bb3"))
            {
                // Aptivi ParelOS Swap
                return PartitionType.ParelOSSwap;
            }
            return PartitionType.Unknown;
        }
    }
}
