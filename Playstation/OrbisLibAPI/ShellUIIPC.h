#pragma once
#include "..\..\Misc\ShellUI_IPC.h"

class ShellUIIPC
{
private:
	static OrbisNetId Connect();
	static bool SendCommand(OrbisNetId Sock, int Command);

public:
	static void RefreshContentArea();
};