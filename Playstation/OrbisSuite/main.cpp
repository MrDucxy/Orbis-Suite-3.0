#include "Common.h"
#include "GoldHEN.h"
#include <orbis/LncUtil.h>
#include <orbis/SystemService.h>
#include <orbis/UserService.h>

#define DEBUG

int main()
{
	// Load internal system modules.
	if (!LoadModules())
	{
		Notify("Failed to Load Modules...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	//TODO: Bug? This doesnt seem to work for some reason.
	// Check GoldHEN SDK Version make sure we can run!
	auto sdkVersion = sys_sdk_version();
	if (sdkVersion < GOLDHEN_SDK_VERSION)
	{
		Notify("Invalid GoldHEN SDK Version %d Orbis Toolbox supports %d+", sdkVersion, GOLDHEN_SDK_VERSION);
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Use future GoldHEN JB
	jailbreak_backup jb1;
	sys_sdk_jailbreak(&jb1);

	//MakeDirs
	MakeDir("/data/Orbis Suite");

	// Set RW on the system directory.
	mount_large_fs("/dev/da0x4.crypt", "/system", "exfatfs", "511", MNT_UPDATE);

	// Install all the things! :D
	InstallDaemon("ORBS30000"); // Orbis Lib
	InstallOrbisToolbox();
	InstallOrbisSuite();

	// Start the API.
	auto appId = sceLncUtilGetAppId("ORBS30000");
	if (appId)
	{
#ifdef DEBUG
		sceLncUtilKillApp(appId);
#else
		sceSystemServiceLoadExec("exit", 0);
		return 0;
#endif
	}

	LaunchAppParam appParam;
	appParam.size = sizeof(LaunchAppParam);
	sceUserServiceGetForegroundUser(&appParam.userId);
	appParam.enableCrashReport = 0;
	appParam.checkFlag = 0;
	appParam.appAttr = 0;

	auto res = sceLncUtilLaunchApp("ORBS30000", nullptr, &appParam);

#ifdef DEBUG
	if (res != 0)
	{
		klog("Failed to load Daemon.\n");
	}
#endif // DEBUG

	//TODO: use IPC to see if prx is already loaded.
	// Use GoldHEN SDK to load PRX to SceShellUI.
	auto Handle = sys_sdk_proc_prx_load("SceShellUI", "/user/data/Orbis Toolbox/OrbisToolbox-2.0.sprx");

	if (Handle > 0)
		klog("Orbis Toolbox loaded! %d\n", Handle);
	else
	{
		klog("error: %d\n", Handle);
		Notify("Failed to load Orbis Toolbox!");
	}
	
	sceSystemServiceLoadExec("exit", 0);

	return 0;
}