#include "Common.h"
#include "Flash.h"

#define FLASH_BUF_LEN (4 * 1024 * 1024)
void ReadFlash(off_t Offset, void* Data, unsigned int Size)
{
	int fd = sceKernelOpen("/dev/sflash0", ORBIS_KERNEL_O_RDONLY, 0777);
	// int fd = sceKernelOpen("/data/Orbis Toolbox/sflash0", ORBIS_KERNEL_O_RDONLY, 0777);
	if (fd)
	{
		sceKernelPread(fd, Data, Size, Offset);
		sceKernelClose(fd);
	}
	else
		klog("Failed to Open sflash\n");
}