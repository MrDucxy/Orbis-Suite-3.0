#include "Common.h"
#include "Utilities.h"
#include <orbis/Net.h>
#include <sys/uio.h>
#include <orbis/MsgDialog.h>

#pragma region Modules

bool(*Jailbreak)();
void(*_sceSysmoduleLoadModuleInternal)(uint32_t); //Import is broken for some reason

bool LoadModules()
{
	//Loading the Firmware Agnostic Jailbreak credits to https://github.com/sleirsgoevy
	int ModuleHandle_libjbc = sceKernelLoadStartModule("/app0/sce_module/libjbc.sprx", 0, nullptr, 0, nullptr, nullptr);
	if (ModuleHandle_libjbc == 0) {
		klog("Failed to load libjbc Library.\n");
		return false;
	}

	sceKernelDlsym(ModuleHandle_libjbc, "Jailbreak", (void**)&Jailbreak);
	if (Jailbreak == nullptr) {
		klog("Failed to load Jailbreak Import.\n");
		return false;
	}


	//Load the sysmodule library and import for sceSysmoduleLoadModuleInternal for some reason wouldnt auto import.
	char Buffer[0x200];
	sprintf(Buffer, "/%s/common/lib/libSceSysmodule.sprx", sceKernelGetFsSandboxRandomWord());
	int ModuleHandle = sceKernelLoadStartModule(Buffer, 0, nullptr, 0, nullptr, nullptr);
	if (ModuleHandle == 0) {
		klog("Failed to load libSceSysmodule Library.\n");
		return false;
	}

	sceKernelDlsym(ModuleHandle, "sceSysmoduleLoadModuleInternal", (void**)&_sceSysmoduleLoadModuleInternal);
	if (_sceSysmoduleLoadModuleInternal == nullptr) {
		klog("Failed to load _sceSysmoduleLoadModuleInternal Import.\n");
		return false;
	}

	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_SYS_CORE);

	return true;
}
#pragma endregion

#pragma region Misc

unsigned long Syscall(unsigned int n, ...) {
	asm(".intel_syntax noprefix");
	asm("xor %rax, %rax");
	asm("mov %r10, %rcx");
	asm("syscall");
	asm("ret");
}

void Notify(const char* MessageFMT, ...)
{
	OrbisNotificationRequest Buffer;

	//Create full string from va list.
	va_list args;
	va_start(args, MessageFMT);
	vsprintf(Buffer.message, MessageFMT, args);
	va_end(args);

	//Populate the notify buffer.
	Buffer.type = OrbisNotificationRequestType::NotificationRequest; //this one is just a standard one and will print what ever is stored at the buffer.Message.
	Buffer.unk3 = 0;
	Buffer.useIconImageUri = 1; //Bool to use a custom uri.
	Buffer.targetId = -1; //Not sure if name is correct but is always set to -1.
	strcpy(Buffer.iconUri, "https://i.imgur.com/SJPIBGG.png"); //Copy the uri to the buffer.

	//From user land we can call int64_t sceKernelSendNotificationRequest(int64_t unk1, char* Buffer, size_t size, int64_t unk2) which is a libkernel import.
	sceKernelSendNotificationRequest(0, &Buffer, 3120, 0);

	//What sceKernelSendNotificationRequest is doing is opening the device "/dev/notification0" or "/dev/notification1"
	// and writing the NotifyBuffer we created to it. Somewhere in ShellUI it is read and parsed into a json which is where
	// I found some clues on how to build the buffer.
}

void klog(const char* fmt, ...)
{
	char Buffer[0x200];

	//Create full string from va list.
	va_list args;
	va_start(args, fmt);
	vsprintf(Buffer, fmt, args);
	va_end(args);

	sceKernelDebugOutText(0, Buffer);
}

int GetUpdateVersion()
{
	int upd_version = 0;
	size_t upd_versionlen = 4;

	sysctlbyname("machdep.upd_version", (char*)&upd_version, &upd_versionlen, nullptr, 0);

	return upd_version;
}

#pragma endregion

#pragma region File IO

static void build_iovec(iovec** iov, int* iovlen, const char* name, const void* val, size_t len) {
	int i;

	if (*iovlen < 0)
		return;

	i = *iovlen;
	*iov = (iovec*)realloc(*iov, sizeof * *iov * (i + 2));
	if (*iov == NULL) {
		*iovlen = -1;
		return;
	}

	(*iov)[i].iov_base = strdup(name);
	(*iov)[i].iov_len = strlen(name) + 1;
	++i;

	(*iov)[i].iov_base = (void*)val;
	if (len == (size_t)-1) {
		if (val != NULL)
			len = strlen((const char*)val) + 1;
		else
			len = 0;
	}
	(*iov)[i].iov_len = (int)len;

	*iovlen = ++i;
}

int nmount(struct iovec* iov, uint32_t niov, int flags)
{
	return syscall(378, iov, niov, flags);
}

int unmount(const char* dir, int flags)
{
	return syscall(22, dir, flags);
}

int mount_large_fs(const char* device, const char* mountpoint, const char* fstype, const char* mode, unsigned int flags)
{
	struct iovec* iov = NULL;
	int iovlen = 0;

	unmount(mountpoint, 0);

	build_iovec(&iov, &iovlen, "fstype", fstype, -1);
	build_iovec(&iov, &iovlen, "fspath", mountpoint, -1);
	build_iovec(&iov, &iovlen, "from", device, -1);
	build_iovec(&iov, &iovlen, "large", "yes", -1);
	build_iovec(&iov, &iovlen, "timezone", "static", -1);
	build_iovec(&iov, &iovlen, "async", "", -1);
	build_iovec(&iov, &iovlen, "ignoreacl", "", -1);

	if (mode) {
		build_iovec(&iov, &iovlen, "dirmask", mode, -1);
		build_iovec(&iov, &iovlen, "mask", mode, -1);
	}

	return nmount(iov, iovlen, flags);
}

void CopyFile(const char* File, const char* Destination)
{
	int src = 0, dst = 0;
	OrbisKernelStat Stats;

	//Open file descriptors 
	src = sceKernelOpen(File, ORBIS_KERNEL_O_RDONLY, 0);
	if (src <= 0)
	{
		klog("Failed to open Source File.\n");
		return;
	}

	dst = sceKernelOpen(Destination, ORBIS_KERNEL_O_CREAT | ORBIS_KERNEL_O_WRONLY, 0777);
	if (dst <= 0)
	{
		klog("Failed to open Destination File.\n");
		return;
	}

	//Get File size
	sceKernelFstat(src, &Stats);

	if (Stats.st_size == 0)
	{
		klog("Failed to get file size.\n");
		return;
	}

	//Allocate space to read data.
	char* FileData = (char*)malloc(Stats.st_size);

	//ReadFile.
	sceKernelRead(src, FileData, Stats.st_size);

	//Write the file data.
	sceKernelWrite(dst, FileData, Stats.st_size);

	//Close The handles.
	sceKernelClose(src);
	sceKernelClose(dst);
}

int MakeDir(char* Dir, ...)
{
	char buffer[0x400] = { 0 };
	va_list args;
	va_start(args, Dir);
	vsprintf(buffer, Dir, args);
	va_end(args);

	klog("Creating Directory \"%s\"...\n", buffer);
	return sceKernelMkdir(buffer, 0777);
}

#pragma endregion