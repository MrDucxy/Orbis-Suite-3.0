#include "Common.h"
#include "Proc.h"
#include "APIHelper.h"

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

	case API_PROC_LOAD_ELF:

		break;

	case API_PROC_CALL:

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
	// TODO: Update
	// Allocate space for packet to send.
	/*auto Packet = (ProcPacket*)malloc(sizeof(ProcPacket));

	// Gets list of running processes.
	auto procInfo = (ProcInfo*)malloc(sizeof(ProcInfo) * 200);
	memset(procInfo, 0, sizeof(ProcInfo) * 200);
	int ProcessCount = KDriver::GetProcessList(200, procInfo);

	// Send the number of processes running.
	sceNetSend(Sock, &ProcessCount, sizeof(int), 0);

	// Populate each process packet and send it.
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

	// Cleanup.
	free(Packet);
	free(procInfo);*/
}