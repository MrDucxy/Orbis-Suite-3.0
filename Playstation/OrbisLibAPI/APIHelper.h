#pragma once
#include "APIPackets.h"

void SendStatus(OrbisNetId Sock, int status);
int RecieveInt(OrbisNetId Sock);

template<class T> std::shared_ptr<T> RecievePacket(OrbisNetId Sock)
{
	auto packet = std::make_shared<T>();

	if (sceNetRecv(Sock, packet.get(), sizeof(T), 0) < 0)
	{
		SendStatus(Sock, APIResults::API_ERROR_GENERAL);
		return nullptr;
	}

	SendStatus(Sock, APIResults::API_OK);

	return packet;
}