#pragma once

// Genreall IPC ADDRS should follow the GeneralIPC-<ProcessName> pattern.
#define GENERAL_IPC_ADDR "/system_tmp/GeneralIPC-%d"

enum GeneralIPCCommands
{
	GIPC_HELLO,
	GIPC_LIB_LIST,
	GIPC_LIB_LOAD,
	GIPC_LIB_UNLOAD,
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

struct RWPacket
{
	bool Write;
	uint64_t Address;
	uint64_t Length;
};

struct LibPacket
{
	uint32_t Handle;
	char Path[256];
	int SegmentCount;
	OrbisKernelModuleSegmentInfo Segments[4];
};

struct PRXPacket
{
	uint32_t Handle;
	char Path[256];
};
