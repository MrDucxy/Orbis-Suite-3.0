#include "Common.h"
#include "Utilities.h"

#define KLOG_FMT "[OrbisHelper] %s"

void klog(const char* fmt, ...)
{
	char Buffer[0x200];
	char Buffer2[0x200];

	//Create full string from va list.
	va_list args;
	va_start(args, fmt);
	vsprintf(Buffer, fmt, args);
	va_end(args);

	sprintf(Buffer2, KLOG_FMT, Buffer);

	sceKernelDebugOutText(0, Buffer2);
}

bool SockSendInt(OrbisNetId Sock, int val)
{
	return !(sceNetSend(Sock, &val, sizeof(int), 0) < 0);
}

bool SockRecvInt(OrbisNetId Sock, int* val)
{
	return !(sceNetRecv(Sock, val, sizeof(int), 0) < 0);
}

int RecieveInt(OrbisNetId Sock)
{
	int Int = 0;
	if (sceNetRecv(Sock, &Int, sizeof(int), 0) < 0)
	{
		SockSendInt(Sock, GIPC_FAIL);
		return 0;
	}

	SockSendInt(Sock, GIPC_OK);
	return Int;
}