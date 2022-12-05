#include "Common.h"
#include "GeneralIPC.h"
#include <sys/un.h>

OrbisNetId GeneralIPC::Connect(const char* ProcessName)
{
	char fullPath[0x200];
	snprintf(fullPath, sizeof(fullPath), GENERAL_IPC_ADDR, ProcessName);

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
	if (!SockSendInt(Sock, Command))
	{
		klog("[GeneralIPC] Failed to send Command\n");
		return false;
	}

	int Status;
	if (!SockRecvInt(Sock, &Status))
	{
		klog("[GeneralIPC] Failed to recv status\n");
		return false;
	}

	return Status == GIPC_OK;
}

bool GeneralIPC::GetExtProcessInfo(const char* ProcessName, ExtProccesInfoPacket* info)
{
	auto Sock = Connect(ProcessName);

	if (!Sock)
	{
		klog("[GeneralIPC] Failed to connect to socket.\n");
		return false;
	}

	if (!SendCommand(Sock, GIPC_INFO))
	{
		klog("[GeneralIPC] Failed to send command.\n");
		return false;
	}

	return sceNetRecv(Sock, (void*)info, sizeof(ExtProccesInfoPacket), 0) == 0;
}