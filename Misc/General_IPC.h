#pragma once

// Genreall IPC ADDRS should follow the GeneralIPC#<ProcessName> pattern.
#define GENERAL_IPC_ADDR "/system_tmp/GeneralIPC#%s"

enum GeneralIPCCommands
{
	GIPC_INFO,
	GIPC_LIB_LIST,
	GIPC_JAILBREAK,
	GIPC_JAIL,
	GIPC_RW,
	GIPC_PROT,
};

enum GeneralIPCResult
{
	GIPC_FAIL,
	GIPC_OK,
};

struct ExtProccesInfoPacket
{
	char Path[64];
	char TitleId[16];
	char ContentId[64];
	char Version[6];
};