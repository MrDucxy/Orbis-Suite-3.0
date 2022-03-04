#pragma once
#include "../../Common.hpp"
#include "DriverDefinitions.h"

class KDriver
{
private:
    cdev* m_Device;
    cdevsw m_DeviceSw;

    static int GetKDriverInfo(caddr_t data);
    
public:
    KDriver();
    ~KDriver();
    void OnProcessStart(void *arg, struct proc *p);
    static int OnIoctl(cdev* p_Device, unsigned long p_Command, caddr_t p_Data, int p_FFlag, thread* p_Thread);
};
