#include "stdafx.h"


void exit()
{

	// Terminate Thread Pool
	ThreadPool::Term();
}

int main(int argc, char** arg)
{
	// Jailbreak our current process.
	//if (!Jailbreak())
	//{
	//	Notify("Failed to jailbreak Process...");
	//	sceSystemServiceLoadExec("exit", 0);
	//	return 0;
	//}

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
	//sceKernelSetProcessName("OrbisLibAPI");

	// Log the loaded version string.
	klog("\n%s\n\n", ORBISLIB_BUILDSTRING);

	// Start up the thread pool.
	ThreadPool::Init(10);

	// Init a thread to monitor the system usage stats.
	// SystemMonitor::Init();

	// Start up the API.
	//if (!API::Init())
	//{
	//	Notify("Failed to init API...");
	//	sceSystemServiceLoadExec("exit", 0);
	//	return 0;
	//}

	// Blocking run monitor.
	// Monitor::Run();

	atexit(exit);

	return 0;
}