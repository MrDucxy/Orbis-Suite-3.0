#include "Common.h"
#include "Detour.h"

LocalSocketListener* LocalListener = nullptr;

void SendLibraryList(OrbisNetId Sock)
{
	int MaxOutput = 0;
	if (!SockRecvInt(Sock, &MaxOutput))
	{
		klog("Failed to recv int.\n");
		return;
	}

	// Get the libraries.
	LibraryInfo* LibraryList = (LibraryInfo*)malloc(MaxOutput * sizeof(LibraryInfo));
	int RealLibCount = jbc_get_proc_libraries(LibraryList, MaxOutput);

	// Make sure not to overflow the sender.
	if (RealLibCount > MaxOutput)
	{
		klog("Warning: SendLibraryList MaxOutput too small.");
		RealLibCount = MaxOutput;
	}

	// Send the Count
	SockSendInt(Sock, RealLibCount);

	// Ship it.
	sceNetSend(Sock, (void*)LibraryList, RealLibCount * sizeof(LibraryInfo), 0);

	// Clean up!
	free(LibraryList);
}

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

		case GIPC_LIB_LIST:
			SendLibraryList(Sock); // Really Only needed for the path.
			break;

		case GIPC_JAILBREAK:
			//sys_sdk_jailbreak(&JailBackup); // Could just use libjbc
			SockSendInt(Sock, GIPC_OK);
			break;

		case GIPC_JAIL:
			//sys_sdk_unjailbreak(&JailBackup); // Could just use libjbc
			SockSendInt(Sock, GIPC_OK);
			break;

		case  GIPC_RW:
			// Might not really need this.
			break;

		case GIPC_PROT:
			// Might not really need this either? Depends on if we can set the protection from the debug proc.
			break;
		}
	}
}

extern "C"
{
	int __cdecl module_start(size_t argc, const void* args)
	{
		klog("Hello from Helper!\n");

		char ProcName[0x20];
		sceKernelGetProcessName(getpid(), ProcName);

		klog("Helping with %s\n", ProcName);

		char serverAddress[0x200];
		snprintf(serverAddress, sizeof(serverAddress), GENERAL_IPC_ADDR, ProcName);

		LocalListener = new LocalSocketListener(ListenerClientThread, nullptr, serverAddress);

		return 0;
	}

	int __cdecl module_stop(size_t argc, const void* args)
	{
		delete LocalListener;

		return 0;
	}
}