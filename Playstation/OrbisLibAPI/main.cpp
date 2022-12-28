#include "Common.h"
#include "Version.h"
#include "API.h"
#include "GoldHEN.h"
#include <orbis/SysCore.h>
#include "AppDatabase.h"

#include "GeneralIPC.h"

int main()
{
	// Jailbreak our current process.
	if (!Jailbreak())
	{
		Notify("Failed to jailbreak Process...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Load internal system modules.
	if (!LoadModules())
	{
		Notify("Failed to Load Modules...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Start up networking interface
	if (sceNetInit() != 0)
	{
		Notify("Failed to init Networking...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Start up user service.
	if (sceUserServiceInitialize(nullptr) != 0)
	{
		Notify("Failed to init User Service...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Init temporary wrapper for lncutils.
	if (LncUtil::Init() != 0)
	{
		Notify("Failed to init LncUtil...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Init temporary wrapper for shellcoreutils.
	if (ShellCoreUtil::Init() != 0)
	{
		Notify("Failed to init ShellCoreUtil...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// TODO: Bug? This doesnt seem to work for some reason.
	// Check GoldHEN SDK Version make sure we can run!
	auto sdkVersion = sys_sdk_version();
	if (sdkVersion < GOLDHEN_SDK_VERSION)
	{
		Notify("Invalid GoldHEN SDK Version %d Orbis Toolbox supports %d+", sdkVersion, GOLDHEN_SDK_VERSION);
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Copy back up of sflash so we can read it and not break things :)
	CopySflash();

	// Set the Name of this process so it shows up as something other than eboot.bin.
	sceKernelSetProcessName("OrbisLibAPI");

	klog("\n%s\n\n", ORBISLIB_BUILDSTRING);

	// Init a thread to monitor the system usage stats.
	// SystemMonitor::Init();

	// Init SysCoreUtils.
	sceApplicationInitialize();

	// Init App install utils.
	sceAppInstUtilInitialize();

//#define LOADTOOLBOX
#ifdef LOADTOOLBOX
	auto handle = sys_sdk_proc_prx_load("SceShellUI", "/user/data/Orbis Toolbox/OrbisToolbox-2.0.sprx");
	if (handle > 0) {
		klog("Orbis Toolbox loaded! %d\n", handle);
	}
	else
	{
		klog("error: %d\n", handle);
		Notify("Failed to load Orbis Toolbox!");
	}
#endif

	// start up the API. NOTE: this is blocking.
	API::Init();

	sceSystemServiceLoadExec("exit", 0);

    return 0;
}
