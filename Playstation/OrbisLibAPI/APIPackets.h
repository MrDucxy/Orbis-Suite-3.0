#pragma once
#include "Common.h"

#define PACKET_VERSION 2

#pragma region Commands

enum APICommands
{
	/* ####### Proc functions ####### */
	PROC_START = 1,

	API_PROC_GET_LIST,
	API_PROC_LOAD_ELF,
	API_PROC_CALL, /* RPC Call. */

	PROC_END,
	/* ############################## */

	/* ####### Apps functions ####### */
	APP_START,

	API_APPS_GET_LIST,
	API_APPS_GET_INFO_STR,
	API_APPS_STATUS,
	API_APPS_START,
	API_APPS_STOP,
	API_APPS_SUSPEND,
	API_APPS_RESUME,
	API_APPS_DELETE,

	APP_END,
	/* ############################## */

	/* ##### Debugger functions ##### */
	DBG_START,

	API_DBG_ATTACH, /* Debugger attach to target */
	API_DBG_DETACH, /* Debugger detach from target */
	API_DBG_GET_CURRENT,
	API_DBG_READ,
	API_DBG_WRITE,
	API_DBG_KILL,
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

	/* Remote Library functions */
	API_DBG_LOAD_SPRX,
	API_DBG_UNLOAD_SPRX,
	API_DBG_RELOAD_SPRX,
	API_DBG_MODULE_LIST,

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
	API_TARGET_GETFILE,

	TARGET_END,
	/* ############################## */
};

#pragma endregion

#pragma region Generic

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
};

#pragma endregion

#pragma region Process

struct ProcPacket
{
	int32_t ProcessId;
	char Name[32];
	char TitleId[10];
};

#pragma endregion

#pragma region Apps

enum AppState
{
	STATE_NOT_RUNNING,
	STATE_RUNNING,
	STATE_SUSPENDED,
};

#pragma endregion

#pragma region Debug

struct LibraryPacket
{
	int64_t Handle;
	char Path[256];
	int32_t SegmentCount;
	OrbisKernelModuleInfo Segments[4];
};

struct DbgRWPacket
{
	uint64_t Address;
	uint64_t Length;
};

struct DbgSPRXPacket
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

#pragma region Kernel



#pragma endregion

#pragma region Target

struct MemoryInfo
{
	int Used;
	int Free;
	int Total;
	float Percentage;
};

struct TargetInfoPacket
{
	int SDKVersion;
	int SoftwareVersion;
	int FactorySoftwareVersion;
	char CurrentTitleID[10];
	char ConsoleName[100];
	char MotherboardSerial[14];
	char Serial[10];
	char Model[14];
	char MACAdressLAN[18];
	char MACAdressWIFI[18];
	int UART;
	int IDUMode;
	char IDPS[16];
	char PSID[16];
	int ConsoleType;
	int Attached;
	int AttachedPid;
	int ForegroundAccountId;

	uint64_t FreeSpace;
	uint64_t TotalSpace;

	int CPUTemp;
	int SOCTemp;
	int ThreadCount;
	float AverageCPUUsage;
	int BusyCore;
	MemoryInfo Ram;
	MemoryInfo VRam;
};

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
