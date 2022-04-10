#include "Common.h"
#include "Proc.h"
#include "APIHelper.h"
#include "KDriver.h"

void Proc::HandleAPI(OrbisNetId Sock, APIPacket* Packet)
{
	switch (Packet->Command)
	{
	default:
		break;

	case API_PROC_GET_LIST:

		SendProcessList(Sock);

		SendStatus(Sock, APIResults::API_OK);

		break;

	case API_PROC_ATTACH:

		break;

	case API_PROC_DETACH:

		break;

	case API_PROC_GET_CURRENT:

		break;

	case API_PROC_READ:

		break;

	case API_PROC_WRITE:

		break;

	case API_PROC_KILL:

		break;

	case API_PROC_LOAD_ELF:

		break;

	case API_PROC_CALL:

		break;

	case API_PROC_LOAD_SPRX:

		break;

	case API_PROC_UNLOAD_SPRX:

		break;

	case API_PROC_UNLOAD_SPRX_NAME:

		break;

	case API_PROC_RELOAD_SPRX_NAME:

		break;

	case API_PROC_RELOAD_SPRX_HANDLE:

		break;

	case API_PROC_DUMP_MODULE:

		break;

	case API_PROC_MODULE_LIST:

		break;
	}
}

Proc::Proc()
{

}

Proc::~Proc()
{

}

void Proc::SendProcessList(OrbisNetId Sock)
{
	auto Packet = (ProcPacket*)malloc(sizeof(ProcPacket));
	memset(Packet, 0, sizeof(ProcPacket));

	// TODO: Fill packet...
	auto procInfo = (ProcInfo*)malloc(sizeof(ProcInfo) * 200);
	memset(procInfo, 0, sizeof(ProcInfo) * 200);
	int ProcessCount = KDriver::GetProcessList(200, procInfo);

	klog("ProcCount: %i\n", ProcessCount);

	sceNetSend(Sock, &ProcessCount, sizeof(int), 0);

	for (auto i = 0; i < ProcessCount; i++)
	{
		memset(Packet, 0, sizeof(ProcPacket));

		Packet->ProcessID = procInfo[i].PID;
		Packet->Attached = procInfo[i].Attached;

		strcpy(Packet->ProcName, procInfo[i].ProcName);
		strcpy(Packet->TitleID, procInfo[i].TitleID);

		Packet->TextSegmentBase = procInfo[i].TextSegmentBase;
		Packet->TextSegmentLen = procInfo[i].TextSegmentLen;
		Packet->DataSegmentBase = procInfo[i].DataSegmentBase;
		Packet->DataSegmentLen = procInfo[i].DataSegmentLen;

		sceNetSend(Sock, Packet, sizeof(ProcPacket), 0);
	}

	free(Packet);
	free(procInfo);
}