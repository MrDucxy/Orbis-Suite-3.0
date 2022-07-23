#include "../../Common.hpp"
#include "Proc.hpp"

#include "DriverDefinitions.h"
#include "../../Util/ShellCode/SPRXShellCode.hpp"
#include "../../Util/System.hpp"

int Proc::OnIoctl(cdev* dev, unsigned long cmd, caddr_t data, int fflag, thread* td)
{
    switch(cmd)
    {
    case PROC_LIST:
		return GetProccessList(data, td);

	case PROC_INFO:
		return GetProccessInfo(data, td);

	case PROC_MODULE_LIST:
		return GetProccessModuleList(data, td);

	case PROC_READ_WRITE_MEMORY:
		return ProcessReadWrite(data, td);

	case PROC_ALLOC_MEMORY:
		return ProcessAlloc(data, td);

	case PROC_FREE_MEMORY:
		return ProcessFree(data);

	case PROC_SPRX:
		return ProcessSPRX(data);

	case PROC_ELF:
		return ProcessELF(data, td);

    default:
        klog("[Proc] Not Implimented. :(");
        break;
    }
}

int Proc::GetProccessList(caddr_t data, thread* td)
{
	if (data == nullptr)
    {
        klog("Data pointer invalid...");
        return EINVAL;
    }

	auto Input = (KDriver_ProcList*)data;
	auto Temp = (ProcInfo*)kmalloc(sizeof(ProcInfo) * Input->ProcCount);

	klog("Itterating Processes!");
	int ProccessCount = 0;
	proc* CurrentProc = *(proc**)allproc;
	do
	{
		if(ProccessCount > Input->ProcCount)
			break;

		klog("%s", CurrentProc->p_comm);

		Temp[ProccessCount].PID = CurrentProc->p_pid;
		Temp[ProccessCount].Attached = ((CurrentProc->p_flag & 0x800) != 0);
		memcpy(&Temp[ProccessCount].ProcName, CurrentProc->p_comm, strlen(CurrentProc->p_comm) + 1);
        memcpy(&Temp[ProccessCount].TitleID, CurrentProc->titleId, 10);
        Temp[ProccessCount].TextSegmentBase = (uint64_t)CurrentProc->p_vmspace->vm_taddr;
        Temp[ProccessCount].TextSegmentLen = (uint64_t)(CurrentProc->p_vmspace->vm_tsize * PAGE_SIZE);
        Temp[ProccessCount].DataSegmentBase = (uint64_t)CurrentProc->p_vmspace->vm_daddr;
        Temp[ProccessCount].DataSegmentLen = (uint64_t)(CurrentProc->p_vmspace->vm_dsize * PAGE_SIZE);

		ProccessCount++;
		CurrentProc = CurrentProc->p_list.le_next;
	} while (CurrentProc != nullptr);

	Input->ProcCount = ProccessCount;

	WriteProcessMemory(td->td_proc, (void*)Input->UserlandAddr, Temp, Input->UserlandSize);

	kfree(Temp);

	klog("done!");

	return 0;
}

int Proc::GetProccessInfo(caddr_t data, thread* td)
{
	if (data == nullptr)
    {
        klog("Data pointer invalid...");
        return EINVAL;
    }

	auto Input = (KDriver_ProcInfo*)data;

	proc* CurrentProc = FindProcessByPID(Input->ProcessID);

	if(CurrentProc == nullptr)
	{
		klog("Failed to find Process with the pid %i\n", Input->ProcessID);
		return EINVAL;
	}

	auto Temp = (ProcInfoExt*)kmalloc(sizeof(ProcInfoExt) + (sizeof(ProcInfoExt::ThreadInfo) * Input->ThreadCount));

	Temp->ProcessID = CurrentProc->p_pid;
	Temp->Attached = ((CurrentProc->p_flag & 0x800) != 0);
	Temp->Signal = CurrentProc->p_sig;
	Temp->Code = CurrentProc->p_code;
	Temp->Stops = CurrentProc->p_stops;
	Temp->StopType = CurrentProc->p_stype;
	strcpy(Temp->ProcName, CurrentProc->p_comm);
	strcpy(Temp->TitleID, CurrentProc->titleId);
	strcpy(Temp->RandomizedPath, CurrentProc->p_elfpath);
	strcpy(Temp->RandomizedPath, CurrentProc->p_randomized_path);
	Temp->TextSegmentBase = (uint64_t)CurrentProc->p_vmspace->vm_taddr;
	Temp->TextSegmentLen = (uint64_t)(CurrentProc->p_vmspace->vm_tsize * PAGE_SIZE);
	Temp->DataSegmentBase = (uint64_t)CurrentProc->p_vmspace->vm_daddr;
	Temp->DataSegmentLen = (uint64_t)(CurrentProc->p_vmspace->vm_dsize * PAGE_SIZE);

	//TODO: Threads...
	thread* CurrentThread = CurrentProc->p_threads.tqh_first;
	for (Temp->ThreadCount = 0; (Temp->ThreadCount < Input->ThreadCount) && CurrentThread != nullptr; Temp->ThreadCount++)
	{
		Temp->Threads[Temp->ThreadCount].ThreadId = CurrentThread->td_tid;
		strcpy(Temp->Threads[Temp->ThreadCount].Name, CurrentThread->td_name);
		Temp->Threads[Temp->ThreadCount].Errno = CurrentThread->td_errno;
		Temp->Threads[Temp->ThreadCount].RetVal = CurrentThread->td_retval[0];

		CurrentThread = CurrentThread->td_plist.tqe_next;
	}

	WriteProcessMemory(td->td_proc, (void*)Input->UserlandAddr, Temp, Input->UserlandSize);

	kfree(Temp);

	return 0;
}

