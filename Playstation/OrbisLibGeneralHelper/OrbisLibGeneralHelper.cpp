#include "Common.h"
#include "Detour.h"

LocalSocketListener* LocalListener = nullptr;

void SendLibraryList(OrbisNetId Sock)
{
	std::vector<LibPacket> libraryList;

	// Get the libraries.
	LibraryInfo* klibraryList = (LibraryInfo*)malloc(200 * sizeof(LibraryInfo));
	int realLibCount = jbc_get_proc_libraries(klibraryList, 200);

	// Send the Count
	Sockets::SendInt(Sock, realLibCount);

	for (int i = 0; i < realLibCount; i++)
	{
		OrbisKernelModuleInfo moduleInfo;
		moduleInfo.size = sizeof(OrbisKernelModuleInfo);
		int res = sceKernelGetModuleInfo(klibraryList[i].Handle, &moduleInfo);

		LibPacket temp;
		temp.Handle = klibraryList[i].Handle;
		strcpy(temp.Path, klibraryList[i].Path);
		temp.SegmentCount = moduleInfo.segmentCount;
		memcpy(&temp.Segments[0], &moduleInfo.segmentInfo[0], sizeof(OrbisKernelModuleSegmentInfo) * 4);

		libraryList.push_back(temp);
	}

	// Ship it.
	Sockets::SendLargeData(Sock, (unsigned char*)libraryList.data(), realLibCount * sizeof(LibPacket));

	// Clean up!
	free(klibraryList);
}

void LoadUnloadLib(int Command, OrbisNetId Sock)
{
	auto Packet = (LibPacket*)malloc(sizeof(LibPacket));
	sceNetRecv(Sock, Packet, sizeof(LibPacket), 0);

	if (Command == GIPC_LIB_LOAD)
	{
		int res = sceKernelLoadStartModule(Packet->Path, 0, 0, 0, 0, 0);

		// Send the result.
		Sockets::SendInt(Sock, res);
	}
	else
	{
		int res = sceKernelStopUnloadModule(Packet->Handle, 0, 0, 0, 0, 0);

		// Send the result.
		Sockets::SendInt(Sock, res);
	}

	free(Packet);
}

void ListenerClientThread(void* tdParam, OrbisNetId Sock)
{
	int Command = 0;

	if (!Sockets::RecvInt(Sock, &Command))
	{
		klog("Failed to recv Command.\n");
		return;
	}

	if (!Sockets::SendInt(Sock, GIPC_OK))
	{
		klog("Failed to send confirm command.\n");
		return;
	}

	if (Command != -1)
	{
		switch (Command)
		{
		default:
			klog("Invalid Command enum %i\n", Command);
			break;

		case GIPC_HELLO:

			Sockets::SendInt(Sock, GIPC_OK);

			break;

		case GIPC_LIB_LIST:
			SendLibraryList(Sock); // Really Only needed for the path.
			break;

		case GIPC_LIB_LOAD:
		case GIPC_LIB_UNLOAD:

			LoadUnloadLib(Command, Sock);

			break;

		case GIPC_JAILBREAK:

			Jailbreak();

			Sockets::SendInt(Sock, GIPC_OK);

			break;

		case GIPC_JAIL:

			RestoreJail();
			
			Sockets::SendInt(Sock, GIPC_OK);

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

		klog("Helping with %d\n", getpid());

		char serverAddress[0x200];
		snprintf(serverAddress, sizeof(serverAddress), GENERAL_IPC_ADDR, getpid());

		LocalListener = new LocalSocketListener(ListenerClientThread, nullptr, serverAddress);

		klog("Helper Init Complete.\n");

		return 0;
	}

	int __cdecl module_stop(size_t argc, const void* args)
	{
		delete LocalListener;

		return 0;
	}
}