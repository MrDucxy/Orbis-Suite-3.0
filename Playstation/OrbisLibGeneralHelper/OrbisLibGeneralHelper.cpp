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
	auto Packet = (PRXPacket*)malloc(sizeof(PRXPacket));
	sceNetRecv(Sock, Packet, sizeof(PRXPacket), 0);

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

void ReadWriteMemory(OrbisNetId Sock)
{
	int res = 0;

	// Get the packet for the info.
	auto Packet = (RWPacket*)malloc(sizeof(RWPacket));
	if (sceNetRecv(Sock, Packet, sizeof(RWPacket), 0) < 0)
	{
		klog("ReadWriteMemory(): Failed to recieve packet\n");

		free(Packet);

		return;
	}

	// Make sure the address is valid.
	OrbisKernelModuleInfo info;
	if (Packet->Address == 0 || !GetModuleFromAddress(Packet->Address, &info) && !IsMemoryMapped(Packet->Address))
	{
		Sockets::SendInt(Sock, 0);
	 
		klog("ReadWriteMemory(): Failed to validate address.\n");
	
		free(Packet);
	
		return;
	}

	// Get the current protection of the memory.
	int prot;
	res = sceKernelQueryMemoryProtection((void*)Packet->Address, nullptr, nullptr, &prot);
	if (res != 0)
	{
		Sockets::SendInt(Sock, 0);

		klog("ReadWriteMemory(): Failed to get memory prot: %llX\n", res);
	
		free(Packet);
	
		return;
	}

	// Set the protection level so we can RW.
	res = sceKernelMprotect((void* )Packet->Address, Packet->Length, VM_PROT_ALL);
	if (res != 0)
	{
		Sockets::SendInt(Sock, 0);

		klog("ReadWriteMemory(): Failed to set memory prot: %llX\n", res);

		free(Packet);

		return;
	}

	// Let the client know we have validated the memory.
	Sockets::SendInt(Sock, 1);

	// Read / Write the memory.
	if (Packet->Write)
	{
		auto buffer = (unsigned char*)malloc(Packet->Length);

		if (!Sockets::RecvLargeData(Sock, buffer, Packet->Length))
		{
			klog("ReadWriteMemory(): Failed to recieve the data.\n");

			free(buffer);

			goto Exit;
		}

		memcpy((void*)Packet->Address, buffer, Packet->Length);

		free(buffer);
	}
	else
	{
		auto buffer = (unsigned char*)malloc(Packet->Length);
		memcpy(buffer, (void*)Packet->Address, Packet->Length);

		if (!Sockets::SendLargeData(Sock, buffer, Packet->Length))
		{
			klog("ReadWriteMemory(): Failed to send memory.\n");
		}

		free(buffer);
	}
	
Exit:
	// Reset the protection back.
	res = sceKernelMprotect((void*)Packet->Address, Packet->Length, prot);
	if (res != 0)
	{
		klog("ReadMemory(): Failed to restore memory prot: %llX\n", res);
	}

	free(Packet);

	// success!
	Sockets::SendInt(Sock, 1);
}

void ListenerClientThread(void* tdParam, OrbisNetId Sock)
{
	int Command = 0;

	if (!Sockets::RecvInt(Sock, &Command))
	{
		klog("Failed to recv Command.\n");
		return;
	}

	if (!Sockets::SendInt(Sock, 1))
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

			Sockets::SendInt(Sock, 1);

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

			Sockets::SendInt(Sock, 1);

			break;

		case GIPC_JAIL:

			RestoreJail();
			
			Sockets::SendInt(Sock, 1);

			break;

		case GIPC_RW:

			ReadWriteMemory(Sock);

			break;

		case GIPC_PROT:
			
			break;
		}
	}
}

extern "C"
{
	int __cdecl module_start(size_t argc, const void* args)
	{
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