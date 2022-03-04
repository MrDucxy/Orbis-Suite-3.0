#include "Common.h"
#include "OrbisDriver.h"

OrbisDriver::OrbisDriver()
{
}

OrbisDriver::~OrbisDriver()
{
}

bool OrbisDriver::TestDriver()
{
	int fd = sceKernelOpen("/dev/OrbisSuite", SCE_KERNEL_O_RDONLY, 0);
	if (fd > 0)
	{
		klog("Driver Installed...\n");
		sceKernelClose(fd);
		return true;
	}

	klog("Driver Not Installed...\n");
	return false;
}

bool OrbisDriver::GetDriverInfo(KDriver_Info* Info)
{
	int fd = sceKernelOpen("/dev/OrbisSuite", 0, 0);
	if (fd > 0)
	{
		int res = ioctl(fd, KDRIVER_INFO, Info);

		if (res != 0)
		{
			klog("KDRIVER_INFO failed with %d\n", res);

			sceKernelClose(fd);

			return false;
		}

		sceKernelClose(fd);

		return true;
	}
	else
		klog("Where kernel??\n");

	return false;
}

int OrbisDriver::LoadSPRX(const char* ProccessName, const char* Path, bool ShouldCallEntry)
{
	klog("OrbisDriver::LoadSPRX\n");
	auto fd = sceKernelOpen("/dev/OrbisSuite", 0, 0);
	if (fd > 0)
	{
		KDriver_ProcSPRX ProcSPRX;
		ProcSPRX.CallType = 0;
		strcpy(ProcSPRX.ProcName, ProccessName);
		strcpy(ProcSPRX.Path, Path);
		ProcSPRX.CallEntryExit = ShouldCallEntry;

		auto res = ioctl(fd, PROC_SPRX, &ProcSPRX);

		sceKernelClose(fd);

		return res;
	}
	else
		klog("Where kernel??\n");

	return -1;
}

int OrbisDriver::UnLoadSPRX(const char* ProccessName, const char* Name, bool ShouldCallExit)
{
	klog("OrbisDriver::UnLoadSPRX\n");
	auto fd = sceKernelOpen("/dev/OrbisSuite", 0, 0);
	if (fd > 0)
	{
		KDriver_ProcSPRX ProcSPRX;
		ProcSPRX.CallType = 1;
		strcpy(ProcSPRX.ProcName, ProccessName);
		strcpy(ProcSPRX.Path, Name);
		ProcSPRX.CallEntryExit = ShouldCallExit;

		auto res = ioctl(fd, PROC_SPRX, &ProcSPRX);

		sceKernelClose(fd);

		return res;
	}
	else
		klog("Where kernel??\n");

	return -1;
}

int OrbisDriver::UnLoadSPRX(const char* ProccessName, int ModuleHandle, bool ShouldCallExit)
{
	klog("OrbisDriver::UnLoadSPRX\n");
	auto fd = sceKernelOpen("/dev/OrbisSuite", 0, 0);
	if (fd > 0)
	{
		KDriver_ProcSPRX ProcSPRX;
		ProcSPRX.CallType = 2;
		strcpy(ProcSPRX.ProcName, ProccessName);
		ProcSPRX.Handle = ModuleHandle;
		ProcSPRX.CallEntryExit = ShouldCallExit;

		auto res = ioctl(fd, PROC_SPRX, &ProcSPRX);

		sceKernelClose(fd);

		return res;
	}
	else
		klog("Where kernel??\n");

	return -1;
}