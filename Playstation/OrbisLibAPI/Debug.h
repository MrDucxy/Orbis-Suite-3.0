#pragma once

class Debug
{
public:
	OrbisPthreadMutex DebugMutex;
	OrbisPthread* ProcMonitorThreadHandle;
	bool IsDebugging;
	int CurrentPID;

	Debug();
	~Debug();
	void HandleAPI(OrbisNetId Sock, APIPacket* Packet);

private:
	bool TryDetach(int pid);
	void* ProcessMonotorThread();
	static void* ProcessMonotorThreadHelper(void* tdParam);
	void Attach(OrbisNetId Sock);
	void Detach(OrbisNetId Sock);

	void LoadLibrary(OrbisNetId Sock);
	void UnloadLibrary(OrbisNetId Sock);
	void ReloadLibrary(OrbisNetId Sock);
	void GetLibraryList(OrbisNetId Sock);

	void ReadWriteMemory(OrbisNetId Sock, bool write);
};
