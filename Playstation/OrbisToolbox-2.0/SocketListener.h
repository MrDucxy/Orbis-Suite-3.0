#pragma once

class SocketListener
{
private:
	OrbisPthread Thread;
	OrbisNetId Socket;
	/// Used to signal thread to shut down
	bool ServerRunning;
	/// Used to see when listen thread has closed.
	bool ThreadCleanedUp;
	unsigned short Port;
	OrbisPthread* ListenThreadHandle;

	void* DoWork();
	static void* ClientThread(void* tdParam);
	void* tdParam;
	void(*ClientCallBack)(void* tdParam, OrbisNetId Sock);
	static void* ListenThread(void* tdParam);

public:
	SocketListener(void(*ClientCallBack)(void* tdParam, OrbisNetId Sock), void* tdParam, unsigned short Port);
	~SocketListener();
};