#include "../../Common.hpp"
#include "SPRXShellCode.hpp"
#include "../System.hpp"
#include "../Proc.hpp"

extern char _binary_Resources_LoaderShellCode_bin_start[];
extern char _binary_Resources_LoaderShellCode_bin_end[];

extern char _binary_Resources_UnLoaderShellCode_bin_start[];
extern char _binary_Resources_UnLoaderShellCode_bin_end[];

int LoadSPRX(const char* ProcessName, const char* Path, bool ShouldCallEntry)
{
    klog("LoadSPRX(%s, %s)", ProcessName, Path);

    uint64_t thr_initial = 0;
    uint8_t ShellCodeComplete = 0;
    uint64_t ModuleHandle = 0;

    auto Process = FindProcessByName(ProcessName);
    if(Process == nullptr)
    {
        klog("LoadSPRX(): Could not find process \"%s\".", ProcessName);

        return -1;
    }

    auto ProcessThread = TAILQ_FIRST(&(Process)->p_threads);
    if(ProcessThread == nullptr)
    {
        klog("LoadSPRX(): Could not find thread on process \"%s\".", ProcessName);

        return -1;
    }

    if(Process->p_dynlibptr == nullptr) 
    {
        klog("LoadSPRX(): p_dynlibptr returned nullptr.");
        return -1;
    }

    // Jailbreak the process.
    Backup_Jail bkJail;
    proc_Jailbreak(Process, &bkJail);

    // Gets the Thread Initial for the shellcore thread. Also checking to make sure were not trying to load a prx already loaded.
    dynlib* m_library = Process->p_dynlibptr->p_dynlib;
    while(m_library != 0)
	{
        if(!strcmp(basename(m_library->ModulePath), basename(Path)))
        {
            klog("LoadSPRX(): Module %s is already loaded on proc %s...", basename(Path), Process->p_comm);

            // Restore previous jail.
            proc_RestoreJail(Process, bkJail);

            return m_library->ModuleHandle;
        }

        if(!strcmp(basename(m_library->ModulePath), "libkernel.sprx"))
			thr_initial = (uint64_t)m_library->codeBase + addr_thr_initial_libkernel;

        if(!strcmp(basename(m_library->ModulePath), "libkernel_web.sprx"))
			thr_initial = (uint64_t)m_library->codeBase + addr_thr_initial_libkernel_web;

        if(!strcmp(basename(m_library->ModulePath), "libkernel_sys.sprx"))
			thr_initial = (uint64_t)m_library->codeBase + addr_thr_initial_libkernel_sys;

        m_library = m_library->dynlib_next;
    }

    if(thr_initial == 0) 
    {
		klog("LoadSPRX(): Failed to resolve thr_initial.");

        // Restore previous jail.
        proc_RestoreJail(Process, bkJail);

        return -1;
    }

    auto ShellCodeSize = (size_t)(_binary_Resources_LoaderShellCode_bin_end - _binary_Resources_LoaderShellCode_bin_start);

    klog("Start: %llX\nEnd %llX\nSize: 0x%X", 
    _binary_Resources_LoaderShellCode_bin_start,
    _binary_Resources_LoaderShellCode_bin_end,
    ShellCodeSize);

    // Allocate space on the process for the shellcode and its threads stack.
    auto UserlandShellCode = kmap(nullptr, ShellCodeSize, PROT_READ | PROT_WRITE | PROT_EXEC, MAP_ANON | MAP_PREFAULT_READ, -1, 0, ProcessThread);
    if(UserlandShellCode == nullptr || (uint64_t)UserlandShellCode < 0)
    {
        klog("LoadSPRX(): Failed to allocate memory on process for ShellCode. Err: %d", UserlandShellCode);

        // Restore previous jail.
        proc_RestoreJail(Process, bkJail);

        return -1;
    }

    klog("LoadSPRX(): thr_initial = 0x%llX", thr_initial);
    klog("LoadSPRX(): ShellCodeSize = 0x%llX", ShellCodeSize);
    klog("LoadSPRX(): UserlandShellCode = 0x%llX", UserlandShellCode);
    klog("LoadSPRX(): UserlandShellCode = %d", UserlandShellCode);

    auto ShellCodeHeader = (OrbisLoader_header*)_binary_Resources_LoaderShellCode_bin_start;
    ShellCodeHeader->ShellCodeComplete = 0;
    ShellCodeHeader->ModuleHandle = 0;
    ShellCodeHeader->ShouldCallEntry = ShouldCallEntry;
    ShellCodeHeader->thr_initial = thr_initial;
    strcpy(ShellCodeHeader->Path, (char*)Path);

    // Write the shellcode to the allocated memory on the process.
    auto res = ReadWriteProcessMemory(Process, (void*)UserlandShellCode, (void*)_binary_Resources_LoaderShellCode_bin_start, ShellCodeSize, true);
    if(!res)
    {
        klog("LoadSPRX(): WriteProcessMemory failed with error %d.", res);

        kmunmap(UserlandShellCode, ShellCodeSize, ProcessThread);

        // Restore previous jail.
        proc_RestoreJail(Process, bkJail);

        return -1;
    }

    // Allocate memory on process for the threads stack.
    auto StackMemory = kmap(nullptr, 0x80000, PROT_READ | PROT_WRITE | PROT_EXEC, MAP_ANON | MAP_PREFAULT_READ, -1, 0, ProcessThread);// AllocateProcessMemory(Process, 0x80000, VM_PROT_ALL);
    if(StackMemory == nullptr)
    {
        klog("LoadSPRX(): Failed to allocate memory on process for the Stack.");

        kmunmap(UserlandShellCode, ShellCodeSize, ProcessThread);

        // Restore previous jail.
        proc_RestoreJail(Process, bkJail);

        return -1;
    }

    // Start a thread with the entry pointing to +0x4 in our shellcode because that holds the offset to the starting instructions.

    klog("LoadSPRX(): Starting Shellcode Thread...");
    struct thread *thr = TAILQ_FIRST(&Process->p_threads);
	uint64_t ShellCodeEntry = (uint64_t)UserlandShellCode + ShellCodeHeader->entry;
	create_thread(thr, NULL, (void*)ShellCodeEntry, NULL, (char*)StackMemory, 0x80000, NULL, NULL, NULL, 0, NULL);

    klog("LoadSPRX(): Thread Started!! Waiting for shellcode to complete...");

    // Wait for the shellcode to complete by reading the byte that will be set to 1 on completion.
    while (!ShellCodeComplete) 
	{
        auto err = ReadProcessMemory(Process, UserlandShellCode + offsetof(OrbisLoader_header, ShellCodeComplete), (void *)&ShellCodeComplete, sizeof(ShellCodeComplete));
        if(!err)
        {
            klog("LoadSPRX(): Failed to read ModuleHandle. %d", err);

            kmunmap(StackMemory, 0x80000, ProcessThread);
            kmunmap(UserlandShellCode, ShellCodeSize, ProcessThread);

            // Restore previous jail.
            proc_RestoreJail(Process, bkJail);

            return -1;
        }

        pause("", 100);
    }

    // Grab the Module handle from the shellcode to see if the loading was a sucess or not.
    auto err = ReadProcessMemory(Process, UserlandShellCode + offsetof(OrbisLoader_header, ModuleHandle), (void *)&ModuleHandle, sizeof(ModuleHandle));
    if(!err)
    {
        klog("LoadSPRX(): Failed to read ModuleHandle. %d", err);

        kmunmap(StackMemory, 0x80000, ProcessThread);
        kmunmap(UserlandShellCode, ShellCodeSize, ProcessThread);

        return -1;
    }

    // Clean up.
    klog("LoadSPRX(): Freeing Shellcode Memory...");
    kmunmap(StackMemory, 0x80000, ProcessThread);
    kmunmap(UserlandShellCode, ShellCodeSize, ProcessThread);

    // Restore previous jail.
    proc_RestoreJail(Process, bkJail);

    if(ModuleHandle > 0 && ModuleHandle < 0x80000000)
        klog("LoadSPRX(): Completed! Module Loaded with handle 0x%llX", ModuleHandle);
    else
        klog("LoadSPRX(): Failed with error 0x%llX", ModuleHandle);

    return ModuleHandle;
}

