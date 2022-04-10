#include "Common.h"
#include "APIHelper.h"

void SendStatus(OrbisNetId Sock, int status)
{
	sceNetSend(Sock, &status, sizeof(int), 0);
}

int RecieveInt(OrbisNetId Sock)
{
	int Int = 0;
	if (sceNetRecv(Sock, &Int, sizeof(int), 0) < 0)
	{
		SendStatus(Sock, APIResults::API_ERROR_GENERAL);
		return 0;
	}

	SendStatus(Sock, APIResults::API_OK);
	return Int;
}
