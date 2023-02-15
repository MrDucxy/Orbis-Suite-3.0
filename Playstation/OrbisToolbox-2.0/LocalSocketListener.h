#pragma once

class LocalSocketListener
{
private:
	OrbisPthread ListenThreadHandle;
	OrbisNetId Socket;
	/// Used to signal thread to shut down
	bool ServerRunning;
	char ServerAddress[0x100];

	void* DoWork();
	static void* ClientThread(void* tdParam);
	void* tdParam;
	void(*ClientCallBack)(void* tdParam, OrbisNetId Sock);
	static void* ListenThread(void* tdParam);

public:
	LocalSocketListener(void(*ClientCallBack)(void* tdParam, OrbisNetId Sock), void* tdParam, char* ServerAddress);
	~LocalSocketListener();
};

struct ClientThreadParams
{
	LocalSocketListener* LocalSocketListener;
	OrbisNetId Sock;
};