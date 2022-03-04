#include "../../Common.hpp"
#include "Kernel.hpp"

#include "DriverDefinitions.h"
#include "../../Util/System.hpp"

int Kernel::OnIoctl(cdev* dev, unsigned long cmd, caddr_t data, int fflag, thread* td)
{
    switch(cmd)
    {

    default:
        klog("Not Implimented. :(");
        break;
    }
}