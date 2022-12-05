#pragma once

// Misc
void klog(const char* fmt, ...);

// Networking
bool SockSendInt(OrbisNetId Sock, int val);
bool SockRecvInt(OrbisNetId Sock, int* val);
int RecieveInt(OrbisNetId Sock);

template<class T> T* RecievePacket(OrbisNetId Sock)
{
	auto Packet = (T*)malloc(sizeof(T));

	if (sceNetRecv(Sock, Packet, sizeof(T), 0) < 0)
	{
		SendStatus(Sock, GIPC_FAIL);
		free(Packet);
		return nullptr;
	}

	SendStatus(Sock, GIPC_OK);

	return Packet;
}