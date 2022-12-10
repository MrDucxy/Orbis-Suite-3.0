#pragma once

class NetWrapper
{
public:
	static int NetGetIfListMacAddress(int ifName_num, MonoObject** mac_address);
	static int NetEtherNtostr(MonoObject* srcEtherAddr, MonoString** dstEtherAddr, unsigned int len);
	static char* GetMacAddressInfo(SceNetIfName net_if_name);

};

