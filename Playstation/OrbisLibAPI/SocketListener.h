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
	OrbisPthread ListenThreadHandle;

	void ListenThread();
	void* tdParam;
	void(*ClientCallBack)(void* tdParam, OrbisNetId Sock, OrbisNetInAddr sin_addr);

public:
	struct ClientThreadParams
	{
		SocketListener* socketListener;
		OrbisNetId Sock;
		OrbisNetInAddr sin_addr;
	};

	SocketListener(void(*ClientCallBack)(void* tdParam, OrbisNetId Sock, OrbisNetInAddr sin_addr), void* tdParam, unsigned short Port);
	~SocketListener();
};