int UnloadSPRX(const char* ProcessName, int Handle, bool ShouldCallExit)
{
    klog("UnloadSPRX(%s, %i)", ProcessName, Handle);

    uint64_t thr_initial = 0;
    uint8_t ShellCodeComplete = 0;
    uint64_t Result = 0;

    auto Process = FindProcessByName(ProcessName);
    if(Process == nullptr)
    {
        klog("UnloadSPRX(): Could not find process \"%s\".", ProcessName);

        return -1;
    }

    auto ProcessThread = TAILQ_FIRST(&(Process)->p_threads);
    if(ProcessThread == nullptr)
    {
        klog("UnloadSPRX(): Could not find thread on process \"%s\".", ProcessName);

        return -1;
    }

    if(Process->p_dynlibptr == nullptr) 
    {
        klog("UnloadSPRX(): p_dynlibptr returned nullptr.");
        return -1;
    }

    // Gets the Thread Initial for the shellcore thread. Also checking to make sure were not trying to un load a prx that is not loaded.
    bool FoundModule = false;
    dynlib* m_library = Process->p_dynlibptr->p_dynlib;
    while(m_library != 0)
	{
        if(m_library->ModuleHandle == Handle)
        {
            klog("UnloadSPRX(): Found Module \"%s\" in Process \"%s\".", basename(m_library->ModulePath), ProcessName);

            FoundModule = true;
        }

        if(!strcmp(basename(m_library->ModulePath), "libkernel.sprx"))
			thr_initial = (uint64_t)m_library->codeBase + addr_thr_initial_libkernel;

        if(!strcmp(basename(m_library->ModulePath), "libkernel_web.sprx"))
			thr_initial = (uint64_t)m_library->codeBase + addr_thr_initial_libkernel_web;

        if(!strcmp(basename(m_library->ModulePath), "libkernel_sys.sprx"))
			thr_initial = (uint64_t)m_library->codeBase + addr_thr_initial_libkernel_sys;

        m_library = m_library->dynlib_next;
    }

    if(!FoundModule)
    {
        klog("UnloadSPRX(): Could not find Module %i on Process \"%s\".", Handle, ProcessName);

        return -1;
    }

    if(thr_initial == 0) 
    {
		klog("UnloadSPRX(): Failed to resolve thr_initial.");

        return -1;
    }

    auto ShellCodeSize = (size_t)(_binary_Resources_UnLoaderShellCode_bin_end - _binary_Resources_UnLoaderShellCode_bin_start);

    klog("Start: %llX\nEnd %llX\nSize: 0x%X", 
    _binary_Resources_UnLoaderShellCode_bin_start,
    _binary_Resources_UnLoaderShellCode_bin_end,
    ShellCodeSize);

    // Allocate space on the process for the shellcode and its threads stack.
    auto UserlandShellCode = kmap(nullptr, ShellCodeSize, PROT_READ | PROT_WRITE | PROT_EXEC, MAP_ANON | MAP_PREFAULT_READ, -1, 0, ProcessThread);// AllocateProcessMemory(Process, ShellCodeSize, VM_PROT_ALL);
    if(UserlandShellCode == nullptr)
    {
        klog("UnloadSPRX(): Failed to allocate memory on process for ShellCode.");

        return -1;
    }

    klog("UnloadSPRX(): thr_initial = 0x%llX", thr_initial);
    klog("UnloadSPRX(): UserlandShellCode = 0x%llX", UserlandShellCode);

    auto ShellCodeHeader = (OrbisUnLoader_header*)_binary_Resources_UnLoaderShellCode_bin_start;
    ShellCodeHeader->thr_initial = thr_initial;
    ShellCodeHeader->ShellCodeComplete = 0;
    ShellCodeHeader->ShouldCallExit = ShouldCallExit;
    ShellCodeHeader->ModuleHandle = Handle;

    // Write the shellcode to the allocated memory on the process.
    auto res = ReadWriteProcessMemory(Process, (void*)UserlandShellCode, (void*)_binary_Resources_UnLoaderShellCode_bin_start, ShellCodeSize, true);
    if(!res)
    {
        klog("UnloadSPRX(): copy out failed with error %d.", res);

        kmunmap(UserlandShellCode, ShellCodeSize, ProcessThread);

        return -1;
    }

    // Allocate memory on process for the threads stack.
    auto StackMemory = kmap(nullptr, 0x80000, PROT_READ | PROT_WRITE | PROT_EXEC, MAP_ANON | MAP_PREFAULT_READ, -1, 0, ProcessThread);// AllocateProcessMemory(Process, 0x80000, VM_PROT_ALL);
    if(StackMemory == nullptr)
    {
        klog("UnloadSPRX(): Failed to allocate memory on process for the Stack.");

        kmunmap(UserlandShellCode, ShellCodeSize, ProcessThread);

        return -1;
    }

    // Start a thread with the entry pointing to +0x4 in our shellcode because that holds the offset to the starting instructions.

    klog("UnloadSPRX(): Starting Shellcode Thread...");
    struct thread *thr = TAILQ_FIRST(&Process->p_threads);
	uint64_t ShellCodeEntry = (uint64_t)UserlandShellCode + ShellCodeHeader->entry;
	create_thread(thr, NULL, (void*)ShellCodeEntry, NULL, (char*)StackMemory, 0x80000, NULL, NULL, NULL, 0, NULL);

    klog("UnloadSPRX(): Thread Started!! Waiting for shellcode to complete...");

    // Wait for the shellcode to complete by reading the byte that will be set to 1 on completion.
    while (!ShellCodeComplete) 
	{
        auto err = ReadProcessMemory(Process, UserlandShellCode + offsetof(OrbisUnLoader_header, ShellCodeComplete), (void *)&ShellCodeComplete, sizeof(ShellCodeComplete));
        if(!err)
        {
            klog("UnloadSPRX(): Failed to read ModuleHandle. %d", err);

            kmunmap(StackMemory, 0x80000, ProcessThread);
            kmunmap(UserlandShellCode, ShellCodeSize, ProcessThread);

            return -1;
        }

        pause("", 100);
    }

    // Grab the Result from the shellcode to see if the Un Loading was a sucess or not.
    auto err = ReadProcessMemory(Process, UserlandShellCode + offsetof(OrbisUnLoader_header, Result), (void *)&Result, sizeof(Result));
    if(!err)
    {
        klog("UnloadSPRX(): Failed to read Result. %d", err);

        kmunmap(StackMemory, 0x80000, ProcessThread);
        kmunmap(UserlandShellCode, ShellCodeSize, ProcessThread);

        return -1;
    }

    // Clean up.
    klog("UnloadSPRX(): Freeing Shellcode Memory...");
    kmunmap(StackMemory, 0x80000, ProcessThread);
    kmunmap(UserlandShellCode, ShellCodeSize, ProcessThread);

    return Result;
}

