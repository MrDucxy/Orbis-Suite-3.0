#include "Common.h"
#include "Settings_Menu.h"
#include "System_Monitor.h"
#include "GamePad.h"
#include "RemoteCaller.h"

RemoteCaller* CallInMonoThread = NULL;

Detour* SearchJob_RunDetour;
void* SearchJob_RunHook(MonoObject* instance)
{
	auto IsCancelled = Mono::Get_Property<bool>(Mono::UI_dll, "Sce.PlayStation.HighLevel.UI2", "Job", instance, "IsCancelled");

	if (!IsCancelled)
	{
		auto SearchJob = Mono::Get_Class(Mono::App_exe, "Sce.Vsh.ShellUI.Settings.PkgInstaller", "SearchJob");
		Mono::Invoke<void>(Mono::App_exe, SearchJob, instance, "SearchDir", Mono::New_String("/user/data/pkg"), Mono::New_String("/user/data/pkg"));
	}

	return SearchJob_RunDetour->Stub<void*>(instance);
}

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

	SearchJob_RunDetour = new Detour();
	SearchJob_RunDetour->DetourMethod(Mono::App_exe, "Sce.Vsh.ShellUI.Settings.PkgInstaller", "SearchJob", "Run", 0, (void*)SearchJob_RunHook);

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
		//Title_Menu::Term();

		// API
		delete CallInMonoThread;

		delete SearchJob_RunDetour;

		sceKernelSleep(4);

		return 0;
	}
}