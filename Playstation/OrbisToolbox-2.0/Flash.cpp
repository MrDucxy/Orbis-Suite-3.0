#include "Common.h"
#include "Flash.h"

#define FLASH_BUF_LEN (4 * 1024 * 1024)
void ReadFlash(off_t Offset, void* Data, unsigned int Size)
{
	int fd = sceKernelOpen("/dev/sflash0", ORBIS_KERNEL_O_RDONLY, 0777);
	if (fd)
	{
		// For some reason I cant just read part of the flash. So all of it go brrr!
		auto Buffer = (unsigned char*)malloc(FLASH_BUF_LEN);
		memset(Buffer, 0, FLASH_BUF_LEN);

		int bytesread = sceKernelRead(fd, Buffer, FLASH_BUF_LEN);

		if (Offset + Size > bytesread)
			return;

		// Copy the wanted data from the sflash.
		memcpy(Data, Buffer + Offset, Size);

		free(Buffer);
		sceKernelClose(fd);
	}
	else
		klog("Failed to Open sflash\n");
}