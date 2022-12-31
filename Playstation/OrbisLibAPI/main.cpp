#include "Common.h"
#include "Version.h"
#include "API.h"
#include "GoldHEN.h"

#include "AppDatabase.h"

#include "GeneralIPC.h"

void exiting()
{
	klog("Good bye friends! :)\n");

	// Terminate System Monitor
	SystemMonitor::Term();

	// Terminate API
	API::Term();

	// Terminate Thread Pool

}

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

	// Copy back up of sflash so we can read it and not break things :)
	CopySflash();

	// Set the Name of this process so it shows up as something other than eboot.bin.
	sceKernelSetProcessName("OrbisLibAPI");

	// Log the loaded version string.
	klog("\n%s\n\n", ORBISLIB_BUILDSTRING);

	// Init a thread to monitor the system usage stats.
	//SystemMonitor::Init();

	// start up the API. NOTE: this is blocking.
	//API::Init();

	sceSystemServiceLoadExec("exit", 0);

    return 0;
}
