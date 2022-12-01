#include "Common.h"
#include "GoldHEN.h"

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

	// Set RW on the system directory. Seems to sus up some shit needs investigating...
	mount_large_fs("/dev/da0x4.crypt", "/system", "exfatfs", "511", MNT_UPDATE);

	// Install all the things! :D
	InstallDaemon("ORBS30000"); // Orbis Lib
	InstallOrbisToolbox();

	//TODO: Pull module list of SceShellUI to see if we should unload the toolbox.

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