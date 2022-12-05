#pragma once
#include "..\..\Misc\General_IPC.h"

class GeneralIPC
{
private:
	static OrbisNetId Connect(const char* ProcessName);
	static bool SendCommand(OrbisNetId Sock, int Command);

public:
	static bool GetExtProcessInfo(const char* ProcessName, ExtProccesInfoPacket* info);
};