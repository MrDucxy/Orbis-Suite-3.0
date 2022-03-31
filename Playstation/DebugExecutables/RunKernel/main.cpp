#include "Common.h"
#include "KernelLoader.h"
#include <sys/ioctl.h>
#include "../../../Misc/DriverDefinitions.h"
#include "OrbisDriver.h"

int main()
{
	int SoftwareVersion = 0;
	int Handle = 0;

	// Load internal system modules.
	if (!LoadModules())
	{
		Notify("Failed to Load Modules...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Break Free
	if (!Jailbreak())
	{
		Notify("Jailbreak failed...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Get Firmware Version
	SoftwareVersion = GetUpdateVersion();

	// Set the Kernel Version.
	SetKernelVersion(SoftwareVersion);

	// Init Kernel driver if not present.
	if (!OrbisDriver::TestDriver())
	{
		klog("[Orbis Suite] Launching Kernel!!\n");
		char KernelPath[0x100];
		sprintf(KernelPath, "/data/Orbis Suite/Kernel.%03X.ELF", (SoftwareVersion >> 16));
		if (!LoadKernel(KernelPath, SoftwareVersion))
		{
			Notify("Failed to load Kernel ELF...");
			klog("Failed to load Kernel ELF...\n");
		}
		else
		{
			Notify("Loaded Kernel ELF Successfully!");
			klog("Loaded Kernel ELF Successfully!\n");
		}
	}
	else
	{
		if (!UnloadKernel())
		{
			Notify("Failed to stop Kernel ELF...");
			klog("Failed to stop Kernel ELF...\n");
		}
		else
		{
			Notify("Un-Loaded Kernel ELF Successfully!");
			klog("Un-Loaded Kernel ELF Successfully!\n");
		}
	}

	while (true) sceKernelSleep(1);

	sceSystemServiceLoadExec("exit", 0);

	return 0;
}