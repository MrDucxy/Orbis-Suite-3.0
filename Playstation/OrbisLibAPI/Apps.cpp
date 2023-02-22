#include "Common.h"
#include "Apps.h"
#include "AppDatabase.h"
#include "ShellUIIPC.h"

#include <orbis/SysCore.h>
#include <orbis/SystemService.h>

void Apps::HandleAPI(OrbisNetId Sock, std::shared_ptr<APIPacket> Packet)
{
	char titleId[10];
	memset(titleId, 0, sizeof(titleId));

	// For commands that need it get the titleId of the App
	if (Packet->Command > API_APPS_GET_DB)
	{
		sceNetRecv(Sock, titleId, sizeof(titleId), 0);
	}

	switch (Packet->Command)
	{
	default:
		break;

	case API_APPS_CHECK_VER:

		CheckDBVersion(Sock);

		break;

	case API_APPS_GET_DB:

		GetDB(Sock);

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

void Apps::CheckDBVersion(OrbisNetId Sock)
{
	int remoteVersion = 0;
	auto currentVersion = AppDatabase::GetDBVersion();

	// Get the remote version.
	if (!Sockets::RecvInt(Sock, &remoteVersion))
	{
		klog("GetAppsList(): Failed to get remote app db version.\n");
		return;
	}

	// Send if the remote version is out of date.
	Sockets::SendInt(Sock, remoteVersion != currentVersion ? 1 : 0);
}

void Apps::GetDB(OrbisNetId Sock)
{
	// Open the file.
	auto fd = sceKernelOpen("/system_data/priv/mms/app.db", ORBIS_KERNEL_O_RDONLY, 0);
	if (!fd)
	{
		Sockets::SendInt(Sock, 0);
		return;
	}

	// Get File stats.
	OrbisKernelStat stats;
	if (sceKernelFstat(fd, &stats) != 0)
	{
		Sockets::SendInt(Sock, 0);
		return;
	}

	// Store the file size.
	auto fileSize = stats.st_size;

	// Allocate space to read data.
	auto fileData = (unsigned char*)malloc(fileSize);

	// ReadFile.
	if (sceKernelRead(fd, fileData, fileSize) <= 0)
	{
		// clean up.
		free(fileData);
		sceKernelClose(fd);

		Sockets::SendInt(Sock, 0);
		return;
	}

	// Send the size of the db file.
	Sockets::SendInt(Sock, fileSize);

	// Send the db file.
	Sockets::SendLargeData(Sock, fileData, fileSize);

	// clean up.
	free(fileData);
	sceKernelClose(fd);
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

int Apps::GetAppId(const char* TitleId)
{
	bool isLaunched = false;
	auto res = sceLncUtilIsAppLaunched(TitleId, &isLaunched);

	if (!isLaunched || res != 0)
		return 0;

	return sceLncUtilGetAppId(TitleId);
}

void Apps::SendAppStatus(OrbisNetId Sock, const char* TitleId)
{
	auto appId = GetAppId(TitleId);
	if (appId <= 0)
	{
		Sockets::SendInt(Sock, STATE_NOT_RUNNING);
	}
	else
	{
		bool isSuspended = 0;
		auto res = sceLncUtilIsAppSuspended(appId, &isSuspended);
		if (res == 0 && isSuspended)
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