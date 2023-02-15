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

		ReadWriteMemory(Sock, false);

		break;

	case API_DBG_WRITE:

		ReadWriteMemory(Sock, true);

		break;

	case API_DBG_KILL:

		break;

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
		// Check if proc is dead anyway and just detach.

		klog("DetachProcess(): ptrace(PT_DETACH) failed with error %llX\n", res);
		return false;
	}

	// Reset vars.
	IsDebugging = false;
	CurrentPID = -1;

	// Wait for the current proc thread to die.
	scePthreadJoin(ProcMonitorThreadHandle, nullptr);

	return true;
}

void* Debug::ProcessMonotorThread()
{
	while (IsDebugging && CurrentPID != -1)
	{
		std::vector<kinfo_proc> procList;
		GetProcessList(procList);

		int currentPID = CurrentPID;
		if (std::find_if(procList.begin(), procList.end(), [&currentPID](const kinfo_proc& arg) {
				return arg.pid == currentPID;
			}) == procList.end())
		{
			klog("Proc %d has died.\n", CurrentPID);

			// Release the IPC file
			GeneralIPC::DeleteTempFile(CurrentPID);

			// Aquire lock.
			scePthreadMutexLock(&DebugMutex);

			// Reset vars.
			IsDebugging = false;
			CurrentPID = -1;

			// Send attach event to host.
			Events::SendEvent(Events::EVENT_DIE, CurrentPID);

			// Let the process die.
			ptrace(PT_CONTINUE, CurrentPID, (void*)1, SIGKILL);

			// release the lock.
			scePthreadMutexUnlock(&DebugMutex);

			goto Thread_Exit;
		}

		int status;
		auto debuggiePid = wait4(CurrentPID, &status, WNOHANG, nullptr);
		if (debuggiePid == CurrentPID)
		{
			int signal = WSTOPSIG(status);
			klog("Process %d has recieved the signal %d\n", CurrentPID, signal);
			// TODO: Back trace here on bad sig

			switch (signal)
			{
			case SIGSTOP:
				klog("SIGSTOP\n");
				break;
			}
		}

		sceKernelSleep(1);
	}

Thread_Exit:
	// TODO: Remove any Watchpoints / Breakpoints now.
	//		 Unless the process is dying maybe?

	// Kill our thread and exit.
	scePthreadExit(NULL);
	return nullptr;
}

void* Debug::ProcessMonotorThreadHelper(void* tdParam)
{
	return ((Debug*)tdParam)->ProcessMonotorThread();
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

	// Create thread to monitor the state of the running process.
	scePthreadCreate(&ProcMonitorThreadHandle, NULL, &ProcessMonotorThreadHelper, this, "Process Monitor Thread");

	// release the lock.
	scePthreadMutexUnlock(&DebugMutex);

	// Send attach event to host.
	Events::SendEvent(Events::EVENT_ATTACH, pid);

	klog("Attached to %s(%d)\n", processName, pid);

	SendStatus(Sock, 1);

	// Check the satus of the general helper.
	if (!GeneralIPC::TestConnection(pid))
	{
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
	if (sceNetRecv(Sock, Packet, sizeof(DbgSPRXPacket), 0) < 0)
	{
		klog("Failed to recieve next packet.\n");

		free(Packet);

		Sockets::SendInt(Sock, -1);

		return;
	}

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

void Debug::ReadWriteMemory(OrbisNetId Sock, bool write)
{
	if (!IsDebugging || CurrentPID == -1)
	{
		return;
	}

	// Get next packet.
	auto Packet = (DbgRWPacket*)malloc(sizeof(DbgRWPacket));
	sceNetRecv(Sock, Packet, sizeof(DbgRWPacket), 0);

	if (write)
	{
		auto buffer = (unsigned char*)malloc(Packet->Length);

		if (!Sockets::RecvLargeData(Sock, buffer, Packet->Length))
		{
			free(buffer);

			klog("Failed to recieve memory to write\n");

			return;
		}

		if (!GeneralIPC::ReadWriteMemory(CurrentPID, Packet->Address, buffer, Packet->Length, true))
		{
			free(buffer);

			klog("Failed to write memory to process %i at %llX\n", CurrentPID, Packet->Address);

			Sockets::SendInt(Sock, 0);

			return;
		}

		free(buffer);

		Sockets::SendInt(Sock, 1);
	}
	else
	{
		auto buffer = (unsigned char*)malloc(Packet->Length);

		if (!GeneralIPC::ReadWriteMemory(CurrentPID, Packet->Address, buffer, Packet->Length, false))
		{
			free(buffer);

			klog("Failed to write memory to process %i at %llX\n", CurrentPID, Packet->Address);

			Sockets::SendInt(Sock, 0);

			return;
		}

		Sockets::SendInt(Sock, 1);

		if (!Sockets::SendLargeData(Sock, buffer, Packet->Length))
		{
			free(buffer);

			klog("Failed to send memory\n");

			return;
		}

		free(buffer);
	}
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