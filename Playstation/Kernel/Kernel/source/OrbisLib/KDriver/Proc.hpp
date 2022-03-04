#pragma once

class Proc
{
private:
    static int GetProccessList(caddr_t data, thread* td);
    static int GetProccessInfo(caddr_t data, thread* td);
    static int GetProccessModuleList(caddr_t data, thread* td);
    static int ProcessReadWrite(caddr_t data, thread* td);
    static int ProcessAlloc(caddr_t data, thread* td);
    static int ProcessFree(caddr_t data);
    static int ProcessSPRX(caddr_t data);
    static int ProcessELF(caddr_t data, thread* td);

public:
    static int OnIoctl(cdev* dev, unsigned long cmd, caddr_t data, int fflag, thread* td);
};
