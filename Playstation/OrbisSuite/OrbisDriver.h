#pragma once
#include "../../Misc/DriverDefinitions.h"

class OrbisDriver
{
public:
	OrbisDriver();
	~OrbisDriver();

	static bool TestDriver();
	static bool GetDriverInfo(KDriver_Info* Info);
	static int LoadSPRX(const char* ProccessName, const char* Path, bool ShouldCallEntry = true);
	static int UnLoadSPRX(const char* ProccessName, const char* Name, bool ShouldCallExit = true);
	static int UnLoadSPRX(const char* ProccessName, int ModuleHandle, bool ShouldCallExit = true);
private:
	
};