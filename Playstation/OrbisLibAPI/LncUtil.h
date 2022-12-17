#pragma once

enum AppType
{
	AppTypeInvalid = -1,
	Unknown,
	ShellUI,
	Daemon,
	CDLG,
	MiniApp,
	BigApp,
	ShellCore,
	ShellApp
};

struct AppStatus
{
	int AppId;
	int LaunchRequestAppId;
	char AppType;
};

struct AppStatusForShellUIReboot
{
	int appId;
	char appType;
	char appAttr;
	int launchRequestAppId;
	int userId;
	int isActiveCdlg;
	char path[1024];
	int isCoredumped;
	int isPrxModuleLoadFailed;
	int appLocalPid;
	char crashReportMode;
	char category[4];
};

class LncUtil
{
public:
	static int Init();
	static int sceLncUtilGetAppId(const char* TitleId);
	static int GetAppStatusListForShellUIReboot(AppStatusForShellUIReboot* outStatusList, unsigned int numEntries, unsigned int* outEntries);

private:
	static uint64_t LibraryBaseAddress;
	static int(*_sceLncUtilGetAppId)(const char* titleId);
	static int(*_GetAppStatusListForShellUIReboot)(AppStatusForShellUIReboot* outStatusList, unsigned int numEntries, unsigned int* outEntries);
};
