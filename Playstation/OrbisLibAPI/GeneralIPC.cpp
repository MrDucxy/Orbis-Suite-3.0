#include "Common.h"
#include "GeneralIPC.h"
#include <sys/un.h>

OrbisNetId GeneralIPC::Connect(int pid)
{
	char fullPath[0x200];
	snprintf(fullPath, sizeof(fullPath), GENERAL_IPC_ADDR, pid);

	OrbisNetSockaddrUn addr = { 0 };
	addr.sun_family = ORBIS_NET_AF_LOCAL;
	strncpy(addr.sun_path, fullPath, sizeof(addr.sun_path));

	// Make new local Socket
	auto Socket = sceNetSocket("GeneralIPC Socket", ORBIS_NET_AF_LOCAL, ORBIS_NET_SOCK_STREAM, 0);

	auto res = sceNetConnect(Socket, (OrbisNetSockaddr*)&addr, SUN_LEN(&addr));
	if (!res)
		return Socket;
	else
	{
		klog("Failed to connect to socket! Error: %llX\n", res);
		return NULL;
	}
}

bool GeneralIPC::SendCommand(OrbisNetId Sock, int Command)
{
	if (!Sockets::SendInt(Sock, Command))
	{
		klog("[GeneralIPC] Failed to send Command\n");
		return false;
	}

	int Status;
	if (!Sockets::RecvInt(Sock, &Status))
	{
		klog("[GeneralIPC] Failed to recv status\n");
		return false;
	}

	return Status == GIPC_OK;
}

bool GeneralIPC::GetLibraryList(int pid, std::vector<LibraryPacket>& Libraries)
{
	// Open a new local socket connection for the process.
	auto sock = Connect(pid);
	if (!sock)
	{
		klog("[GeneralIPC] Failed to connect to socket.\n");
		return false;
	}

	// Send the command.
	if (!SendCommand(sock, GIPC_LIB_LIST))
	{
		klog("[GeneralIPC] Failed to send command.\n");
		return false;
	}

	// Get the library count.
	int LibraryCount = 0;
	if (!Sockets::RecvInt(sock, &LibraryCount))
	{
		klog("[GeneralIPC] Failed to recv library count.\n");
		return false;
	}

	// Resize the vector to accept the data.
	Libraries.resize(LibraryCount);

	if (!Sockets::RecvLargeData(sock, (unsigned char*)Libraries.data(), LibraryCount * sizeof(LibPacket)))
	{
		klog("[GeneralIPC] Failed to recv library data.\n");
		return false;
	}

	return true;
}

bool GeneralIPC::LoadLibrary(int pid, const char* Path, int* HandleOut)
{
	// Open a new local socket connection for the process.
	auto sock = Connect(pid);
	if (!sock)
	{
		klog("[GeneralIPC] Failed to connect to socket.\n");
		return false;
	}

	// Send the command.
	if (!SendCommand(sock, GIPC_LIB_LOAD))
	{
		klog("[GeneralIPC] Failed to send command.\n");
		return false;
	}

	// Jailbreak the process.
	Jailbreak(pid);

	// Create next packet.
	auto Packet = (LibPacket*)malloc(sizeof(LibPacket));
	strcpy(Packet->Path, Path);

	// Send the packet.
	if (sceNetSend(sock, Packet, sizeof(LibPacket), 0) < 0)
	{
		klog("[GeneralIPC] Failed to send LibPacket.\n");

		// Restore the jail.
		Jail(pid);

		return false;
	}

	// Recieve the result.
	if (!Sockets::RecvInt(sock, HandleOut))
	{
		klog("[GeneralIPC] Failed to recv handle.\n");

		// Restore the jail.
		Jail(pid);

		return false;
	}

	// Check to see if it was loaded successfully.
	if (*HandleOut <= 0)
	{
		klog("[GeneralIPC] Failed to load PRX '%s' (0x%llX).\n", *HandleOut);

		// Restore the jail.
		Jail(pid);

		return false;
	}

	// Restore the jail.
	Jail(pid);

	return true;
}

bool GeneralIPC::UnLoadLibrary(int pid, int Handle)
{
	// Open a new local socket connection for the process.
	auto sock = Connect(pid);
	if (!sock)
	{
		klog("[GeneralIPC] Failed to connect to socket.\n");
		return false;
	}

	// Send the command.
	if (!SendCommand(sock, GIPC_LIB_UNLOAD))
	{
		klog("[GeneralIPC] Failed to send command.\n");
		return false;
	}

	// Recieve the result.
	int result = 0;
	if (!Sockets::RecvInt(sock, &result))
	{
		klog("[GeneralIPC] Failed to recv result.\n");
		return false;
	}

	// Check to see if it was unloaded successfully.
	if (result != 0)
	{
		klog("[GeneralIPC] Failed to un load PRX '%s' (0x%llX).\n", result);
		return false;
	}

	return true;
}

bool GeneralIPC::Jailbreak(int pid)
{
	// Open a new local socket connection for the process.
	auto sock = Connect(pid);
	if (!sock)
	{
		klog("[GeneralIPC] Failed to connect to socket.\n");
		return false;
	}

	// Send the command.
	if (!SendCommand(sock, GIPC_JAILBREAK))
	{
		klog("[GeneralIPC] Failed to send command.\n");
		return false;
	}

	// Recieve the result.
	int result = 0;
	if (!Sockets::RecvInt(sock, &result))
	{
		klog("[GeneralIPC] Failed to recv result.\n");
		return false;
	}

	return result == 1;
}

bool GeneralIPC::Jail(int pid)
{
	// Open a new local socket connection for the process.
	auto sock = Connect(pid);
	if (!sock)
	{
		klog("[GeneralIPC] Failed to connect to socket.\n");
		return false;
	}

	// Send the command.
	if (!SendCommand(sock, GIPC_JAIL))
	{
		klog("[GeneralIPC] Failed to send command.\n");
		return false;
	}

	// Recieve the result.
	int result = 0;
	if (!Sockets::RecvInt(sock, &result))
	{
		klog("[GeneralIPC] Failed to recv result.\n");
		return false;
	}

	return result == 1;
}