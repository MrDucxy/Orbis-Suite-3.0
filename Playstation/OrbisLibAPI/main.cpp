#include "Common.h"
#include "Version.h"

int main()
{
	// Load internal system modules.
	if (!LoadModules())
	{
		Notify("Failed to Load Modules...");
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

	// Use future GoldHEN JB.
	jailbreak_backup jb1;
	sys_sdk_jailbreak(&jb1);

	// Set the Name of this process so it shows up as something other than eboot.bin.
	jbc_set_proc_name("OrbisLibAPI");

	klog(ORBISLIB_BUILDSTRING);

	while (true)
	{
		sceKernelSleep(1);
	}

	sceSystemServiceLoadExec("exit", 0);

    return 0;
}
