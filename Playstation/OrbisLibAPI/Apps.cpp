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
	int appId = 0;

	// Get the list of running processes.
	std::vector<kinfo_proc> processList;
	GetProcessList(processList);

	for (const auto& i : processList)
	{
		// Get the app info using the pid.
		OrbisAppInfo appInfo;
		sceKernelGetAppInfo(i.pid, &appInfo);

		// Using the titleId match our desired app and return the appId from the appinfo.
		if (!strcmp(appInfo.TitleId, TitleId))
		{
			appId = appInfo.AppId;

			break;
		}
	}

	// If we have no appId that means the process is not running. 
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