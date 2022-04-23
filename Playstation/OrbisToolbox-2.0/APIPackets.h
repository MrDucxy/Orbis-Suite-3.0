#pragma once

#define PACKET_VERSION 1

enum APICommands
{
	APITest = 1,

	/* ####### Proc functions ####### */
	PROC_START,
	API_PROC_GET_LIST,
	API_PROC_ATTACH,
	API_PROC_DETACH,
	API_PROC_GET_CURRENT,
	API_PROC_READ,
	API_PROC_WRITE,
	API_PROC_KILL,
	API_PROC_LOAD_ELF,
	API_PROC_CALL,

	/* Remote Library functions */
	API_PROC_LOAD_SPRX,
	API_PROC_UNLOAD_SPRX,
	API_PROC_UNLOAD_SPRX_NAME,
	API_PROC_RELOAD_SPRX_NAME,
	API_PROC_RELOAD_SPRX_HANDLE,
	API_PROC_DUMP_MODULE,
	API_PROC_MODULE_LIST,
	PROC_END,
	/* ############################## */

	/* ##### Debugger functions ##### */
	DBG_START,
	API_DBG_START, /* Debugger attach to target */
	API_DBG_STOP, /* Debugger detach from target */
	API_DBG_BREAK,
	API_DBG_RESUME,
	API_DBG_SIGNAL,
	API_DBG_STEP,
	API_DBG_STEP_OVER,
	API_DBG_STEP_OUT,
	API_DBG_GET_CALLSTACK,
	API_DBG_GET_REG,
	API_DBG_SET_REG,
	API_DBG_GET_FREG,
	API_DBG_SET_FREG,
	API_DBG_GET_DBGREG,
	API_DBG_SET_DBGREG,

	/* Thread Management */
	API_DBG_THREAD_LIST,
	API_DBG_THREAD_STOP,
	API_DBG_THREAD_RESUME,

	/* Breakpoint functions */
	API_DBG_BREAKPOINT_GETFREE,
	API_DBG_BREAKPOINT_SET,
	API_DBG_BREAKPOINT_UPDATE,
	API_DBG_BREAKPOINT_REMOVE,
	API_DBG_BREAKPOINT_GETINFO,
	API_DBG_BREAKPOINT_LIST,

	/* Watchpoint functions */
	API_DBG_WATCHPOINT_SET,
	API_DBG_WATCHPOINT_UPDATE,
	API_DBG_WATCHPOINT_REMOVE,
	API_DBG_WATCHPOINT_GETINFO,
	API_DBG_WATCHPOINT_LIST,
	DBG_END,
	/* ############################## */

	/* ###### Kernel functions ###### */
	KERN_START,
	API_KERN_BASE,
	API_KERN_READ,
	API_KERN_WRITE,
	KERN_END,
	/* ############################## */

	/* ###### Target functions ###### */
	TARGET_START,
	API_TARGET_INFO,
	API_TARGET_RESTMODE,
	API_TARGET_SHUTDOWN,
	API_TARGET_REBOOT,
	API_TARGET_NOTIFY,
	API_TARGET_BUZZER,
	API_TARGET_SET_LED,
	API_TARGET_DUMP_PROC,
	API_TARGET_SET_SETTINGS,
	//API_TARGET_LOAD_VSH_MODULE
	TARGET_END,
	/* ############################## */
};

enum APIResults
{
	API_OK = 1,

	API_ERROR_COULDNT_CONNECT,
	API_ERROR_NOT_CONNECTED,
	API_ERROR_NOT_ATTACHED,
	API_ERROR_LOST_PROC,
	API_ERROR_GENERAL,
	API_ERROR_INVALID_ADDRESS,

	//Debugger
	API_ERROR_PROC_RUNNING,
	API_ERROR_DEBUG_TO_ATTACHED,
};

struct APIPacket
{
	int PacketVersion;
	int Command;
	char ProcName[32];
};

#pragma region Process

struct ProcPacket
{
	int32_t ProcessID; //0x00
	int32_t Attached; //0x04
	char ProcName[32]; //0x08
	char TitleID[10]; //0x28
	uint64_t TextSegmentBase;
	uint64_t TextSegmentLen;
	uint64_t DataSegmentBase;
	uint64_t DataSegmentLen;
};

struct ModuleListPacket
{
	char Name[36]; //0x00
	char Handle[256]; //0x24
	int mHandle; //0x124
	uint64_t TextSegmentBase; //0x128
	uint64_t TextSegmentLen; //0x130
	uint64_t DataSegmentBase; //0x138
	uint64_t DataSegmentLen; //0x140
};


struct ProcRWPacket
{
	uint64_t Address;
	uint64_t Length;
};

struct ProcSPRXPacket
{
	char Name[256];
	char Path[256];
	int ModuleHandle;
	int Flags;
};

struct ProcBreakpointPacket
{
	int Index;
	uint64_t Address;
	int Enable;
};

#pragma endregion

#pragma region Debug



#pragma endregion


#pragma region Kernel



#pragma endregion

#pragma region Target

enum ConsoleTypes
{
	UNK,
	DIAG, //0x80
	DEVKIT, //0x81
	TESTKIT, //0x82
	RETAIL, //0x83 -> 0x8F
	KRATOS, //0xA0 IMPOSSIBLE??
};

struct TargetInfoPacket
{
	int SDKVersion;
	int SoftwareVersion;
	int FactorySoftwareVersion;
	int CPUTemp;
	int SOCTemp;
	char CurrentTitleID[10];
	char ConsoleName[100];
	char MotherboardSerial[14];
	char Serial[10];
	char Model[14];
	unsigned char MACAdressLAN[6];
	unsigned char MACAdressWIFI[6];
	int UART;
	int IDUMode;
	char IDPS[16];
	char PSID[16];
	int ConsoleType;
	int Attached;
	char CurrentProc[32];
}; //0x104

struct TargetNotifyPacket
{
	char IconURI[1024];
	char Message[1024];
};

struct TargetSettingsPacket
{
	int AutoLoadSettings;
	int ShowDebugTitleIdLabel;
	int ShowDevkitPanel;
	int ShowDebugSettings;
	int ShowAppHome;
	int ShowBuildOverlay;

	char GameOverlayLocation[256];
	int ShowCPUUsage;
	int ShowThreadCount;
	int Showram;
	int Showvram;
	int ShowCPUTemp;
	int ShowSOCTemp;
};

#pragma endregion
