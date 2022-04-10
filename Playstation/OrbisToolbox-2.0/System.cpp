#include "Common.h"
#include "System.h"
#include "Registry.h"

int ChangeSystemState(System_State State)
{
	OrbisKernelEventFlag EventFlag = 0;
	int ret = 0;

	// sceSysCoreSetRebootFlag
	ret = sceKernelOpenEventFlag(&EventFlag, "SceSysCoreReboot");
	if (!ret)
	{
		ret = sceKernelCancelEventFlag(EventFlag, State, 0);
		if (!ret)
		{
			sceKernelCloseEventFlag(EventFlag);
			ret = 0;
		}
	}

	sceKernelIccIndicatorShutdown();
	kill(1, 30); //Terminate Process 1 (mini-syscore.elf) with the signal 30.

	return ret;
}

void SetConsoleLED(ConsoleLEDColours Colour)
{
	switch (Colour)
	{
	default:
	case white:
		sceKernelIccIndicatorBootDone();
		break;

	case white_Blinking:
		sceKernelIccIndicatorShutdown();
		break;

	case Blue_Blinking:
		sceKernelIccIndicatorStandbyBoot();
		break;
	}
}

void SetControllerLED()
{

}

void RingBuzzer(BuzzerType Type)
{
	if (Type < 6)
		sceKernelIccSetBuzzer(Type);
}

int32_t GetCPUTemp()
{
	int32_t CPUTemp = 0;
	if (sceKernelGetCpuTemperature(&CPUTemp) == 0)
		return CPUTemp;
	else
		return -1;
}

int32_t GetSOCTemp()
{
	int32_t SOCTemp = 0;
	if (sceKernelGetSocSensorTemperature(0, &SOCTemp) == 0)
		return SOCTemp;
	else
		return -1;
}

int GetSDKVersion()
{
	int sdk_version = 0;
	size_t sdk_versionlen = 4;

	sysctlbyname("kern.sdk_version", (char*)&sdk_version, &sdk_versionlen, nullptr, 0);

	return sdk_version;
}

int GetUpdateVersion()
{
	int sdk_version = 0;
	size_t sdk_versionlen = 4;

	sysctlbyname("machdep.upd_version", (char*)&sdk_version, &sdk_versionlen, nullptr, 0);

	return sdk_version;
}

bool GetConsoleName(char* Out, size_t len)
{
	return sceRegMgrGetStr(Registry_Find_By_Name("/SYSTEM/nickname"), Out, len) == 0;
}

int GetIDPS(char* Out)
{
	size_t IDPSlen = 16;
	return sysctlbyname("machdep.idps", Out, &IDPSlen, NULL, NULL);
}

int GetPSID(char* Out)
{
	size_t PSID = 16;
	return sysctlbyname("machdep.openpsid", Out, &PSID, NULL, NULL);
}

Console_Types GetConsoleType()
{
	char IDPS[16] = { 0 };
	size_t IDPSlen = 16;
	sysctlbyname("machdep.idps", (char*)&IDPS, &IDPSlen, NULL, NULL);

	switch (IDPS[5] & 0xffU)
	{
	case 0x80:
		return CT_DIAG;
		break;

	case 0x81:
		return CT_DEVKIT;
		break;

	case 0x82:
		return CT_TESTKIT;
		break;

	case 0x83 ... 0x8F:
		return CT_RETAIL;
		break;

	case 0xA0:
		return CT_KRATOS;
		break;
	}

	return CT_UNK;
}