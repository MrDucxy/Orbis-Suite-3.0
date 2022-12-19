#pragma once
#include "Common.h"
#include "APIHelper.h"

class Apps
{
public:
	Apps();
	~Apps();

	void HandleAPI(OrbisNetId Sock, APIPacket* Packet);

private:
	void GetAppsList(OrbisNetId Sock);
	void GetAppInfoString(OrbisNetId Sock, const char* TitleId);
	int GetAppId(const char* TitleId);
	void SendAppStatus(OrbisNetId Sock, const char* TitleId);
	void StartApp(OrbisNetId Sock, const char* TitleId);
	void KillApp(OrbisNetId Sock, const char* TitleId);
	void SuspendApp(OrbisNetId Sock, const char* TitleId);
	void ResumeApp(OrbisNetId Sock, const char* TitleId);
};
