#include "Common.h"
#include "ShellCoreUtil.h"

uint64_t ShellCoreUtil::LibraryBaseAddress = 0;
int(*ShellCoreUtil::_sceShellCoreUtilGetFreeSizeOfUserPartition)(uint64_t* free, uint64_t* total);

#ifdef FIRMWARE_505
#define sceShellCoreUtilGetFreeSizeOfUserPartitionOffset 0x105A0
#endif

#ifdef FIRMWARE_900
#define sceShellCoreUtilGetFreeSizeOfUserPartitionOffset 0x10B40
#endif

int ShellCoreUtil::Init()
{
	// Load the prx or get its module handle.
	int libHandle = sceKernelLoadStartModule("/system/common/lib/libSceSystemService.sprx", 0, nullptr, 0, nullptr, nullptr);
	if (libHandle == 0) {
		klog("Failed to load libSceSystemService Library.\n");
		return -1;
	}

	// Get module info so we can find the base address.
	OrbisKernelModuleInfo moduleInfo;
	moduleInfo.size = sizeof(OrbisKernelModuleInfo);
	if (sceKernelGetModuleInfo(libHandle, &moduleInfo) != 0)
	{
		klog("Failed to get libSceSystemService module info.\n");
		return -1;
	}

	// Save base address for later.
	LibraryBaseAddress = (uint64_t)moduleInfo.segmentInfo[0].address;

	if (LibraryBaseAddress <= 0)
	{
		klog("Failed to get libSceSystemService base address.\n");
		return -1;
	}

	// Set up Functions.
	_sceShellCoreUtilGetFreeSizeOfUserPartition = (decltype(_sceShellCoreUtilGetFreeSizeOfUserPartition))(LibraryBaseAddress + sceShellCoreUtilGetFreeSizeOfUserPartitionOffset);

	return 0;
}

int ShellCoreUtil::sceShellCoreUtilGetFreeSizeOfUserPartition(uint64_t* free, uint64_t* total)
{
	if ((uint64_t)_sceShellCoreUtilGetFreeSizeOfUserPartition > sceShellCoreUtilGetFreeSizeOfUserPartitionOffset)
	{
		return _sceShellCoreUtilGetFreeSizeOfUserPartition(free, total);
	}
	else
	{
		klog("failed to resolve sceShellCoreUtilGetFreeSizeOfUserPartition\n");
		return -1;
	}
}