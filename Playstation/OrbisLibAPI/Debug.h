#pragma once

class Debug
{
public:
	OrbisPthreadMutex DebugMutex;
	bool IsDebugging;
	int CurrentPID;

	Debug();
	~Debug();
	void HandleAPI(OrbisNetId Sock, APIPacket* Packet);

private:
	bool TryDetach(int pid);
	void Attach(OrbisNetId Sock);
	void Detach(OrbisNetId Sock);

	void LoadLibrary(OrbisNetId Sock);
	void UnloadLibrary(OrbisNetId Sock);
	void ReloadLibrary(OrbisNetId Sock);
	void GetLibraryList(OrbisNetId Sock);
};
