#include "Common.h"
#include "Version.h"
#include "API.h"
#include "GoldHEN.h"
#include <orbis/SystemService.h>
#include <orbis/SysCore.h>

#define CTL_KERN 1
#define KERN_PROC 14
#define KERN_PROC_PID 1
#define KERN_PROC_APPINFO 35

void hexdump(void* ptr, int buflen) {
	unsigned char* buf = (unsigned char*)ptr;
	int i, j;
	for (i = 0; i < buflen; i += 16) {
		klog("%06x: ", i);
		for (j = 0; j < 16; j++)
			if (i + j < buflen)
				klog("%02x ", buf[i + j]);
			else
				klog("   ");
		klog(" ");
		for (j = 0; j < 16; j++)
			if (i + j < buflen)
				klog("%c", isprint(buf[i + j]) ? buf[i + j] : '.');
		klog("\n");
	}
}

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

	// Use GoldHEN JB.
	jailbreak_backup jb1;
	sys_sdk_jailbreak(&jb1);

	// Set the Name of this process so it shows up as something other than eboot.bin.
	jbc_set_proc_name("OrbisLibAPI");

	klog("\n%s\n\n", ORBISLIB_BUILDSTRING);

	API::Init();

	while (true)
	{
		sceKernelSleep(1);
	}

	klog("Time to go :(\n");

	sceSystemServiceLoadExec("exit", 0);

    return 0;
}
