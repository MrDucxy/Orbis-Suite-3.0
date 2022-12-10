#include "Common.h"
#include "Version.h"
#include "API.h"
#include "GoldHEN.h"
#include <orbis/SystemService.h>
#include <orbis/SysCore.h>
#include <net/if.h>
#include <sys/ioctl.h>
#include <netinet/in.h>
#include <arpa/inet.h>

#include <orbis/Net.h>

#include "GeneralIPC.h"

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

	// Jailbreak our current process.
	if (!Jailbreak())
	{
		Notify("Failed to jailbreak Process...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Copy back up of sflash so we can read it and not break things :)
	CopySflash();

	// Set the Name of this process so it shows up as something other than eboot.bin.
	jbc_set_proc_name("OrbisLibAPI");

	klog("\n%s\n\n", ORBISLIB_BUILDSTRING);

// #define KILLTHEMALL
#ifdef KILLTHEMALL
	char Buffer[0x200];
	sprintf(Buffer, "/%s/common/lib/libSceSystemService.sprx", sceKernelGetFsSandboxRandomWord());
	int ModuleHandle = sceKernelLoadStartModule(Buffer, 0, nullptr, 0, nullptr, nullptr);
	if (ModuleHandle == 0) {
		klog("Failed to load libSceSystemService Library.\n");
		return false;
	}

	SceDbgModuleInfo minfos;
	sys_dynlib_get_info(ModuleHandle, &minfos);

	void(*sceLncUtilInitialize)() = (void(*)())((uint64_t)minfos.segmentInfo[0].baseAddr + 0x4BF0);
	int(*sceLncUtilGetAppId)(char* titleId) = (int(*)(char*))((uint64_t)minfos.segmentInfo[0].baseAddr + 0x4E10);

	sceLncUtilInitialize();
	sceSystemServiceKillApp(sceLncUtilGetAppId("NPXS20001"), -1, 0, 0);
#else
	/*int hndl = sys_sdk_proc_prx_load("SceShellUI", "/user/data/Orbis Suite/OrbisLibGeneralHelper.sprx");

	sceKernelSleep(2);

	ExtProccesInfoPacket info;
	if (GeneralIPC::GetExtProcessInfo("SceShellUI", &info))
	{
		klog("path = %s\n", info.Path);
	}*/

	sceNetInit();

	API::Init();
	
#endif

	while (true)
	{
		sceKernelSleep(1);
	}

	sceSystemServiceLoadExec("exit", 0);

    return 0;
}
