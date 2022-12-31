#include "Common.h"
#include "LncUtil.h"

uint64_t LncUtil::LibraryBaseAddress = 0;
int(*LncUtil::_sceLncUtilGetAppId)(const char*);
int(*LncUtil::_sceLncUtilLaunchApp)(const char* titleId, char* args, LaunchAppParam* appParam);
int(*LncUtil::_sceLncUtilSuspendApp)(int AppId, int Flag);
int(*LncUtil::_sceLncUtilResumeApp)(int AppId, int Flag);

#ifdef FIRMWARE_505
#define sceLncUtilInitializeOffset 0x4BF0
#define sceLncUtilGetAppIdOffset 0x4E10
#define sceLncUtilLaunchAppOffset 0x4C10
#define sceLncUtilSuspendAppOffset 0x4F20
#define sceLncUtilResumeAppOffset 0x4F40
#endif

#ifdef FIRMWARE_900
#define sceLncUtilInitializeOffset 0x5580
#define sceLncUtilGetAppIdOffset 0x5770
#define sceLncUtilLaunchAppOffset 0x55A0
#define sceLncUtilSuspendAppOffset 0x5880
#define sceLncUtilResumeAppOffset 0x58A0
#endif

int LncUtil::Init()
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

	// Init the lnc util library.
	auto sceLncUtilInitialize = (int(*)())(LibraryBaseAddress + sceLncUtilInitializeOffset);
	if (sceLncUtilInitialize() != 0)
	{
		klog("Failed to call sceLncUtilInitialize().\n");
		return -1;
	}

	// Set up Functions.
	_sceLncUtilGetAppId = (decltype(_sceLncUtilGetAppId))(LibraryBaseAddress + sceLncUtilGetAppIdOffset);
	_sceLncUtilLaunchApp = (decltype(_sceLncUtilLaunchApp))(LibraryBaseAddress + sceLncUtilLaunchAppOffset);
	_sceLncUtilSuspendApp = (decltype(_sceLncUtilSuspendApp))(LibraryBaseAddress + sceLncUtilSuspendAppOffset);
	_sceLncUtilResumeApp = (decltype(_sceLncUtilResumeApp))(LibraryBaseAddress + sceLncUtilResumeAppOffset);

	return 0;
}

int LncUtil::sceLncUtilGetAppId(const char* TitleId)
{
	if ((uint64_t)_sceLncUtilGetAppId > sceLncUtilGetAppIdOffset)
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
	if ((uint64_t)_sceLncUtilLaunchApp > sceLncUtilLaunchAppOffset)
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
	if ((uint64_t)_sceLncUtilSuspendApp > sceLncUtilSuspendAppOffset)
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
	if ((uint64_t)_sceLncUtilResumeApp > sceLncUtilResumeAppOffset)
	{
		return _sceLncUtilResumeApp(AppId, Flag);
	}
	else
	{
		klog("failed to resolve sceLncUtilResumeApp\n");
		return -1;
	}
}
