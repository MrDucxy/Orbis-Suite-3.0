#include "Common.h"
#include "ShellUIIPC.h"
#include <sys/un.h>

OrbisNetId ShellUIIPC::Connect()
{
	OrbisNetSockaddrUn addr = { 0 };
	addr.sun_family = ORBIS_NET_AF_LOCAL;
	strncpy(addr.sun_path, SHELL_IPC_ADDR, sizeof(addr.sun_path));

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

bool ShellUIIPC::SendCommand(OrbisNetId Sock, int Command)
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

	return Status == 1;
}

void ShellUIIPC::RefreshContentArea()
{
	auto Sock = Connect();

	if (!Sock)
	{
		klog("[ShellUIIPC] Failed to connect to socket.\n");
		return;
	}

	if (!SendCommand(Sock, SIPC_REFRESH_CONTENT_AREA))
	{
		klog("[ShellUIIPC] Failed to send command.\n");
	}
}