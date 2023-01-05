#include "Common.h"
#include "Sockets.h"

bool Sockets::SendInt(OrbisNetId Sock, int val)
{
	auto res = sceNetSend(Sock, &val, sizeof(int), 0);
	if (res <= 0)
	{
		klog("SendInt(): Failed to send %llX\n", res);

		return false;
	}

	return true;
}

bool Sockets::RecvInt(OrbisNetId Sock, int* val)
{
	auto res = sceNetRecv(Sock, val, sizeof(int), 0);
	if (res <= 0)
	{
		klog("RecvInt(): Failed to send %llX\n", res);

		return false;
	}

	return true;
}

bool Sockets::SendLargeData(OrbisNetId Sock, unsigned char* data, size_t dataLen)
{
	unsigned char* CurrentPosition = data;
	size_t DataLeft = dataLen;
	int res = 0;

	while (DataLeft > 0)
	{
		size_t DataChunkSize = std::min((size_t)8192, DataLeft);

		res = sceNetSend(Sock, CurrentPosition, DataChunkSize, 0);

		if (res < 0)
		{
			klog("SendLargeData() Error: %llX\n", res);
			return false;
		}

		DataLeft -= res;
		CurrentPosition += res;
	}

	return true;
}

bool Sockets::RecvLargeData(OrbisNetId Sock, unsigned char* data, size_t dataLen)
{
	size_t DataLeft = dataLen;
	int Received = 0;
	int res = 0;

	while (DataLeft > 0)
	{
		size_t DataChunkSize = std::min((size_t)8192, DataLeft);
		res = sceNetRecv(Sock, data + Received, DataChunkSize, 0);

		if (res < 0)
		{
			return false;
		}

		Received += res;
		DataLeft -= res;
	}

	return true;
}