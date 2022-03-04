#pragma once
#include "../Common.hpp"
#include "../Util/kproc.hpp"
#include "../Util/Proc.hpp"

class kproc;
class KDriver;

class OrbisLib
{
private:
    eventhandler_entry* SystemSuspendEvent;
    eventhandler_entry* SystemResumeEvent;
    eventhandler_entry* ProcessStartEvent;
    eventhandler_entry* ProcessExitEvent;
    Backup_Jail BackupJail;
    bool ShouldRun;
    
    static void OrbisLibKernelEntry(void* arg);

    static void OnSystemSuspend(void* arg);
    static void OnSystemResume(void* arg);
    static void OnProcessStart(void *arg, struct proc *p);
    static void OnProcessExit(void *arg, struct proc *p);

public:
    kproc* KprocInstance;
    KDriver* KDriverInstance;

    OrbisLib();
    ~OrbisLib();
};
