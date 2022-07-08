#include "Common.h"
#include "NetWrapper.h"

int NetWrapper::NetGetIfListMacAddress(int ifName_num, MonoObject** mac_address)
{
	auto NetWrapper = Mono::Get_Class(Mono::platform_dll, "Sce.Vsh.ShellUI.Settings.Net", "NetWrapper");
	return Mono::Invoke<int>(Mono::platform_dll, NetWrapper, NULL, "NetGetIfListMacAddress", ifName_num, mac_address);
}

int NetWrapper::NetEtherNtostr(MonoObject* srcEtherAddr, MonoString** dstEtherAddr, unsigned int len)
{
	auto NetWrapper = Mono::Get_Class(Mono::platform_dll, "Sce.Vsh.ShellUI.Settings.Net", "NetWrapper");
	return Mono::Invoke<int>(Mono::platform_dll, NetWrapper, NULL, "NetEtherNtostr", srcEtherAddr, dstEtherAddr, len);
}

char* NetWrapper::GetMacAddressInfo(SceNetIfName net_if_name)
{
	MonoString* mac_addr = Mono::New_String("");
	auto srcEtherAddr = (MonoObject*)mono_object_unbox(Mono::New_Object(Mono::platform_dll, "Sce.Vsh.ShellUI.Settings.Net", "NetEtherAddr"));

	auto listMacAddressResult = NetGetIfListMacAddress(net_if_name, &srcEtherAddr);

	if (listMacAddressResult < 0 || listMacAddressResult != 1)
		return (char*)"";

	auto NetEthertostrResult = NetEtherNtostr(srcEtherAddr, &mac_addr, 18);

	if (NetEthertostrResult < 0)
		return (char*)"";

	return mono_string_to_utf8(mac_addr);
}