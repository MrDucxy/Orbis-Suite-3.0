#include "Common.h"
#include "Flash.h"

void ReadFlash(off_t Offset, void* Data, unsigned int Size)
{
	int fd = sceKernelOpen("/dev/sflash0", ORBIS_KERNEL_O_RDONLY, 0777);
	if (fd)
	{
		sceKernelPread(fd, Data, Size, Offset);
		sceKernelClose(fd);
	}
	else
		klog("Failed to Open sflash\n");
}