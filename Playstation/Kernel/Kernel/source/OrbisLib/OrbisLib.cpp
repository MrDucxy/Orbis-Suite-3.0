#include "../Common.hpp"
#include "OrbisLib.hpp"
#include "../Util/ShellCode/SPRXShellCode.hpp"
#include "../Util/System.hpp"

void OrbisLib::OrbisLibKernelEntry(void* arg)
{
    auto OrbisLibInstance = (OrbisLib*)arg;

    proc_Jailbreak(curthread()->td_proc, &OrbisLibInstance->BackupJail);

    //LoadSPRX("SceShellUI", "/data/Orbis Toolbox/Orbis Toolbox.sprx");

    // Set up Kernel Instances...
    OrbisLibInstance->KDriverInstance = new KDriver();

    // Block to keep alive.
    while(OrbisLibInstance->ShouldRun)
    {
        kthread_suspend_check();
        Sleep(100);
    }

    kproc_exit(0);
}

OrbisLib::OrbisLib()
{
    // Set Initial Vars.
    ShouldRun = true;

    // Start up Kernel Process.
    KprocInstance = new kproc(OrbisLibKernelEntry, this, "OrbisSuite Kernel", "ORBS00001", 0, 0);

    // Register Events.
    SystemSuspendEvent = EVENTHANDLER_REGISTER(system_suspend_phase1, (void*)OnSystemSuspend, nullptr, EVENTHANDLER_PRI_FIRST);
    SystemResumeEvent = EVENTHANDLER_REGISTER(system_resume_phase1, (void*)OnSystemResume, nullptr, EVENTHANDLER_PRI_FIRST);
    ProcessStartEvent = EVENTHANDLER_REGISTER(process_exec_end, (void*)OnProcessStart, nullptr, EVENTHANDLER_PRI_ANY);
    ProcessExitEvent = EVENTHANDLER_REGISTER(process_exit, (void*)OnProcessExit, nullptr, EVENTHANDLER_PRI_ANY);

    // Do the thing.
    DoShellUIMount(FindProcessByName("SceShellUI"), true);
}

OrbisLib::~OrbisLib()
{
    // Teardown Kernel Process.
    ShouldRun = false;

    // De-Register Events.
    EVENTHANDLER_DEREGISTER(system_suspend_phase1, SystemSuspendEvent);
    EVENTHANDLER_DEREGISTER(system_resume_phase1, SystemResumeEvent);
    EVENTHANDLER_DEREGISTER(process_exec_end, ProcessStartEvent);
    EVENTHANDLER_DEREGISTER(process_exit, ProcessExitEvent);

    // Clean up.
    delete KDriverInstance;
}

void OrbisLib::OnSystemSuspend(void* arg)
{
    klog("System is Suspending...");

}

void OrbisLib::OnSystemResume(void* arg)
{
    klog("System is Resuming...");

}

void OrbisLib::OnProcessStart(void *arg, struct proc *p)
{
    if(!p)
        return;

    klog("OnProcessStart: %s(%s)", p->p_comm, p->titleId);

    DoShellUIMount(p, true);
}

void OrbisLib::OnProcessExit(void *arg, struct proc *p)
{
    if(!p)
        return;

    klog("OnProcessExit: %s(%s)", p->p_comm, p->titleId);

    DoShellUIMount(p, false);
}