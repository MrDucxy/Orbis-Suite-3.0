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

enum Flag
{
	Flag_None = 0,
	SkipLaunchCheck = 1,
	SkipResumeCheck = 1,
	SkipSystemUpdateCheck = 2,
	RebootPatchInstall = 4,
	VRMode = 8,
	NonVRMode = 16
};

struct LaunchAppParam
{
	unsigned int size;      //0x00
	int userId;             //0x04
	int appAttr;            //0x08
	int enableCrashReport;  //0x0C
	uint64_t checkFlag;     //0x10
};

class LncUtil
{
public:
	static int Init();
	static int sceLncUtilGetAppId(const char* TitleId);
	static int sceLncUtilLaunchApp(const char* titleId, char* args, LaunchAppParam* appParam);
	static int sceLncUtilSuspendApp(int AppId, int Flag);
	static int sceLncUtilResumeApp(int AppId, int Flag);

private:
	static uint64_t LibraryBaseAddress;
	static int(*_sceLncUtilGetAppId)(const char* titleId);
	static int(*_sceLncUtilLaunchApp)(const char* titleId, char* args, LaunchAppParam* appParam);
	static int(*_sceLncUtilSuspendApp)(int AppId, int Flag);
	static int(*_sceLncUtilResumeApp)(int AppId, int Flag);
};
