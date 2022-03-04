#pragma once

#if defined(__cplusplus)
extern "C" {
#endif

#if defined(KERNELDRIVER)
#include <sys/ioccom.h>
#else
#include <sys/ioctl.h>
#define	IOC_VOID	0x20000000	/* no parameters */
#define	IOC_OUT		0x40000000	/* copy out parameters */
#define	IOC_IN		0x80000000	/* copy in parameters */
#define	IOC_INOUT	(IOC_IN|IOC_OUT)
#endif

#if defined(__cplusplus)
};
#endif

enum KDriverCommands
{
    /* ######## Proc Commands ####### */
    CMD_PROC_LIST,
    CMD_PROC_INFO,
    CMD_PROC_MODULE_LIST,
    CMD_PROC_READ_WRITE_MEMORY,
    CMD_PROC_ALLOC_MEMORY,
    CMD_PROC_FREE_MEMORY,
    CMD_PROC_SPRX,
    CMD_PROC_ELF,
    /* ############################## */

    /* ###### Kernel Commands ####### */
    CMD_KERN_READ_WRITE_MEMORY,
    /* ############################## */

    /* ###### KDriver Commands ###### */
    CMD_KDRIVER_INFO,
    /* ############################## */
};

// Process Commands
struct KDriver_ProcList
{
    uint64_t UserlandAddr;
    size_t UserlandSize;
    int ProcCount;
};

struct ProcInfo
{
    int PID;
    bool Attached;
    char ProcName[32];
    char TitleID[10];
    uint64_t TextSegmentBase;
    uint64_t TextSegmentLen;
    uint64_t DataSegmentBase;
    uint64_t DataSegmentLen;
};

struct KDriver_ProcSPRX
{
    int CallType;
    char ProcName[32];
    int Handle;
    char Path[4096];
    bool CallEntryExit;
};

struct KDriver_ProcInfo
{
    int ProcessID;
    uint64_t UserlandAddr;
    size_t UserlandSize;
    int ThreadCount;
};

struct ProcInfoExt
{
    struct ThreadInfo
    {
        int ThreadId;
        char Name[36];
        int Errno;
        int RetVal;
    };

    int ProcessID;
    int Attached;
    int Signal;
    int Code;
    int Stops;
    int StopType;
    char ProcName[32];
    char TitleID[10];
    char ElfPath[1024];
    char RandomizedPath[256];
    uint64_t TextSegmentBase;
    uint64_t TextSegmentLen;
    uint64_t DataSegmentBase;
    uint64_t DataSegmentLen;

    int ThreadCount;
    ThreadInfo Threads[];
};

struct KDriver_ModuleList
{
    int ProcessID;
    uint64_t UserlandAddr;
    size_t UserlandSize;
    int ModuleCount;
};

struct ModuleInfo
{   
    int Handle;
    char Name[36];
    char Path[256];
    uint64_t TextSegmentBase;
    uint64_t TextSegmentLen;
    uint64_t DataSegmentBase;
    uint64_t DataSegmentLen;
};

struct KDriver_ReadWriteMemory
{
    int ProcessID;
    bool IsRead;
    uint64_t ProcessAddress;
    uint64_t UserlandAddr;
    size_t Length;
};

struct KDriver_AllocFreeMemory
{
    int ProcessID;
    size_t Ammount;
    uint64_t ProcessAddress;
};

struct KDriver_ProcELF
{
    int ProcessID;
    uint64_t ELFAddress;
    size_t ELFSize;
};

#define PROC_LIST _IOC(IOC_INOUT, 'P', (uint32_t)(KDriverCommands::CMD_PROC_LIST), sizeof(KDriver_ProcList))
#define PROC_INFO _IOC(IOC_INOUT, 'P', (uint32_t)(KDriverCommands::CMD_PROC_INFO), sizeof(KDriver_ProcInfo))
#define PROC_MODULE_LIST _IOC(IOC_INOUT, 'P', (uint32_t)(KDriverCommands::CMD_PROC_MODULE_LIST), sizeof(KDriver_ModuleList))
#define PROC_READ_WRITE_MEMORY _IOC(IOC_INOUT, 'P', (uint32_t)(KDriverCommands::CMD_PROC_READ_WRITE_MEMORY), sizeof(KDriver_ReadWriteMemory))
#define PROC_ALLOC_MEMORY _IOC(IOC_INOUT, 'P', (uint32_t)(KDriverCommands::CMD_PROC_ALLOC_MEMORY), sizeof(KDriver_AllocFreeMemory))
#define PROC_FREE_MEMORY _IOC(IOC_INOUT, 'P', (uint32_t)(KDriverCommands::CMD_PROC_FREE_MEMORY), sizeof(KDriver_AllocFreeMemory))
#define PROC_SPRX _IOC(IOC_INOUT, 'P', (uint32_t)(KDriverCommands::CMD_PROC_SPRX), sizeof(KDriver_ProcSPRX))
#define PROC_ELF _IOC(IOC_INOUT, 'P', (uint32_t)(KDriverCommands::CMD_PROC_ELF), sizeof(KDriver_ProcELF))

// Kernel Commands
#define KERN_READ_WRITE_MEMORY _IOC(IOC_INOUT, 'K', (uint32_t)(KDriverCommands::CMD_KERN_READ_WRITE_MEMORY), sizeof(KDriver_ReadWriteMemory))

// Kdriver Commands
struct KDriver_Info
{
    int MajorVersion;
    int MinorVersion;
    int BuildVersion;
    bool Running;
    int(*Shutdown)();
    int(*Entry)(void* p);
    void* ELFBase;
    int Size;
};

#define KDRIVER_INFO _IOC(IOC_OUT, 'D', (uint32_t)(KDriverCommands::CMD_KDRIVER_INFO), sizeof(KDriver_Info))
