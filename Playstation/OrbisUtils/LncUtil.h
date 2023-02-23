#pragma once

namespace LncUtil
{
	struct LncLaunchAppParam
	{
		unsigned int size;      //0x00
		int userId;             //0x04
		int appAttr;            //0x08
		int enableCrashReport;  //0x0C
		uint64_t checkFlag;     //0x10
	};

	int IsAppLaunched(const char* titleId, bool* isLaunched);
	int IsAppSuspended(int appId, bool* isSuspended);
	int GetAppId(const char* titleId);
	int LaunchApp(const char* titleId, char** args, LncLaunchAppParam* appParam);
	int SuspendApp(int appId, int flags);
	int ResumeApp(int appId, int flags);
	int SetAppFocus(int appId, int flags);
}