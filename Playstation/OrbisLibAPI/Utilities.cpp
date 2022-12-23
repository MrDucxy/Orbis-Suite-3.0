#include "Common.h"
#include "Utilities.h"

#pragma region Modules

void(*_sceSysmoduleLoadModuleInternal)(uint32_t); //Import is broken for some reason
int (*_sceSysmoduleLoadModuleByNameInternal)(const char* name, int, int, int, int);

bool LoadModules()
{
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

	sceKernelDlsym(ModuleHandle, "sceSysmoduleLoadModuleByNameInternal", (void**)&_sceSysmoduleLoadModuleByNameInternal);
	if (_sceSysmoduleLoadModuleInternal == nullptr) {
		klog("Failed to load _sceSysmoduleLoadModuleByNameInternal Import.\n");
		return false;
	}

	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_SYSTEM_SERVICE);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_USER_SERVICE);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_SYS_CORE);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_PAD);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_NETCTL);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_NET);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_HTTP);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_BGFT);
	_sceSysmoduleLoadModuleInternal(0xA4);

	return true;
}

#pragma endregion

#pragma region Misc

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
}

void Notify_Custom(const char* IconURI, const char* MessageFMT, ...)
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
	strcpy(Buffer.iconUri, IconURI); //Copy the uri to the buffer.

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

bool Jailbreak()
{
	jbc_cred cred;
	if (jbc_get_cred(&cred) != 0)
	{
		klog("jbc failed to get cred.\n");
		return false;
	}

	if (jbc_jailbreak_cred(&cred) != 0)
	{
		klog("jbc failed to jailbreak cred.\n");
		return false;
	}

	if (jbc_set_cred(&cred) != 0)
	{
		klog("jbc failed to set cred.\n");
		return false;
	}

	return true;
}

bool CopySflash()
{
	int sflashFd = sceKernelOpen("/dev/sflash0", ORBIS_KERNEL_O_RDONLY, 0);
	int backupFd = sceKernelOpen("/data/Orbis Suite/sflash0", ORBIS_KERNEL_O_CREAT | ORBIS_KERNEL_O_WRONLY | ORBIS_KERNEL_O_APPEND, 0777);
	if (sflashFd && backupFd)
	{
		auto buffer = (unsigned char*)malloc(4 * 1024 * 1024);
		if (buffer == nullptr)
		{
			klog("failled to allocate memory for sflash read.\n");
			return false;
		}

		size_t bytesRead = 0;
		while ((bytesRead = sceKernelRead(sflashFd, buffer, 4 * 1024 * 1024)) > 0)
		{
			sceKernelWrite(backupFd, buffer, bytesRead);
		}

		free(buffer);
		sceKernelClose(sflashFd);
		sceKernelClose(backupFd);
		return true;
	}

	return false;
}

int getMacAddress(OrbisNetIfName ifName_Num, char* strOut, size_t len)
{
	if (len < 18)
	{
		klog("getMacAddress(): Output len must be >= 18.\n");
		return -1;
	}

	OrbisNetIfEntry ifEntry;
	auto res = sceNetGetIfList(ifName_Num, &ifEntry, 1);
	if (res < 0)
	{
		klog("getMacAddress(): failed to get IfList for %i\n", ifName_Num);
		return res;
	}

	return sceNetEtherNtostr(ifEntry.MacAddress, strOut, len);
}

#pragma endregion

#pragma region Networking

bool SockSendInt(OrbisNetId Sock, int val)
{
	return !(sceNetSend(Sock, &val, sizeof(int), 0) < 0);
}

bool SockRecvInt(OrbisNetId Sock, int* val)
{
	return !(sceNetRecv(Sock, val, sizeof(int), 0) < 0);
}

#pragma endregion

void hexdump(void* ptr, int buflen) {
	unsigned char* buf = (unsigned char*)ptr;
	int i, j;
	for (i = 0; i < buflen; i += 16) {
		klog("%06x: ", i);
		for (j = 0; j < 16; j++)
			if (i + j < buflen)
				klog("%02x ", buf[i + j]);
			else
				klog("   ");
		klog(" ");
		for (j = 0; j < 16; j++)
			if (i + j < buflen)
				klog("%c", isprint(buf[i + j]) ? buf[i + j] : '.');
		klog("\n");
	}
}

int GetProcessList(std::vector<kinfo_proc>& ProcessList)
{
	size_t length;

	static int name[] = { CTL_KERN, KERN_PROC, KERN_PROC_PROC, 0 };

	// Get the size of buffer needed.
	if (sysctl(name, 3, nullptr, &length, nullptr, NULL) < 0)
		return -1;

	// Resize our vector to accommodate.
	ProcessList.resize(length / sizeof(kinfo_proc));

	// Retrive the processes.
	if (sysctl(name, 3, ProcessList.data(), &length, nullptr, NULL) < 0)
		return -1;

	// Remove duplicates.
	ProcessList.erase(std::unique(ProcessList.begin(), ProcessList.end(), [](kinfo_proc const& a, kinfo_proc const& b)
	{
		sceKernelGetProcessName(a.pid, (char*)a.name);
		return a.pid == b.pid;
	}), ProcessList.end());

	return 0;
}