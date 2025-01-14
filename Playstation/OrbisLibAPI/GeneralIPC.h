#pragma once
#include "..\..\Misc\General_IPC.h"

class GeneralIPC
{
private:
	static OrbisNetId Connect(int pid);
	static bool SendCommand(OrbisNetId Sock, int Command);

public:
	static void DeleteTempFile(int pid);
	static bool TestConnection(int pid);
	static bool GetLibraryList(int pid, std::vector<LibraryPacket>& Libraries);
	static bool LoadLibrary(int pid, const char* Path, int* HandleOut);
	static bool UnLoadLibrary(int pid, int Handle);
	static bool Jailbreak(int pid);
	static bool Jail(int pid);
	static bool ReadWriteMemory(int pid, uint64_t address, unsigned char* data, size_t length, bool write);
};