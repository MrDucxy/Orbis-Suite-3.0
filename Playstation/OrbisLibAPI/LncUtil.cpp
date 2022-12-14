#include "Common.h"
#include "LncUtil.h"

uint64_t LncUtil::LibraryBaseAddress = 0;
int(*LncUtil::_sceLncUtilGetAppId)(const char*);

int LncUtil::Init()
{
	// Load the prx or get its module handle.
	char Buffer[0x200];
	sprintf(Buffer, "/%s/common/lib/libSceSystemService.sprx", sceKernelGetFsSandboxRandomWord());
	int libHandle = sceKernelLoadStartModule(Buffer, 0, nullptr, 0, nullptr, nullptr);
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

	// Init the lnc util library.
	auto sceLncUtilInitialize = (int(*)())(LibraryBaseAddress + 0x4BF0);
	if (sceLncUtilInitialize() != 0)
	{
		klog("Failed to call sceLncUtilInitialize().\n");
		return -1;
	}

	// Set up Functions.
	_sceLncUtilGetAppId = (decltype(_sceLncUtilGetAppId))(LibraryBaseAddress + 0x4E10);

	return 0;
}

int LncUtil::sceLncUtilGetAppId(const char* TitleId)
{
	if ((uint64_t)_sceLncUtilGetAppId > 0x4E10)
	{
		return _sceLncUtilGetAppId(TitleId);
	}
	else
	{
		klog("failed to resolve sceLncUtilGetAppId\n");
		return -1;
	}
}