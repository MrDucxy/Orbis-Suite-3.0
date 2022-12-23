#include "Common.h"
#include "Target.h"
#include "APIHelper.h"
#include <orbis/SysCore.h>

void Target::HandleAPI(OrbisNetId Sock, APIPacket* Packet)
{
	switch (Packet->Command)
	{
	default:
		break;

	case APICommands::API_TARGET_INFO:

		SendTargetInfo(Sock);

		SendStatus(Sock, APIResults::API_OK);

		break;

	case APICommands::API_TARGET_RESTMODE:

		ChangeSystemState(SystemState::Suspend);

		break;

	case APICommands::API_TARGET_SHUTDOWN:

		ChangeSystemState(SystemState::Shutdown);

		break;

	case APICommands::API_TARGET_REBOOT:

		ChangeSystemState(SystemState::Reboot);;

		break;

	case APICommands::API_TARGET_NOTIFY:

		DoNotify(Sock);

		break;

	case APICommands::API_TARGET_BUZZER:

		RingBuzzer((BuzzerType)RecieveInt(Sock));

		break;

	case APICommands::API_TARGET_SET_LED:

		SetConsoleLED((ConsoleLEDColours)RecieveInt(Sock));

		break;

	case APICommands::API_TARGET_DUMP_PROC:

		break;

	case APICommands::API_TARGET_SET_SETTINGS:

		SetSettings(Sock);

		break;
	}
}

Target::Target()
{

}

Target::~Target()
{

}

void Target::SendTargetInfo(OrbisNetId Sock)
{
	auto Packet = (TargetInfoPacket*)malloc(sizeof(TargetInfoPacket));
	memset(Packet, 0, sizeof(TargetInfoPacket));

	Packet->SDKVersion = GetSDKVersion();
	Packet->SoftwareVersion = GetUpdateVersion();
	ReadFlash(FLASH_FACTORY_FW, &Packet->FactorySoftwareVersion, sizeof(int));
	Packet->CPUTemp = GetCPUTemp();
	Packet->SOCTemp = GetSOCTemp();

	auto bigAppAppId = sceSystemServiceGetAppIdOfBigApp();
	if (bigAppAppId > 0)
	{
		OrbisAppInfo bigAppInfo;
		sceApplicationGetAppInfoByAppId(bigAppAppId, &bigAppInfo);
		strcpy(Packet->CurrentTitleID, bigAppInfo.TitleId);
	}
	else
		strcpy(Packet->CurrentTitleID, "N/A");

	GetConsoleName(Packet->ConsoleName, 100);
	ReadFlash(FLASH_MB_SERIAL, &Packet->MotherboardSerial, 14);
	ReadFlash(FLASH_SERIAL, &Packet->Serial, 10);
	ReadFlash(FLASH_MODEL, &Packet->Model, 14);
	getMacAddress(SCE_NET_IF_NAME_ETH0, Packet->MACAdressLAN, 18);
	getMacAddress(SCE_NET_IF_NAME_WLAN0, Packet->MACAdressWIFI, 18);
	ReadFlash(FLASH_UART_FLAG, &Packet->UART, 1);
	ReadFlash(FLASH_IDU_MODE, &Packet->IDUMode, 1);
	GetIDPS(Packet->IDPS);
	GetPSID(Packet->PSID);
	Packet->ConsoleType = GetConsoleType();

	// TODO: Debugging Added Later.
	Packet->Attached = false; // TODO: Add funcionality.
	//Packet->CurrentProc

	sceUserServiceGetForegroundUser(&Packet->ForegroundAccountId);
	auto res = ShellCoreUtil::sceShellCoreUtilGetFreeSizeOfUserPartition(&Packet->FreeSpace, &Packet->TotalSpace);

	// Perf Stats. TODO: Move from toolbox
	/*Packet->ThreadCount = SystemMonitor::Thread_Count;
	Packet->AverageCPUUsage = SystemMonitor::Average_Usage;
	Packet->BusyCore = SystemMonitor::Busy_Core;
	memcpy(&Packet->Ram, &SystemMonitor::RAM, sizeof(MemoryInfo));
	memcpy(&Packet->VRam, &SystemMonitor::VRAM, sizeof(MemoryInfo));*/

	sceNetSend(Sock, Packet, sizeof(TargetInfoPacket), 0);

	free(Packet);
}

void Target::DoNotify(OrbisNetId Sock)
{
	auto Packet = new TargetNotifyPacket();

	auto res = sceNetRecv(Sock, Packet, sizeof(TargetNotifyPacket), 0);
	klog("%llX\n", res);
	if (!strcmp(Packet->IconURI, ""))
		Notify(Packet->Message);
	else
		Notify_Custom(Packet->IconURI, Packet->Message);

	SendStatus(Sock, APIResults::API_OK);
}

void Target::SetSettings(OrbisNetId Sock)
{
	//TODO: IPC here...
	/*auto Packet = new TargetSettingsPacket();

	sceNetRecv(Sock, Packet, sizeof(TargetSettingsPacket), 0);

	Debug_Feature::DebugTitleIdLabel::ShowLabels = Config::Data->Show_DebugTitleIdLabel = Packet->ShowDebugTitleIdLabel;
	Debug_Feature::DevkitPanel::ShowPanel = Config::Data->Show_DevkitPanel = Packet->ShowDevkitPanel;
	Debug_Feature::Custom_Content::Show_Debug_Settings = Config::Data->Show_Debug_Settings = Packet->ShowDebugSettings;
	Debug_Feature::Custom_Content::Show_App_Home = Config::Data->Show_App_Home = Packet->ShowAppHome;

	SendStatus(Sock, APIResults::API_OK);

	Config::SetSettingsNow = true;*/
}

//TODO: Get Target Settings.