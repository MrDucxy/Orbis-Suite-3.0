#include "Common.h"
#include "Apps.h"
#include <orbis/SysCore.h>

void Apps::HandleAPI(OrbisNetId Sock, APIPacket* Packet)
{
	// Get the titleId of the App
	char titleId[9];
	sceNetRecv(Sock, titleId, sizeof(titleId), 0);

	switch (Packet->Command)
	{
	default:
		break;

	case API_APPS_STATUS:

		SendAppStatus(Sock, titleId);

		break;
	}
}

void Apps::SendAppStatus(OrbisNetId Sock, const char* TitleId)
{
	if (TitleId != nullptr && strlen(TitleId) != 9)
	{
		SockSendInt(Sock, -1);
		return;
	}

	size_t length;
	int count;
	kinfo_proc* info;
	int appId = 0;

	static int name[] = { CTL_KERN, KERN_PROC, KERN_PROC_ALL, 0 };

	if (sysctl(name, 3, nullptr, &length, nullptr, NULL) < 0)
	{
		klog("first sysctl failed.\n");

		SockSendInt(Sock, -1);
		return;
	}

	if (!(info = (kinfo_proc*)malloc(length)))
	{
		klog("malloc failed.\n");

		SockSendInt(Sock, -2);
		return;
	}
		
	if (sysctl(name, 3, info, &length, nullptr, NULL) < 0)
	{
		klog("second sysctl failed.\n");

		free(info);

		SockSendInt(Sock, -3);
		return;
	}

	count = length / sizeof(kinfo_proc);

	for (int i = 0; i < count; i++)
	{
		OrbisAppInfo appInfo;
		sceKernelGetAppInfo(info[i].pid, &appInfo);

		if (!strcmp(appInfo.TitleId, TitleId))
		{
			klog("Found appinfo for %s %i\n", TitleId, appInfo.AppId);
			appId = appInfo.AppId;
			break;
		}
	}

	free(info);

	if (appId <= 0)
	{
		SockSendInt(Sock, STATE_NOT_RUNNING);
	}
	else
	{
		bool state = false;
		auto res = sceSystemServiceIsAppSuspended(appId, &state);
		if (res == 0 && state)
		{
			SockSendInt(Sock, STATE_SUSPENDED);
		}
		else
		{
			SockSendInt(Sock, STATE_RUNNING);
		}
	}
}

Apps::Apps()
{

}

Apps::~Apps()
{

}