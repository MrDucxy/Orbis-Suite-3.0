#include "Common.h"
#include "LncUtil.h"

uint64_t LncUtil::LibraryBaseAddress = 0;
int(*LncUtil::_sceLncUtilGetAppId)(const char*);
int(*LncUtil::_sceLncUtilLaunchApp)(const char* titleId, char* args, LaunchAppParam* appParam);
int(*LncUtil::_sceLncUtilSuspendApp)(int AppId, int Flag);
int(*LncUtil::_sceLncUtilResumeApp)(int AppId, int Flag);

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
	_sceLncUtilLaunchApp = (decltype(_sceLncUtilLaunchApp))(LibraryBaseAddress + 0x4C10);
	_sceLncUtilSuspendApp = (decltype(_sceLncUtilSuspendApp))(LibraryBaseAddress + 0x4F20);
	_sceLncUtilResumeApp = (decltype(_sceLncUtilResumeApp))(LibraryBaseAddress + 0x4F40);

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

int LncUtil::sceLncUtilLaunchApp(const char* titleId, char* args, LaunchAppParam* appParam)
{
	if ((uint64_t)_sceLncUtilLaunchApp > 0x4C10)
	{
		return _sceLncUtilLaunchApp(titleId, args, appParam);
	}
	else
	{
		klog("failed to resolve sceLncUtilLaunchApp\n");
		return -1;
	}
}

int LncUtil::sceLncUtilSuspendApp(int AppId, int Flag)
{
	if ((uint64_t)_sceLncUtilSuspendApp > 0x4F20)
	{
		return _sceLncUtilSuspendApp(AppId, Flag);
	}
	else
	{
		klog("failed to resolve sceLncUtilSuspendApp\n");
		return -1;
	}
}

int LncUtil::sceLncUtilResumeApp(int AppId, int Flag)
{
	if ((uint64_t)_sceLncUtilResumeApp > 0x4F40)
	{
		return _sceLncUtilResumeApp(AppId, Flag);
	}
	else
	{
		klog("failed to resolve sceLncUtilResumeApp\n");
		return -1;
	}
}
