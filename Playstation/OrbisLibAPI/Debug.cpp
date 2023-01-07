#include "Common.h"
#include "Debug.h"
#include "APIHelper.h"
#include "GeneralIPC.h"
#include "Events.h"

#include <sys/ptrace.h>

#define HelperPrxPath "/data/Orbis Suite/OrbisLibGeneralHelper.sprx"

void Debug::HandleAPI(OrbisNetId Sock, APIPacket* Packet)
{
	if (Packet->Command > API_DBG_GET_CURRENT)
	{
		Sockets::SendInt(Sock, IsDebugging ? 1 : 0);
	}

	switch (Packet->Command)
	{
	default:
		break;

	case API_DBG_ATTACH:

		Attach(Sock);

		break;

	case API_DBG_DETACH:

		Detach(Sock);
		
		break;

	case API_DBG_GET_CURRENT:

		Sockets::SendInt(Sock, CurrentPID);

		break;

	case API_DBG_READ:

		break;

	case API_DBG_WRITE:

		break;

	case API_DBG_KILL:

		break;

		//...

	case API_DBG_LOAD_LIBRARY:

		LoadLibrary(Sock);

		break;

	case API_DBG_UNLOAD_LIBRARY:

		UnloadLibrary(Sock);

		break;

	case API_DBG_RELOAD_LIBRARY:

		ReloadLibrary(Sock);

		break;

	case API_DBG_LIBRARY_LIST:

		GetLibraryList(Sock);

		break;
	}
}

bool Debug::TryDetach(int pid)
{
	// Check if we are even attached.
	if (!IsDebugging)
	{
		return true;
	}

	// Detach from the process.
	int res = ptrace(PT_DETACH, pid, nullptr, 0);
	if (res != 0)
	{
		klog("DetachProcess(): ptrace(PT_DETACH) failed with error %llX\n", res);
		return false;
	}

	// TODO: Kill the watching thread and Remove any Watchpoints / Breakpoints now.

	IsDebugging = false;
	CurrentPID = -1;

	return true;
}

void Debug::Attach(OrbisNetId Sock)
{
	auto pid = RecieveInt(Sock);

	// Get Process name.
	char processName[32];
	sceKernelGetProcessName(pid, processName);

	// Aquire lock.
	scePthreadMutexLock(&DebugMutex);

	klog("Attempting to attach to %s (%d)\n", processName, pid);

	// If we are currently debugging another process lets detach from it.
	if (!TryDetach(pid))
	{
		// release the lock.
		scePthreadMutexUnlock(&DebugMutex);

		klog("Attach(): TryDetach Failed. :(\n");
		SendStatus(Sock, 0);
		return;
	}

	// Use ptrace to attach to begin debugging this pid.
	int res = ptrace(PT_ATTACH, pid, nullptr, 0);
	if (res != 0)
	{
		// release the lock.
		scePthreadMutexUnlock(&DebugMutex);

		klog("Attach(): ptrace(PT_ATTACH) failed with error %llX\n", res);
		SendStatus(Sock, 0);
		return;
	}

	sceKernelUsleep(500);

	// Attaching by default will stop execution of the remote process. Lets continue it now.
	res = ptrace(PT_CONTINUE, pid, (void*)1, 0);
	if (res != 0)
	{
		// release the lock.
		scePthreadMutexUnlock(&DebugMutex);

		klog("Attach(): ptrace(PT_CONTINUE) failed with error %llX\n", res);
		SendStatus(Sock, 0);
		return;
	}

	// Set current debugging state.
	IsDebugging = true;
	CurrentPID = pid;

	// release the lock.
	scePthreadMutexUnlock(&DebugMutex);

	// Send attach event to host.
	Events::SendEvent(Events::EVENT_ATTACH, pid);

	klog("Attached to %s(%d)\n", processName, pid);

	SendStatus(Sock, 1);

	if (strcmp(processName, "SceShellCore"))
	{
		// Get app info.
		OrbisAppInfo appInfo;
		sceKernelGetAppInfo(pid, &appInfo);

		// Get sandbox path.
		char sandBoxPath[PATH_MAX];
		snprintf(sandBoxPath, sizeof(sandBoxPath), "/mnt/sandbox/%s_000/data", appInfo.TitleId);

		// Mount data into sandbox
		LinkDir("/data/", sandBoxPath);
	}

	// Load the helper library.
	int handle = sys_sdk_proc_prx_load(processName, HelperPrxPath);
	klog("Helper handle = %llX\n", handle);
}

