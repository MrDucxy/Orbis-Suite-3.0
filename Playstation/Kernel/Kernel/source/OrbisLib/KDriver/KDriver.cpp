#include "../../Common.hpp"
#include "KDriver.hpp"
#include "DriverDefinitions.h"

#include "Proc.hpp"
#include "Kernel.hpp"

KDriver::KDriver()
{
	m_DeviceSw.d_version = D_VERSION;
    m_DeviceSw.d_name = "OrbisSuite";
    m_DeviceSw.d_ioctl = OnIoctl;

	int ret = make_dev_p(MAKEDEV_CHECKNAME | MAKEDEV_WAITOK, 
		&m_Device,
        &m_DeviceSw,
        nullptr,
        UID_ROOT,
        GID_WHEEL,
        S_IRWXU | S_IRWXG | S_IRWXO,
        "OrbisSuite");

	if (ret == 0)
		klog("device driver created successfully!");
	else if (ret == EEXIST)
		klog("could not create device driver, device driver already exists.");
	else
		klog("could not create device driver (%d).", ret);
}

KDriver::~KDriver()
{
    destroy_dev(m_Device);
}

void KDriver::OnProcessStart(void *arg, struct proc *p)
{
	
}

int KDriver::OnIoctl(cdev* dev, unsigned long cmd, caddr_t data, int fflag, thread* td)
{
	cmd = cmd & 0xFFFFFFFF; // Clear the upper32
	auto group = IOCGROUP(cmd);

	klog("Group: %c Cmd: %i", group, cmd);

	switch (group)
	{
	case 'P':
		return Proc::OnIoctl(dev, cmd, data, fflag, td);

	case 'K':
		return Kernel::OnIoctl(dev, cmd, data, fflag, td);

	case 'D':
		switch (cmd)
		{
		case KDRIVER_INFO:
			return GetKDriverInfo(data);

		default:
			klog("[KDriver] Not Implimented. :(");
			break;
		}
		break;

	default:
		klog("[KDriver] Not Implimented. :(");
		break;
	}

	return 0;
}

int KDriver::GetKDriverInfo(caddr_t data)
{
	if (data == nullptr)
    {
        klog("Data pointer invalid...");
        return EINVAL;
    }

	if(KDriverInfo == nullptr)
	{
		klog("KDriverInfo pointer invalid...");
		return EINVAL;
	}

	int res = copyout(KDriverInfo, data, sizeof(KDriver_Info));
	if(res != 0)
	{
		klog("Failed to copy out data.");
		return res;
	}

	return 0;
}