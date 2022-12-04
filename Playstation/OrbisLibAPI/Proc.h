#pragma once

class Proc
{
public:
	Proc();
	~Proc();
	void HandleAPI(OrbisNetId Sock, APIPacket* Packet);

	void SendProcessList(OrbisNetId Sock);

private:

};
