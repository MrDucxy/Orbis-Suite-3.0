#include "Common.h"
#include "Version.h"
#include "API.h"
#include "GoldHEN.h"
#include "Events.h"
#include <orbis/NpManager.h>
#include "ThreadPool.h"

typedef unsigned char vm_prot_t;	/* protection codes */

#define	VM_PROT_NONE		((vm_prot_t) 0x00)
#define	VM_PROT_READ		((vm_prot_t) 0x01)
#define	VM_PROT_WRITE		((vm_prot_t) 0x02)
#define	VM_PROT_EXECUTE		((vm_prot_t) 0x04)
#define	VM_PROT_COPY		((vm_prot_t) 0x08)	/* copy-on-read */

#define	VM_PROT_ALL		(VM_PROT_READ|VM_PROT_WRITE|VM_PROT_EXECUTE)
#define VM_PROT_RW		(VM_PROT_READ|VM_PROT_WRITE)
#define	VM_PROT_DEFAULT		VM_PROT_ALL

void exiting()
{
	klog("Good bye friends! :)\n");

	// Terminate System Monitor
	SystemMonitor::Term();

	// Terminate the Events class.
	Events::Term();

	// Terminate API
	API::Term();

	// Terminate Thread Pool
	ThreadPool::Term();
}

int main()
{
	// Jailbreak our current process.
	if (!Jailbreak())
	{
		Notify("Failed to jailbreak Process...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Load internal system modules.
	if (!LoadModules())
	{
		Notify("Failed to Load Modules...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}

	// Call cleanup on exit.
	// TODO: Find working way to handle this.

	// Copy back up of sflash so we can read it and not break things :)
	CopySflash();
	
	// Set the Name of this process so it shows up as something other than eboot.bin.
	sceKernelSetProcessName("OrbisLibAPI");
	
	// Log the loaded version string.
	klog("\n%s\n\n", ORBISLIB_BUILDSTRING);
	
	// Start up the thread pool.
	ThreadPool::Init(20);
	
	// Init a thread to monitor the system usage stats.
	// SystemMonitor::Init();
	
	// Set up the events.
	if (!Events::Init())
	{
		Notify("Failed to init Events...");
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}
	
	// Load the tool box.
	if (!LoadToolbox())
	{
		sceSystemServiceLoadExec("exit", 0);
		return 0;
	}
	
	// start up the API. NOTE: this is blocking.
	API::Init();

	sceSystemServiceLoadExec("exit", 0);

    return 0;
}
