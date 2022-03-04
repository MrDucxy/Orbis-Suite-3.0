#pragma once
#include "../Common.hpp"

class kproc
{
private:
    bool Proc_Should_Run = false;
    
public:
    proc* Process = nullptr;
    thread* Main_Thread = nullptr;

    kproc(void(*Entry)(void*), void* Args, const char* Proc_Name, const char* Title_ID, int Flags, int Pages);
    ~kproc();

    thread* Add_Thread(void(*Thread)(void*), void *Args, int Flags, int Pages, const char* Thread_Name);
};
