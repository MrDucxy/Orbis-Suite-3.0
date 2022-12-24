#include "Common.h"
#include "Settings_Menu.h"
#include "System_Monitor.h"
#include "GamePad.h"
#include "RemoteCaller.h"
#include "LocalSocketListener.h"
#include "../../Misc/ShellUI_IPC.h"

RemoteCaller* CallInMonoThread = nullptr;
LocalSocketListener* LocalListener = nullptr;

void ListenerClientThread(void* tdParam, OrbisNetId Sock)
{
	int Command = RecieveInt(Sock);
	if (Command != -1)
	{
		switch (Command)
		{
		default:
			klog("Invalid Command enum %i\n", Command);
			break;

		case SIPC_HANDLE:

			break;

		case SIPC_REFRESH_CONTENT_AREA:

			CallInMonoThread->RemoteCall(UI::Utilities::ReloadItemList);

			break;

		case SIPC_SETTINGS_RW:

			break;
		}
	}
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
	LocalListener = new LocalSocketListener(ListenerClientThread, nullptr, SHELL_IPC_ADDR);

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
		//Title_Menu::Term();

		// API
		delete CallInMonoThread;
		delete LocalListener;

		sceKernelSleep(4);

		return 0;
	}
}