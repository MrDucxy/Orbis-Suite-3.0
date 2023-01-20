#pragma once

class Sockets
{
public:
	static bool SendInt(OrbisNetId Sock, int val);
	static bool RecvInt(OrbisNetId Sock, int* val);
	static bool SendLargeData(OrbisNetId Sock, unsigned char* data, size_t dataLen);
	static bool RecvLargeData(OrbisNetId Sock, unsigned char* data, size_t dataLen);
private:

};
