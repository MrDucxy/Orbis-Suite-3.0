#pragma once

class Debug
{
public:
	bool IsDebugging;
	int CurrentPID;

	Debug();
	~Debug();
	void HandleAPI(OrbisNetId Sock, APIPacket* Packet);

private:
	bool TryDetach(int pid);
	void Attach(OrbisNetId Sock);
	void Detach(OrbisNetId Sock);

};
