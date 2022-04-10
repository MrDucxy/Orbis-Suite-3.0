#pragma once

class Target
{
public:
	Target();
	~Target();
	void HandleAPI(OrbisNetId Sock, APIPacket* Packet);

	void SendTargetInfo(OrbisNetId Sock);
	void DoNotify(OrbisNetId Sock);

private:

};