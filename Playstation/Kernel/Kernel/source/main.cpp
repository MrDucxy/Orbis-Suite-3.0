#include "Common.hpp"
#include "OrbisLib/OrbisLib.hpp"
#include "Util/Patches/Patches.hpp"

KDriver_Info* KDriverInfo = nullptr;
OrbisLib* OrbisLibInstance = nullptr;

extern "C" int _Shutdown()
{
	klog("!!! SHUTDOWN SHUTDOWN SHUTDOWN !!!");

	KDriverInfo->Running = false;

    delete OrbisLibInstance;

	Sleep(2000);

	klog("!!! BYE !!!");

	return 0;
}

extern "C" int _main(uint64_t* p)
{ 
	ResolveFunctions();
	Install_Patches();

	KDriverInfo = new KDriver_Info();

	// Copy in our start up info.
	if(p != nullptr)
		memcpy(KDriverInfo, p, sizeof(KDriver_Info));

	// Set up Driver info.
	KDriverInfo->MajorVersion = KDRIVER_MAJOR_VERSION;
	KDriverInfo->MinorVersion = KDRIVER_MINOR_VERSION;
	KDriverInfo->BuildVersion = KDRIVER_BUILD_VERSION;
	KDriverInfo->Shutdown = &_Shutdown;

	// Start up main Kdriver Class.
    OrbisLibInstance = new OrbisLib();

	klog("Hello from Kernel\nKDriver %s v%d.%d.%d", SOFTWARE_VERSION_STR, KDRIVER_MAJOR_VERSION, KDRIVER_MINOR_VERSION, KDRIVER_BUILD_VERSION);

	// Set our running state.
	KDriverInfo->Running = true;

	return 0;
}
