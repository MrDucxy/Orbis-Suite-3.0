#pragma once

class Debug;

class Target
{
public:
	Target(Debug* Debug);
	~Target();
	void HandleAPI(OrbisNetId Sock, APIPacket* Packet);

	void SendTargetInfo(OrbisNetId Sock);
	void DoNotify(OrbisNetId Sock);
	void SetSettings(OrbisNetId Sock);

private:
	Debug* Debug;
};