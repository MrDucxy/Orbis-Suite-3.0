#pragma once

class Proc
{
public:
	Proc();
	~Proc();
	void HandleAPI(OrbisNetId Sock, std::shared_ptr<APIPacket> Packet);

	void SendProcessList(OrbisNetId Sock);

private:

};
