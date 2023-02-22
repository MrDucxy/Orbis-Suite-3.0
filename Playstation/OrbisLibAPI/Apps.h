#pragma once
#include "Common.h"
#include "APIHelper.h"

class Apps
{
public:
	Apps();
	~Apps();

	void HandleAPI(OrbisNetId Sock, std::shared_ptr<APIPacket> Packet);

private:
	void CheckDBVersion(OrbisNetId Sock);
	void GetDB(OrbisNetId Sock);
	void GetAppInfoString(OrbisNetId Sock, const char* TitleId);
	int GetAppId(const char* TitleId);
	void SendAppStatus(OrbisNetId Sock, const char* TitleId);
	void StartApp(OrbisNetId Sock, const char* TitleId);
	void KillApp(OrbisNetId Sock, const char* TitleId);
	void SuspendApp(OrbisNetId Sock, const char* TitleId);
	void ResumeApp(OrbisNetId Sock, const char* TitleId);
	void DeleteApp(OrbisNetId Sock, const char* TitleId);
	void SetVisibility(OrbisNetId Sock, const char* TitleId);
	void GetVisibility(OrbisNetId Sock, const char* TitleId);
};
