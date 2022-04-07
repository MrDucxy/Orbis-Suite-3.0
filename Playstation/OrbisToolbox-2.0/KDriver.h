#pragma once
#include "../../Misc/DriverDefinitions.h"

class KDriver
{
public:
	// Driver
	static bool TestDriver();
	static bool GetDriverInfo(KDriver_Info* Info);

	// Proc
	static int GetProcessList(int ProcessCount, ProcInfo* ProcList);
	// AttachtoProcess
	// DetachfromProcess
	// ReadProcessMemory
	// WriteProcessMemory
	// GetProcessInfo
	// GetModuleList
	static int LoadSPRX(const char* ProccessName, const char* Path, bool ShouldCallEntry = true);
	static int UnLoadSPRX(const char* ProccessName, const char* Name, bool ShouldCallExit = true);
	static int UnLoadSPRX(const char* ProccessName, int ModuleHandle, bool ShouldCallExit = true);

private:

};