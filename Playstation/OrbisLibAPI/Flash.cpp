#include "Common.h"
#include "Flash.h"

void ReadFlash(off_t Offset, void* Data, unsigned int Size)
{
	int fd = sceKernelOpen("/data/Orbis Suite/sflash0", ORBIS_KERNEL_O_RDONLY, 0777);
	if (fd)
	{
		auto res = sceKernelPread(fd, Data, Size, Offset);
		sceKernelClose(fd);
	}
	else
	{
		klog("Failed to Open sflash\n");
	}
}