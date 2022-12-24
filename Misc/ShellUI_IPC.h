#pragma once 

#define SHELL_IPC_ADDR "/system_tmp/ShellIPC"

enum ShellIPCCommands
{
	// Returns the current handle.
	SIPC_HANDLE = 1,
	// Refreshes the app list on the Home menu.
	SIPC_REFRESH_CONTENT_AREA,
	// Sets/Gets the Orbis Toolbox settings bellow.
	SIPC_SETTINGS_RW,
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