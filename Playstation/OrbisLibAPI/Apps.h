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
	void SendAppStatus(OrbisNetId Sock, const char* TitleId);
};
