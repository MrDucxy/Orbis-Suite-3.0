#include "Common.h"
#include "Target.h"
#include "APIHelper.h"
#include "Registry.h"
#include "System.h"
#include "Flash.h"

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

		ChangeSystemState(System_State::Suspend);

		SendStatus(Sock, APIResults::API_OK);

		break;

	case APICommands::API_TARGET_SHUTDOWN:

		ChangeSystemState(System_State::Shutdown);

		SendStatus(Sock, APIResults::API_OK);

		break;

	case APICommands::API_TARGET_REBOOT:

		ChangeSystemState(System_State::Reboot);

		SendStatus(Sock, APIResults::API_OK);

		break;

	case APICommands::API_TARGET_NOTIFY:

		DoNotify(Sock);

		break;

	case APICommands::API_TARGET_BUZZER:

		RingBuzzer((BuzzerType)RecieveInt(Sock));

		SendStatus(Sock, APIResults::API_OK);

		break;

	case APICommands::API_TARGET_SET_LED:

		SetConsoleLED((ConsoleLEDColours)RecieveInt(Sock));

		SendStatus(Sock, APIResults::API_OK);

		break;

	case APICommands::API_TARGET_DUMP_PROC:

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
	strcpy(Packet->CurrentTitleID, "TODO"); // Kernel
	GetConsoleName(Packet->ConsoleName, 100);
	ReadFlash(FLASH_MB_SERIAL, &Packet->MotherboardSerial, 14);
	ReadFlash(FLASH_SERIAL, &Packet->Serial, 10);
	ReadFlash(FLASH_MODEL, &Packet->Model, 14);
	ReadFlash(FLASH_LAN_MAC, &Packet->MACAdressLAN, 6);
	//MACAdressWIFI
	ReadFlash(FLASH_UART_FLAG, &Packet->UART, 1);
	ReadFlash(FLASH_IDU_MODE, &Packet->IDUMode, 1);
	GetIDPS(Packet->IDPS);
	GetPSID(Packet->PSID);
	Packet->ConsoleType = GetConsoleType();
	Packet->Attached = false; // TODO: Add funcionality.
	// CurrentProc

	sceNetSend(Sock, Packet, sizeof(TargetInfoPacket), 0);

	free(Packet);
}

void Target::DoNotify(OrbisNetId Sock)
{
	auto Packet = new TargetNotifyPacket();

	sceNetRecv(Sock, Packet, sizeof(TargetNotifyPacket), 0);

	if (!strcmp(Packet->IconURI, ""))
		Notify(Packet->Message);
	else
		Notify_Custom(Packet->IconURI, Packet->Message);

	SendStatus(Sock, APIResults::API_OK);
}