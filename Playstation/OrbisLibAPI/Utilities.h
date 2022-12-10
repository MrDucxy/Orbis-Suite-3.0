#pragma once

typedef struct SceDbgModuleSegmentInfo {
    void* baseAddr;
    uint32_t size;
    int32_t prot;
} SceDbgModuleSegmentInfo;

struct SceDbgModuleInfo {
    uint64_t size;
    char name[256];
    SceDbgModuleSegmentInfo segmentInfo[4];
    uint32_t numSegments;
    uint8_t fingerprint[20];
};

// Modules.
extern void(*_sceSysmoduleLoadModuleInternal)(uint32_t); //Import is broken for some reason
bool LoadModules();

// Misc
void Notify(const char* MessageFMT, ...);
void Notify_Custom(const char* IconURI, const char* MessageFMT, ...);
void klog(const char* fmt, ...);
int sys_dynlib_get_info(int moduleHandle, SceDbgModuleInfo* destModuleInfo);
int sys_dynlib_get_list(int* destModuleHandles, int max, int* count);
bool Jailbreak();
bool CopySflash();
int getMacAddress(SceNetIfName ifName_Num, char* strOut, size_t strlen);

// Networking
bool SockSendInt(OrbisNetId Sock, int val);
bool SockRecvInt(OrbisNetId Sock, int* val);
