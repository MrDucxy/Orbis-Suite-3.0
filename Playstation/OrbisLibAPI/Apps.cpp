#include "Common.h"
#include "Apps.h"
#include <orbis/SysCore.h>
#include <orbis/SystemService.h>
#include "AppDatabase.h"

void Apps::HandleAPI(OrbisNetId Sock, APIPacket* Packet)
{
	char titleId[10];
	memset(titleId, 0, sizeof(titleId));

	// For commands that need it get the titleId of the App
	if (Packet->Command > API_APPS_GET_LIST)
	{
		sceNetRecv(Sock, titleId, sizeof(titleId), 0);
	}

	switch (Packet->Command)
	{
	default:
		break;

	case API_APPS_GET_LIST:

		GetAppsList(Sock);

		break;

	case API_APPS_GET_INFO_STR:

		GetAppInfoString(Sock, titleId);

		break;

	case API_APPS_STATUS:
		
		SendAppStatus(Sock, titleId);

		break;

	case API_APPS_START:

		StartApp(Sock, titleId);

		break;

	case API_APPS_STOP:

		KillApp(Sock, titleId);

		break;

	case API_APPS_SUSPEND:

		SuspendApp(Sock, titleId);

		break;

	case API_APPS_RESUME:

		ResumeApp(Sock, titleId);

		break;
	}
}

void Apps::GetAppsList(OrbisNetId Sock)
{
	std::vector<AppDatabase::AppInfo> AppList;
	if (!AppDatabase::GetApps(AppList))
	{
		SockSendInt(Sock, 0);
		return;
	}

	// Send the number of apps.
	SockSendInt(Sock, AppList.size());

	// Send all of the apps.
	for (const auto& App : AppList)
	{
		sceNetSend(Sock, &App, sizeof(AppDatabase::AppInfo), 0);
	}
}

void Apps::GetAppInfoString(OrbisNetId Sock, const char* TitleId)
{
	// Get the key we are interested in.
	char KeyValue[50];
	sceNetRecv(Sock, KeyValue, sizeof(KeyValue), 0);

	// Look up the key for that titleId in the app.db.
	char OutStr[200];
	memset(OutStr, 0, sizeof(OutStr));
	AppDatabase::GetAppInfoString(TitleId, OutStr, sizeof(OutStr), KeyValue);

	// Send back the result.
	sceNetSend(Sock, OutStr, sizeof(OutStr), 0);
}

// TODO: Currently cant get the appId of child processes like the Web Browser since it is a child of the ShellUI.
int Apps::GetAppId(const char* TitleId)
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

	return appId;
}

void Apps::SendAppStatus(OrbisNetId Sock, const char* TitleId)
{
	auto appId = GetAppId(TitleId);

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

void Apps::StartApp(OrbisNetId Sock, const char* TitleId)
{
	LaunchAppParam appParam;
	appParam.size = sizeof(LaunchAppParam);

	if (auto res = sceUserServiceGetForegroundUser(&appParam.userId) != 0)
	{
		klog("sceUserServiceGetForegroundUser(): Failed with error %llX\n", res);

		SockSendInt(Sock, 0);
		return;
	}

	auto res = LncUtil::sceLncUtilLaunchApp(TitleId, nullptr, &appParam);
	if (res <= 0)
	{
		klog("sceLncUtilLaunchApp() : Failed with error % llX\n", res);

		SockSendInt(Sock, 0);
		return;
	}

	SockSendInt(Sock, res);
}

void Apps::KillApp(OrbisNetId Sock, const char* TitleId)
{
	auto appId = GetAppId(TitleId);

	if (appId > 0 && sceSystemServiceKillApp(appId, -1, 0, 0) == 0)
	{
		SockSendInt(Sock, 1);
	}
	else
	{
		SockSendInt(Sock, 0);
	}
}

void Apps::SuspendApp(OrbisNetId Sock, const char* TitleId)
{
	auto appId = GetAppId(TitleId);

	if (appId > 0 && LncUtil::sceLncUtilSuspendApp(appId, 0) == 0)
	{
		SockSendInt(Sock, 1);
	}
	else
	{
		SockSendInt(Sock, 0);
	}
}

void Apps::ResumeApp(OrbisNetId Sock, const char* TitleId)
{
	auto appId = GetAppId(TitleId);

	if (appId > 0 && LncUtil::sceLncUtilResumeApp(appId, 0) == 0)
	{
		SockSendInt(Sock, 1);
	}
	else
	{
		SockSendInt(Sock, 0);
	}
}

Apps::Apps()
{

}

Apps::~Apps()
{

}