#include "Common.h"
#include "Sockets.h"

bool Sockets::SendInt(OrbisNetId Sock, int val)
{
	return !(sceNetSend(Sock, &val, sizeof(int), 0) < 0);
}

bool Sockets::RecvInt(OrbisNetId Sock, int* val)
{
	return !(sceNetRecv(Sock, val, sizeof(int), 0) < 0);
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
			return false;
		}

		DataLeft -= res;
		CurrentPosition += res;
	}

	return true;
}