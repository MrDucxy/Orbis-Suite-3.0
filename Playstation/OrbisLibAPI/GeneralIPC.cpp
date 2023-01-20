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
	if (Socket < 0)
	{
		klog("Failed to get socket: %llX\n", Socket);
		return NULL;
	}

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

bool GeneralIPC::TestConnection(int pid)
{
	// Open a new local socket connection for the process.
	auto sock = Connect(pid);
	if (!sock)
	{
		klog("[GeneralIPC] Failed to connect to socket.\n");
		return false;
	}

	// Send the command.
	if (!SendCommand(sock, GIPC_HELLO))
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to send command.\n");
		return false;
	}

	// Get the library count.
	int status = 0;
	if (!Sockets::RecvInt(sock, &status))
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to recv status.\n");
		return false;
	}

	// Close the socket.
	sceNetSocketClose(sock);

	return status == GIPC_OK;
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
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to send command.\n");
		return false;
	}

	// Get the library count.
	int LibraryCount = 0;
	if (!Sockets::RecvInt(sock, &LibraryCount))
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to recv library count.\n");
		return false;
	}

	// Resize the vector to accept the data.
	Libraries.resize(LibraryCount);

	if (!Sockets::RecvLargeData(sock, (unsigned char*)Libraries.data(), LibraryCount * sizeof(LibPacket)))
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to recv library data.\n");
		return false;
	}

	// Close the socket.
	sceNetSocketClose(sock);

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
		// Close the socket.
		sceNetSocketClose(sock);

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
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to send LibPacket.\n");

		// Restore the jail.
		Jail(pid);

		// Cleanup
		free(Packet);

		return false;
	}

	// Cleanup
	free(Packet);

	// Recieve the result.
	if (!Sockets::RecvInt(sock, HandleOut))
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to recv handle.\n");

		// Restore the jail.
		Jail(pid);

		return false;
	}

	// Check to see if it was loaded successfully.
	if (*HandleOut <= 0)
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to load PRX '%s' (0x%llX).\n", *HandleOut);

		// Restore the jail.
		Jail(pid);

		return false;
	}

	// Restore the jail.
	Jail(pid);

	// Close the socket.
	sceNetSocketClose(sock);

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
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to send command.\n");
		return false;
	}

	// Create next packet.
	auto Packet = (LibPacket*)malloc(sizeof(LibPacket));
	Packet->Handle = Handle;

	// Send the packet.
	if (sceNetSend(sock, Packet, sizeof(LibPacket), 0) < 0)
	{
		// Close the socket.
		sceNetSocketClose(sock);

		// Cleanup
		free(Packet);

		klog("[GeneralIPC] Failed to send LibPacket.\n");

		return false;
	}

	// Cleanup
	free(Packet);

	// Recieve the result.
	int result = 0;
	if (!Sockets::RecvInt(sock, &result))
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to recv result.\n");
		return false;
	}

	// Check to see if it was unloaded successfully.
	if (result != 0)
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to un load PRX '%s' (0x%llX).\n", result);
		return false;
	}

	// Close the socket.
	sceNetSocketClose(sock);

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
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to send command.\n");
		return false;
	}

	// Recieve the result.
	int result = 0;
	if (!Sockets::RecvInt(sock, &result))
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to recv result.\n");
		return false;
	}

	// Close the socket.
	sceNetSocketClose(sock);

	return result == GIPC_OK;
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
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to send command.\n");
		return false;
	}

	// Recieve the result.
	int result = 0;
	if (!Sockets::RecvInt(sock, &result))
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to recv result.\n");
		return false;
	}

	// Close the socket.
	sceNetSocketClose(sock);

	return result == GIPC_OK;
}

bool GeneralIPC::ReadWriteMemory(int pid, uint64_t address, unsigned char* data, size_t length, bool write)
{
	// Open a new local socket connection for the process.
	auto sock = Connect(pid);
	if (!sock)
	{
		klog("[GeneralIPC] ReadWriteMemory(): Failed to connect to socket.\n");
		return false;
	}

	// Send the command.
	if (!SendCommand(sock, GIPC_RW))
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] ReadWriteMemory(): Failed to send command.\n");
		return false;
	}

	// Create next packet.
	auto Packet = (RWPacket*)malloc(sizeof(RWPacket));
	Packet->Address = address;
	Packet->Length = length;
	Packet->Write = write;

	// Send the packet.
	if (sceNetSend(sock, Packet, sizeof(RWPacket), 0) < 0)
	{
		// Close the socket.
		sceNetSocketClose(sock);

		// Cleanup
		free(Packet);

		klog("[GeneralIPC] ReadWriteMemory(): Failed to send RWPacket.\n");

		return false;
	}

	// Cleanup
	free(Packet);

	// Make sure the address was valid.
	int result = 0;
	if (!Sockets::RecvInt(sock, &result))
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to recv result.\n");
		return false;
	}

	if (result == 0)
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] ReadWriteMemory():Invalid Address %llX\n", address);
		return false;
	}

	// Recieve/Send Data
	if (write)
	{
		if (!Sockets::SendLargeData(sock, data, length))
		{
			klog("[GeneralIPC] ReadWriteMemory(): Failed to send the data.");

			return false;
		}
	}
	else
	{
		if (!Sockets::RecvLargeData(sock, data, length))
		{
			klog("[GeneralIPC] ReadWriteMemory(): Failed to recv the data.");

			return false;
		}
	}

	// Recieve the result.
	result = 0;
	if (!Sockets::RecvInt(sock, &result))
	{
		// Close the socket.
		sceNetSocketClose(sock);

		klog("[GeneralIPC] Failed to recv result.\n");
		return false;
	}

	// Close the socket.
	sceNetSocketClose(sock);

	return result == GIPC_OK;
}