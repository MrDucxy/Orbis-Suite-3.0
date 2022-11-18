#include "Common.h"

#define DEBUG_BUILD

int main()
{
	// Load internal system modules.
	if (!LoadModules())
	{
		Notify("Failed to Load Modules...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Check GoldHEN SDK Version make sure we can run!

	// Use future GoldHEN JB

	//MakeDirs
	MakeDir("/data/Orbis Suite");

	// Set RW on the system directory. Seems to sus up some shit needs investigating...
	mount_large_fs("/dev/da0x4.crypt", "/system", "exfatfs", "511", MNT_UPDATE);

	// Install all the things! :D
	//InstallDaemon("ORBS30000"); // Orbis Lib
	InstallOrbisToolbox();

	// Pull module list of SceShellUI to see if we should unload the toolbox.

	// Use GoldHEN SDK to load PRX to SceShellUI.
	/*
	
	auto Handle = OrbisDriver::LoadSPRX("SceShellUI", "/data/Orbis Toolbox/OrbisToolbox-2.0.sprx");
		if (Handle > 0)
			klog("Orbis Toolbox loaded! %d\n", Handle);

	*/

	sceSystemServiceLoadExec("exit", 0);

	return 0;
}