void Debug::Detach(OrbisNetId Sock)
{
	if(!IsDebugging)
		SendStatus(Sock, 0);

	// Aquire lock.
	scePthreadMutexLock(&DebugMutex);

	if (TryDetach(CurrentPID))
	{
		// release the lock.
		scePthreadMutexUnlock(&DebugMutex);

		Events::SendEvent(Events::EVENT_DETACH);

		SendStatus(Sock, 1);
	}
	else
	{
		// release the lock.
		scePthreadMutexUnlock(&DebugMutex);

		klog("Failed to detach from %d\n", CurrentPID);

		SendStatus(Sock, 0);
	}
}

void Debug::LoadLibrary(OrbisNetId Sock)
{
	if (!IsDebugging || CurrentPID == -1)
	{
		return;
	}

	// Get next packet.
	auto Packet = (DbgSPRXPacket*)malloc(sizeof(DbgSPRXPacket));
	sceNetRecv(Sock, Packet, sizeof(DbgSPRXPacket), 0);

	// Load the library.
	int handle = 0;
	GeneralIPC::LoadLibrary(CurrentPID, Packet->Path, &handle);

	// Send the result.
	Sockets::SendInt(Sock, handle);

	free(Packet);
}

void Debug::UnloadLibrary(OrbisNetId Sock)
{
	if (!IsDebugging || CurrentPID == -1)
	{
		return;
	}

	// Get next packet.
	auto Packet = (DbgSPRXPacket*)malloc(sizeof(DbgSPRXPacket));
	sceNetRecv(Sock, Packet, sizeof(DbgSPRXPacket), 0);

	// Unload the library.
	auto result = GeneralIPC::UnLoadLibrary(CurrentPID, Packet->Handle);

	// Send the result.
	Sockets::SendInt(Sock, result);

	free(Packet);
}

void Debug::ReloadLibrary(OrbisNetId Sock)
{
	if (!IsDebugging || CurrentPID == -1)
	{
		return;
	}

	// Get next packet.
	auto Packet = (DbgSPRXPacket*)malloc(sizeof(DbgSPRXPacket));
	sceNetRecv(Sock, Packet, sizeof(DbgSPRXPacket), 0);

	// Unload the library.
	auto result = GeneralIPC::UnLoadLibrary(CurrentPID, Packet->Handle);
	if (result != 0)
	{
		klog("Failed to unload %d\n", Packet->Handle);

		Sockets::SendInt(Sock, result);

		free(Packet);

		return;
	}

	// Wait a second.
	sceKernelSleep(1);

	// Load the library again.
	int handle = 0;
	GeneralIPC::LoadLibrary(CurrentPID, Packet->Path, &handle);

	// Send the result.
	Sockets::SendInt(Sock, handle);

	free(Packet);
}

void Debug::GetLibraryList(OrbisNetId Sock)
{
	if (!IsDebugging || CurrentPID == -1)
	{
		return;
	}

	// Get the library list with path.
	std::vector<LibraryPacket> libraryList;
	GeneralIPC::GetLibraryList(CurrentPID, libraryList);

	// Send the data size.
	Sockets::SendInt(Sock, libraryList.size());

	// Send the list to host.
	Sockets::SendLargeData(Sock, (unsigned char*)libraryList.data(), libraryList.size() * sizeof(LibraryPacket));
}

Debug::Debug()
{
	IsDebugging = false;
	CurrentPID = -1;

	// Create the mutex to protect our host list.
	auto res = scePthreadMutexInit(&DebugMutex, nullptr, "DebugMutex");
	if (res != 0)
	{
		klog("Error: Failed to create mutex for Debug class!! Err 0x%llX\n", res);
	}
}

Debug::~Debug()
{
	// Destroy for clean up.
	scePthreadMutexDestroy(&DebugMutex);
}