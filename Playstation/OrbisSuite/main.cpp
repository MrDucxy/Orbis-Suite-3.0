#include "Common.h"
#include "KernelLoader.h"
#include <sys/ioctl.h>
#include "../../Misc/DriverDefinitions.h"
#include "OrbisDriver.h"
#include "MsgDialog.h"

#include <unistd.h>
#include <sys/stat.h>
#include <fcntl.h>

#define DEBUG_BUILD

int main()
{
	int SoftwareVersion = 0;
	int Handle = 0;

	// Load internal system modules.
	if (!LoadModules())
	{
		Notify("Failed to Load Modules...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Break Free
	if (!Jailbreak())
	{
		Notify("Jailbreak failed...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Get Firmware Version
	SoftwareVersion = GetUpdateVersion();

	//MakeDirs
	MakeDir("/data/Orbis Suite");

	// Copy Kernel ELF
	CopyKernelELF(SoftwareVersion);

	// Set the Kernel Version.
	SetKernelVersion(SoftwareVersion);

	// Set RW on the system directory. Seems to sus up some shit needs investigating...
	mount_large_fs("/dev/da0x4.crypt", "/system", "exfatfs", "511", MNT_UPDATE);

	// Install all the things! :D
	//InstallDaemon("ORBS30000"); // Orbis Lib
	//InstallDaemon("OFTP00001"); // Orbis FTP
	//InstallDaemon("PLDR00000"); // Orbis Payload Loader
	InstallOrbisToolbox();

	// Init Kernel driver if not present.
	if (!OrbisDriver::TestDriver())
	{
		klog("[Orbis Suite] Launching Kernel!!\n");
		char KernelPath[0x100];
		sprintf(KernelPath, DEBUGKERNELPATH, (SoftwareVersion >> 16));
		if (!LoadKernel(KernelPath, SoftwareVersion))
		{
			Notify("Failed to load Kernel ELF...");
			sceSystemServiceLoadExec("exit", 0);
			return 0;
		}

		klog("[Orbis Suite] Launching Toolbox!!\n");
		// Init Orbis Toolbox
		Handle = OrbisDriver::LoadSPRX("SceShellUI", "/data/Orbis Toolbox/OrbisToolbox-2.0.sprx");
		if (Handle > 0)
			klog("Orbis Toolbox loaded! %d\n", Handle);
	}
	else
	{
		sceSystemServiceHideSplashScreen();
		MsgDialog::Open("Orbis Suite is currently running...\nWould you like to:", true, ORBIS_MSG_DIALOG_BUTTON_TYPE_2BUTTONS, ORBIS_MSG_DIALOG_MODE_USER_MSG, "Unload Orbis Suite", "Reload Toolbox");

		if (MsgDialog::Result())
		{
			switch (MsgDialog::DialogResult.buttonId)
			{
			case ORBIS_MSG_DIALOG_BUTTON_ID_BUTTON1:
				OrbisDriver::UnLoadSPRX("SceShellUI", "OrbisToolbox-2.0.sprx");

				sceKernelSleep(3.0);

				if (!UnloadKernel())
				{
					klog("Failed to stop Kernel ELF...\n");
				}
				break;

			case ORBIS_MSG_DIALOG_BUTTON_ID_BUTTON2:
				OrbisDriver::UnLoadSPRX("SceShellUI", "Orbis Toolbox.sprx");
				sceKernelSleep(1.5);
				OrbisDriver::LoadSPRX("SceShellUI", "/data/Orbis Toolbox/OrbisToolbox-2.0.sprx");
				break;

			default:
				klog("Unknown... %i\n", MsgDialog::DialogResult.buttonId);
				break;
			}
		}
	}

	klog("Orbis Suite Started!\n");

	sceMsgDialogTerminate();
	sceSystemServiceLoadExec("exit", 0);

	return 0;
}