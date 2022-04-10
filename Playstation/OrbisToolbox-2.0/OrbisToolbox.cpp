#include "Common.h"
#include "Settings_Menu.h"
#include "System_Monitor.h"
#include "GamePad.h"
#include "API.h"

extern "C"
{
	int __cdecl module_start(size_t argc, const void* args)
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
		API::Init();

		Notify(ORBIS_TOOLBOX_NOTIFY);

		return 0;
	}

	int __cdecl module_stop(size_t argc, const void* args)
	{
		klog("!! BYE !!\n");

		// Toolbox
		Settings_Menu::Term();
		System_Monitor::Term();
		//Title_Menu::Term();

		// API
		API::Term();

		sceKernelSleep(4);

		return 0;
	}

	void _start()
	{

	}
}