int Proc::GetProccessModuleList(caddr_t data, thread* td)
{
	if (data == nullptr)
    {
        klog("Data pointer invalid...");
        return EINVAL;
    }

	auto Input = (KDriver_ModuleList*)data;

	proc* CurrentProc = FindProcessByPID(Input->ProcessID);

	if(CurrentProc == nullptr)
	{
		klog("Failed to find Process with the pid %i\n", Input->ProcessID);
		return EINVAL;
	}

	auto Temp = (ModuleInfo*)kmalloc(sizeof(ModuleInfo) * Input->ModuleCount);

	int ModuleCount = 0;
	auto CurrentModule = CurrentProc->p_dynlibptr->p_dynlib;
    while(CurrentModule != 0)
	{
		Temp[ModuleCount].Handle = CurrentModule->ModuleHandle;
		strcpy(Temp[ModuleCount].Name, (char*)basename(CurrentModule->ModulePath));
        strcpy(Temp[ModuleCount].Path, (char*)CurrentModule->ModulePath);
		Temp[ModuleCount].TextSegmentBase = (uint64_t)CurrentModule->codeBase;
        Temp[ModuleCount].TextSegmentLen = CurrentModule->codeSize;
        Temp[ModuleCount].DataSegmentBase = (uint64_t)CurrentModule->dataBase;
        Temp[ModuleCount].DataSegmentLen = CurrentModule->dataSize;

        ModuleCount ++;
        CurrentModule = CurrentModule->dynlib_next;
    }

	WriteProcessMemory(td->td_proc, (void*)Input->UserlandAddr, Temp, Input->UserlandSize);

	kfree(Temp);

	return 0;
}

int Proc::ProcessReadWrite(caddr_t data, thread* td)
{
	if (data == nullptr)
    {
        klog("Data pointer invalid...");
        return EINVAL;
    }

	auto Input = (KDriver_ReadWriteMemory*)data;

	proc* CurrentProc = FindProcessByPID(Input->ProcessID);

	if(CurrentProc == nullptr)
	{
		klog("Failed to find Process with the pid %i\n", Input->ProcessID);
		return EINVAL;
	}

	// TODO: Error handling.
	if (Input->IsRead)
	{
		auto Temp = (char*)kmalloc(Input->Length);

		ReadProcessMemory(CurrentProc, (void*)Input->ProcessAddress, Temp, Input->Length);
		WriteProcessMemory(td->td_proc, (void*)Input->UserlandAddr, Temp, Input->Length);

		kfree(Temp);
	}
	else
	{
		auto Temp = (char*)kmalloc(Input->Length);

		ReadProcessMemory(td->td_proc, (void*)Input->UserlandAddr, Temp, Input->Length);
		WriteProcessMemory(CurrentProc, (void*)Input->ProcessAddress, Temp, Input->Length);

		kfree(Temp);
	}

	return 0;
}

int Proc::ProcessAlloc(caddr_t data, thread* td)
{
	if (data == nullptr)
    {
        klog("Data pointer invalid...");
        return EINVAL;
    }

	return 0;
}

int Proc::ProcessFree(caddr_t data)
{
	if (data == nullptr)
    {
        klog("Data pointer invalid...");
        return EINVAL;
    }

	return 0;
}

int Proc::ProcessSPRX(caddr_t data)
{
	if (data == nullptr)
    {
        klog("Data pointer invalid...");
        return EINVAL;
    }

	auto Input = (KDriver_ProcSPRX*)data;

	switch (Input->CallType)
	{
	case 0:
		LoadSPRX(Input->ProcName, Input->Path, Input->CallEntryExit);
		break;

	case 1:
		UnloadSPRX(Input->ProcName, Input->Path, Input->CallEntryExit);
		break;

	case 2:
		UnloadSPRX(Input->ProcName, Input->Handle, Input->CallEntryExit);
		break;

	default:
		klog("ProcessSPRX: Invalid CallType %d.", Input->CallType);
		break;
	}
	
	return 0;
}

int Proc::ProcessELF(caddr_t data, thread* td)
{
	if (data == nullptr)
    {
        klog("Data pointer invalid...");
        return EINVAL;
    }

	return 0;
}