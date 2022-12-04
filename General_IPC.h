#pragma once

// Genreall IPC ADDRS should follow the GeneralIPC#CUSA00000 pattern.
#define GENERAL_IPC_ADDR "GeneralIPC#%s"

enum GeneralIPCCommands
{
	GIPC_INFO,
	GIPC_LIB_LIST,
	GIPC_JAILBREAK,
	GIPC_JAIL,
	GIPC_RW,
	GIPC_PROT,
};

struct ExtProccesInfoPacket
{
	char Path[64];
	char TitleId[16];
	char ContentId[64];
	char Version[6];
};