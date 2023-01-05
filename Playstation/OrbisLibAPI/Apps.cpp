#include "Common.h"
#include "Apps.h"
#include "AppDatabase.h"
#include "ShellUIIPC.h"

#include <orbis/SysCore.h>
#include <orbis/SystemService.h>

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

	case API_APPS_DELETE:

		DeleteApp(Sock, titleId);

		break;

	case API_APPS_SET_VISIBILITY:

		SetVisibility(Sock, titleId);

		break;

	case API_APPS_GET_VISIBILITY:

		GetVisibility(Sock, titleId);

		break;
	}
}

void Apps::GetAppsList(OrbisNetId Sock)
{
	std::vector<AppDatabase::AppInfo> AppList;
	if (!AppDatabase::GetApps(AppList))
	{
		Sockets::SendInt(Sock, 0);
		return;
	}

	Sockets::SendInt(Sock, AppList.size());

	Sockets::SendLargeData(Sock, (unsigned char*)AppList.data(), AppList.size() * sizeof(AppInfoPacket));
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
		Sockets::SendInt(Sock, STATE_NOT_RUNNING);
	}
	else
	{
		bool state = false;
		auto res = sceSystemServiceIsAppSuspended(appId, &state);
		if (res == 0 && state)
		{
			Sockets::SendInt(Sock, STATE_SUSPENDED);
		}
		else
		{
			Sockets::SendInt(Sock, STATE_RUNNING);
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

		Sockets::SendInt(Sock, 0);
		return;
	}

	auto res = sceLncUtilLaunchApp(TitleId, nullptr, &appParam);
	if (res <= 0)
	{
		klog("sceLncUtilLaunchApp() : Failed with error % llX\n", res);

		Sockets::SendInt(Sock, 0);
		return;
	}

	Sockets::SendInt(Sock, res);
}

void Apps::KillApp(OrbisNetId Sock, const char* TitleId)
{
	auto appId = GetAppId(TitleId);

	if (appId > 0 && sceSystemServiceKillApp(appId, -1, 0, 0) == 0)
	{
		Sockets::SendInt(Sock, 1);
	}
	else
	{
		Sockets::SendInt(Sock, 0);
	}
}

void Apps::SuspendApp(OrbisNetId Sock, const char* TitleId)
{
	auto appId = GetAppId(TitleId);

	if (appId > 0 && sceLncUtilSuspendApp(appId, 0) == 0)
	{
		Sockets::SendInt(Sock, 1);
	}
	else
	{
		Sockets::SendInt(Sock, 0);
	}
}

void Apps::ResumeApp(OrbisNetId Sock, const char* TitleId)
{
	auto appId = GetAppId(TitleId);

	if (appId > 0 && sceLncUtilResumeApp(appId, 0) == 0 && sceLncUtilSetAppFocus(appId, 0) == 0)
	{
		Sockets::SendInt(Sock, 1);
	}
	else
	{
		Sockets::SendInt(Sock, 0);
	}
}

void Apps::DeleteApp(OrbisNetId Sock, const char* TitleId)
{
	auto result = sceAppInstUtilAppUnInstall(TitleId);

	Sockets::SendInt(Sock, (result == 0) ? 1 : 0);
}

void Apps::SetVisibility(OrbisNetId Sock, const char* TitleId)
{
	auto value = RecieveInt(Sock);

	if (value >= AppDatabase::VisibilityType::VT_NONE && value <= AppDatabase::VisibilityType::VT_INVISIBLE)
	{
		auto result = AppDatabase::SetVisibility(TitleId, (AppDatabase::VisibilityType)value);

		ShellUIIPC::RefreshContentArea();

		Sockets::SendInt(Sock, result ? 1 : 0);
	}

	Sockets::SendInt(Sock, 0);
}

void Apps::GetVisibility(OrbisNetId Sock, const char* TitleId)
{
	auto visibility = AppDatabase::GetVisibility(TitleId);
	Sockets::SendInt(Sock, visibility);
}

Apps::Apps()
{

}

Apps::~Apps()
{

}