int UnloadSPRX(const char* ProcessName, const char* Name, bool ShouldCallExit)
{
    auto Process = FindProcessByName(ProcessName);
    if(Process == nullptr)
    {
        klog("UnloadSPRX(): Could not find process \"%s\".", ProcessName);

        return -1;
    }

    klog("Found Process...\n");

    auto ProcessThread = TAILQ_FIRST(&(Process)->p_threads);
    if(ProcessThread == nullptr)
    {
        klog("UnloadSPRX(): Could not find thread on process \"%s\".", ProcessName);

        return -1;
    }

    klog("Found Thread...\n");

    if(Process->p_dynlibptr == nullptr) 
    {
        klog("UnloadSPRX(): p_dynlibptr returned nullptr.");
        return -1;
    }

    klog("Found Thread...\n");

    int ModuleHandle = -1;
    dynlib* m_library = Process->p_dynlibptr->p_dynlib;
    while(m_library != 0)
	{
        if(!strcmp(basename(m_library->ModulePath), Name))
        {
            klog("UnloadSPRX(): Found Module \"%s\" in Process \"%s\".", Name, ProcessName);

            ModuleHandle = m_library->ModuleHandle;

            break;
        }

        m_library = m_library->dynlib_next;
    }

    if(ModuleHandle == -1)
    {
        klog("UnloadSPRX(): Could not find Module \"%s\" on Process \"%s\".", Name, ProcessName);

        return -1;
    }

    return UnloadSPRX(ProcessName, ModuleHandle, ShouldCallExit);
}