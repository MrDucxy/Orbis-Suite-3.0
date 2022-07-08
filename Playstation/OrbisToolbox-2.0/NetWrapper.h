#pragma once

enum SceNetIfName
{
	SCE_NET_IF_NAME_UNKNOWN = -2,
	SCE_NET_IF_NAME_NONE,
	SCE_NET_IF_NAME_LO0,
	SCE_NET_IF_NAME_PHYSICAL,
	SCE_NET_IF_NAME_ETH0 = 1,
	SCE_NET_IF_NAME_ETH1,
	SCE_NET_IF_NAME_DBG0,
	SCE_NET_IF_NAME_WLAN0,
	SCE_NET_IF_NAME_WLAN1,
	SCE_NET_IF_NAME_GBE0,
	SCE_NET_IF_NAME_BT0,
	SCE_NET_IF_NAME_PHONE0,
	SCE_NET_IF_NAME_VIRTUAL,
	SCE_NET_IF_NAME_PPPOE0 = 9,
	SCE_NET_IF_NAME_MAX
};

class NetWrapper
{
public:
	static int NetGetIfListMacAddress(int ifName_num, MonoObject** mac_address);
	static int NetEtherNtostr(MonoObject* srcEtherAddr, MonoString** dstEtherAddr, unsigned int len);
	static char* GetMacAddressInfo(SceNetIfName net_if_name);

};

