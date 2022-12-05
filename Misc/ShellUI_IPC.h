#pragma once 

#define SHELL_IPC_ADDR "/system_tmp/ShellIPC"
#define GENERAL_IPC_PORT 2023

enum ShellIPCCommands
{
	SIPC_HANDLE,
	SIPC_TARGET_INFO,
	SIPC_SETTINGS_RW,
};

struct ShellUI_TargetInfoPacket
{
	char MACAdressLAN[18];
	char MACAdressWIFI[18];

	uint64_t FreeSpace;
	uint64_t TotalSpace;

	int ThreadCount;
	float AverageCPUUsage;
	int BusyCore;
	MemoryInfo Ram;
	MemoryInfo VRam;
};

struct ShellUI_SettingsPacket
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