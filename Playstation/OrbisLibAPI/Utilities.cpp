#include "Common.h"
#include "Utilities.h"

#pragma region Modules

void(*_sceSysmoduleLoadModuleInternal)(uint32_t); //Import is broken for some reason

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

	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_SYSTEM_SERVICE);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_USER_SERVICE);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_SYS_CORE);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_PAD);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_NETCTL);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_NET);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_HTTP);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_BGFT);
	_sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_APPINSTUTIL);
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

int sys_dynlib_get_info(int moduleHandle, SceDbgModuleInfo* destModuleInfo)
{
	destModuleInfo->size = sizeof(*destModuleInfo);
	return syscall(593, moduleHandle, destModuleInfo);
}

int sys_dynlib_get_list(int* destModuleHandles, int max, int* count)
{
	return syscall(592, destModuleHandles, max, count);
}

#pragma endregion