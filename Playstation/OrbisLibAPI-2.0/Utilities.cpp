#include "stdafx.h"
#include "Utilities.h"
#include <libsysmodule.h>

int(*sceKernelDebugOutText)(int dbg_channel, const char* text, ...);
int(*sceKernelSendNotificationRequest)(int device, SceNotificationRequest* req, size_t size, int blocking);
int(*sceKernelGetModuleInfo)(SceKernelModule handle, SceDbgModuleInfo* info);

bool LoadModules()
{
	int res = 0;
	//auto res = sceSysmoduleLoadModule(SCE_SYSMODULE_INTERNAL_SYSTEM_SERVICE);
	//if (res != 0)
	//{
	//	klog("LoadModules(): Failed to load SCE_SYSMODULE_INTERNAL_SYSTEM_SERVICE (%llX)\n", res);
	//	return false;
	//}
	//
	//res = sceSysmoduleLoadModule(SCE_SYSMODULE_INTERNAL_APPINSTUTIL);
	//if (res != 0)
	//{
	//	klog("LoadModules(): Failed to load SCE_SYSMODULE_INTERNAL_APPINSTUTIL (%llX)\n", res);
	//	return false;
	//}
	//
	//res = sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_USER_SERVICE);
	//if (res != 0)
	//{
	//	klog("LoadModules(): Failed to load SCE_SYSMODULE_INTERNAL_USER_SERVICE (%llX)\n", res);
	//	return false;
	//}
	//
	//res = sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_SYS_CORE);
	//if (res != 0)
	//{
	//	klog("LoadModules(): Failed to load SCE_SYSMODULE_INTERNAL_SYS_CORE (%llX)\n", res);
	//	return false;
	//}
	//
	//res = sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_NETCTL);
	//if (res != 0)
	//{
	//	klog("LoadModules(): Failed to load SCE_SYSMODULE_INTERNAL_NETCTL (%llX)\n", res);
	//	return false;
	//}
	//
	//res = sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_NET);
	//if (res != 0)
	//{
	//	klog("LoadModules(): Failed to load SCE_SYSMODULE_INTERNAL_NET (%llX)\n", res);
	//	return false;
	//}
	//
	//res = sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_HTTP);
	//if (res != 0)
	//{
	//	klog("LoadModules(): Failed to load SCE_SYSMODULE_INTERNAL_HTTP (%llX)\n", res);
	//	return false;
	//}
	//
	//res = sceSysmoduleLoadModuleInternal(SCE_SYSMODULE_INTERNAL_BGFT);
	//if (res != 0)
	//{
	//	klog("LoadModules(): Failed to load SCE_SYSMODULE_INTERNAL_BGFT (%llX)\n", res);
	//	return false;
	//}
	//
	//sceSysmoduleLoadModuleInternal(0x8000000D);

	// Start up networking interface
	res = sceNetInit();
	if (res != 0)
	{
		klog("LoadModules(): sceNetInit failed\n");
		return false;
	}

	// Start up user service.
	res = sceUserServiceInitialize(nullptr);
	if (res != 0)
	{
		klog("LoadModules(): sceUserServiceInitialize failed (%llX)\n", res);
		return false;
	}

	int libKernelHandle = sceKernelLoadStartModule("libkernel.sprx", 0, nullptr, 0, nullptr, nullptr);
	if (libKernelHandle > 0)
	{
		if (sceKernelDlsym(libKernelHandle, "sceKernelDebugOutText", (void**)&sceKernelDebugOutText) != 0)
		{
			klog("Failed to resolve: sceKernelDebugOutText\n");
			return false;
		}

		if (sceKernelDlsym(libKernelHandle, "sceKernelSendNotificationRequest", (void**)&sceKernelSendNotificationRequest) != 0)
		{
			klog("Failed to resolve: sceKernelSendNotificationRequest\n");
			return false;
		}

		if (sceKernelDlsym(libKernelHandle, "sceKernelGetModuleInfo", (void**)&sceKernelGetModuleInfo) != 0)
		{
			klog("Failed to resolve: sceKernelGetModuleInfo\n");
			return false;
		}
	}

	klog("LoadModules(): Success!\n");
	return true;
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

void Notify(const char* MessageFMT, ...)
{
	SceNotificationRequest Buffer;

	//Create full string from va list.
	va_list args;
	va_start(args, MessageFMT);
	vsprintf(Buffer.message, MessageFMT, args);
	va_end(args);

	//Populate the notify buffer.
	Buffer.type = SceNotificationRequestType::NotificationRequest; //this one is just a standard one and will print what ever is stored at the buffer.Message.
	Buffer.unk3 = 0;
	Buffer.useIconImageUri = 1; //Bool to use a custom uri.
	Buffer.targetId = -1; //Not sure if name is correct but is always set to -1.
	strcpy(Buffer.iconUri, "https://i.imgur.com/SJPIBGG.png"); //Copy the uri to the buffer.

	//From user land we can call int64_t sceKernelSendNotificationRequest(int64_t unk1, char* Buffer, size_t size, int64_t unk2) which is a libkernel import.
	sceKernelSendNotificationRequest(0, &Buffer, 3120, 0);
}

void NotifyCustom(const char* IconURI, const char* MessageFMT, ...)
{
	SceNotificationRequest Buffer;

	//Create full string from va list.
	va_list args;
	va_start(args, MessageFMT);
	vsprintf(Buffer.message, MessageFMT, args);
	va_end(args);

	//Populate the notify buffer.
	Buffer.type = SceNotificationRequestType::NotificationRequest; //this one is just a standard one and will print what ever is stored at the buffer.Message.
	Buffer.unk3 = 0;
	Buffer.useIconImageUri = 1; //Bool to use a custom uri.
	Buffer.targetId = -1; //Not sure if name is correct but is always set to -1.
	strcpy(Buffer.iconUri, IconURI); //Copy the uri to the buffer.

	//From user land we can call int64_t sceKernelSendNotificationRequest(int64_t unk1, char* Buffer, size_t size, int64_t unk2) which is a libkernel import.
	sceKernelSendNotificationRequest(0, &Buffer, 3120, 0);
}

bool CopySflash()
{
	int sflashFd = sceKernelOpen("/dev/sflash0", SCE_KERNEL_O_RDONLY, 0);
	int backupFd = sceKernelOpen("/data/Orbis Suite/sflash0", SCE_KERNEL_O_CREAT | SCE_KERNEL_O_WRONLY | SCE_KERNEL_O_APPEND, 0777);
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
