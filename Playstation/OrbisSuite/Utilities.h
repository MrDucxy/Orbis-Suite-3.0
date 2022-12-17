#pragma once
#include <sys/uio.h>

#define	MNT_UPDATE	0x00010000

//Modules.
extern void(*_sceSysmoduleLoadModuleInternal)(uint32_t); //Import is broken for some reason
bool LoadModules();

//Misc
void Notify(const char* MessageFMT, ...);
void klog(const char* fmt, ...);
void InstallDaemon(const char* Daemon);
void InstallOrbisToolbox();
void InstallOrbisSuite();

//File IO.
static void build_iovec(iovec** iov, int* iovlen, const char* name, const void* val, size_t len);
int nmount(struct iovec *iov, uint32_t niov, int flags);
int unmount(const char *dir, int flags);
int mount_large_fs(const char* device, const char* mountpoint, const char* fstype, const char* mode, unsigned int flags);
void CopyFile(const char* File, const char* Destination);
int MakeDir(char* Dir, ...);