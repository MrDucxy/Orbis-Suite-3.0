#include "Common.h"
#include "Settings_Menu.h"
#include "System_Monitor.h"
#include "GamePad.h"
#include "RemoteCaller.h"

RemoteCaller* CallInMonoThread = NULL;

void* InitThread(void* args)
{
	klog("!! Hello World !!\n");

	Mono::Init();

	if (GamePad::IsDown(GamePad::Buttons::Left | GamePad::Buttons::Triangle))
	{
		Notify("Orbis Toolbox: Aborting Launch!!");
		return 0;
	}

	// Toolbox
	System_Monitor::Init();
	Settings_Menu::Init();
	//Title_Menu::Init();	

	// API
	CallInMonoThread = new RemoteCaller();

	Notify(ORBIS_TOOLBOX_NOTIFY);
	
	scePthreadExit(NULL);
	return 0;
}

extern "C"
{
	int __cdecl module_start(size_t argc, const void* args)
	{
		OrbisPthread* hThread;
		scePthreadCreate(&hThread, nullptr, InitThread, nullptr, "Init");

		return 0;
	}

	int __cdecl module_stop(size_t argc, const void* args)
	{
		klog("!! BYE !!\n");

		// Toolbox
		Settings_Menu::Term();
		System_Monitor::Term();
		//Title_Menu::Term(); 0x83D1E4A78

		// API
		delete CallInMonoThread;

		sceKernelSleep(4);

		return 0;
	}
}