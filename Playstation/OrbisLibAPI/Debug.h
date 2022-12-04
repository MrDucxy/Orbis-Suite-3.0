#pragma once

class Debug
{
public:
	Debug();
	~Debug();
	void HandleAPI(OrbisNetId Sock, APIPacket* Packet);

private:
	bool IsDebugging;
	int CurrentPID;

};
