#include "../Common.hpp"
#include "kproc.hpp"

kproc::kproc(void(*Entry)(void*), void* Args, const char* Proc_Name, const char* Title_ID, int Flags, int Pages)
{
    //Create new Process.
    int ret = kproc_create(Entry, Args, &this->Process, Flags, Pages, Proc_Name);
    if( ret != 0 || this->Process == nullptr )
    {
        kprintf("Failed to create new Process.\n");
        return;
    }

    //Copy Title ID Text.
    strcpy(this->Process->titleId, (char*)Title_ID);

    //Help with graceful shutdown.
    Proc_Should_Run = true;

    kprintf("Created New Process Sucessfully.\n%s(%i) %s", this->Process->p_comm, this->Process->p_pid, this->Process->titleId);
}

kproc::~kproc()
{

}

thread* kproc::Add_Thread(void(*Thread)(void*), void *Args, int Flags, int Pages, const char* Thread_Name)
{
    thread* NewThread = nullptr;
    int ret = kproc_kthread_add(Thread, Args, &this->Process, &NewThread, Flags, Pages, this->Process->p_comm, Thread_Name);

    if(ret != 0 || NewThread == nullptr)
    {
        kprintf("Failed to Create New Thread (%s) on Proc %s(%i) %s", Thread_Name, this->Process->p_comm, this->Process->p_pid, this->Process->titleId);
        return nullptr;
    }

    kprintf("New Thread (%s) on Proc %s(%i) %s", Thread_Name, this->Process->p_comm, this->Process->p_pid, this->Process->titleId);

    return NewThread;
}