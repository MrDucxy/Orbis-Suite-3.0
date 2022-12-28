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
	// Allocate space for packet to send.
	auto Packet = (ProcPacket*)malloc(sizeof(ProcPacket));

	// Get the list of running processes.
	std::vector<kinfo_proc> processList;
	GetProcessList(processList);

	Sockets::SendInt(Sock, processList.size());

	for (const auto& i : processList)
	{
		memset(Packet, 0, sizeof(ProcPacket));

		// Get the app info using the pid.
		OrbisAppInfo appInfo;
		sceKernelGetAppInfo(i.pid, &appInfo);

		Packet->AppId = appInfo.AppId;
		Packet->ProcessId = i.pid;
		strcpy(Packet->Name, i.name);
		strcpy(Packet->TitleId, appInfo.TitleId);

		sceNetSend(Sock, Packet, sizeof(ProcPacket), 0);
	}

	// Cleanup.
	free(Packet);
}