#include "Common.h"
#include "OrbisDriver.h"

int main()
{
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

	if (OrbisDriver::TestDriver())
	{
		OrbisDriver::UnLoadSPRX("SceShellUI", "OrbisToolbox-2.0.sprx");
		sceKernelSleep(1);
		int handle = OrbisDriver::LoadSPRX("SceShellUI", "/data/Orbis Toolbox/OrbisToolbox-2.0.sprx");
		if (handle < 0)
		{
			Notify("Orbis Toolbox failed to load! :( %d\n", handle);
			klog("Orbis Toolbox failed to load! :( %d\n", handle);
		}
	}
	else
	{
		Notify("Could not Load: Kernel Driver not running!!");
		klog("Could not Load: Kernel Driver not running!!\n");
	}

	sceSystemServiceLoadExec("exit", 0);

	return 0;
}