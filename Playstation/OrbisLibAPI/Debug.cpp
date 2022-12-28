#include "Common.h"
#include "Debug.h"
#include "APIHelper.h"

#include <sys/ptrace.h>

void Debug::HandleAPI(OrbisNetId Sock, APIPacket* Packet)
{
	switch (Packet->Command)
	{
	default:
		break;

	case API_DBG_ATTACH:

		break;

	case API_DBG_DETACH:
		
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
	CurrentPID = 0;

	return true;
}

void Debug::Attach(OrbisNetId Sock)
{
	auto pid = RecieveInt(Sock);

	// If we are currently debugging another process lets detach from it.
	if (!TryDetach(pid))
	{
		klog("Attach(): TryDetach Failed. :(\n");
		SendStatus(Sock, 0);
		return;
	}

	// Use ptrace to attach to begin debugging this pid.
	int res = ptrace(PT_ATTACH, pid, nullptr, 0);
	if (res != 0)
	{
		klog("Attach(): ptrace(PT_ATTACH) failed with error %llX\n", res);
		SendStatus(Sock, 0);
		return;
	}

	// Attaching by default will stop execution of the remote process. Lets continue it now.
	res = ptrace(PT_CONTINUE, pid, nullptr, 0);
	if (res != 0)
	{
		klog("Attach(): ptrace(PT_CONTINUE) failed with error %llX\n", res);
		SendStatus(Sock, 0);
		return;
	}

	IsDebugging = true;
	CurrentPID = pid;

	SendStatus(Sock, 1);
}

void Debug::Detach(OrbisNetId Sock)
{
	auto pid = RecieveInt(Sock);
	auto result = TryDetach(pid);
	SendStatus(Sock, result ? 1 : 0);
}

Debug::Debug()
{

}

Debug::~Debug()
{

}