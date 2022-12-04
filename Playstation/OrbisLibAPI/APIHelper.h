#pragma once
#include "APIPackets.h"

void SendStatus(OrbisNetId Sock, int status);
int RecieveInt(OrbisNetId Sock);

template<class T> T* RecievePacket(OrbisNetId Sock)
{
	auto Packet = (T*)malloc(sizeof(T));

	if (sceNetRecv(Sock, Packet, sizeof(T), 0) < 0)
	{
		SendStatus(Sock, APIResults::API_ERROR_GENERAL);
		free(Packet);
		return nullptr;
	}

	SendStatus(Sock, APIResults::API_OK);

	return Packet;
}