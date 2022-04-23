#include "../Common.hpp"
#include "System.hpp"
#include "../Util/ShellCode/SPRXShellCode.hpp"

char* strrchr(const char *cp, int ch)
{
    char *save;
    char c;

    for (save = (char *) 0; (c = *cp); cp++) {
	if (c == ch)
	    save = (char *) cp;
    }

    return save;
}

char* strchr(const char *s, int c)
{
  do {
    if (*s == c)
      {
        return (char*)s;
      }
  } while (*s++);
  return (0);
}

char* basename(const char *filename)
{
  char *p = strrchr(filename, '/');
  return p ? p + 1 : (char *)filename;
}

// Shamelessly yoinked from MIRA
// Credits: flatz
proc* FindProcessByName(const char* p_Name) 
{
	sx* allproclock = (sx*)allproc_lock;
	proclist* _allproc = (proclist*)*(uint64_t*)(allproc);

	proc* s_FoundProc = nullptr;

	if (!p_Name)
		return NULL;

	_sx_slock(allproclock, 0, __FILE__, __LINE__);

	do
	{
		proc* s_Proc = nullptr;

		FOREACH_PROC_IN_SYSTEM(s_Proc) 
		{
			//PROC_LOCK(s_Proc);

			if (strncmp(p_Name, s_Proc->p_comm, strlen(p_Name)) == 0) {
				s_FoundProc = s_Proc;
				//PROC_UNLOCK(s_Proc);
				break;
			}

			//PROC_UNLOCK(s_Proc);
		}
	} while (false);

	_sx_sunlock(allproclock, __FILE__, __LINE__);

	return s_FoundProc;
}

proc* FindProcessByPID(pid_t pid) 
{
	sx* allproclock = (sx*)allproc_lock;
	proclist* _allproc = (proclist*)*(uint64_t*)(allproc);

	proc* s_FoundProc = nullptr;

	_sx_slock(allproclock, 0, __FILE__, __LINE__);

	do
	{
		proc* s_Proc = nullptr;

		FOREACH_PROC_IN_SYSTEM(s_Proc) 
		{
			//PROC_LOCK(s_Proc);

			if (s_Proc->p_pid == pid) {
				s_FoundProc = s_Proc;
				//PROC_UNLOCK(s_Proc);
				break;
			}

			//PROC_UNLOCK(s_Proc);
		}
	} while (false);

	_sx_sunlock(allproclock, __FILE__, __LINE__);

	return s_FoundProc;
}

uint8_t* AllocateProcessMemory(proc* Process, uint32_t Size, uint32_t Protection)
{
    if (Process == nullptr)
        return nullptr;
    
    klog("Requested Size: (%x).", Size);
    Size = round_page(Size);
    klog("Adjusted Size (%x).", Size);

    vm_offset_t s_Address = 0;

    // Get the vmspace
    auto s_VmSpace = Process->p_vmspace;
    if (s_VmSpace == nullptr)
    {
        klog("invalid vmspace.");
        return nullptr;
    }

    // Get the vmmap
    vm_map_t s_VmMap = &s_VmSpace->vm_map;

    // Lock the vmmap
    vm_map_lock(s_VmMap, __FILE__, __LINE__);

    do
    {
        // Find some free space to allocate memory
        auto s_Result = vm_map_findspace(s_VmMap, s_VmMap->header.start, Size, &s_Address);
        if (s_Result != 0)
        {
            klog("vm_map_findspace returned (%d).", s_Result);
            break;
        }

        klog("_vm_map_findspace returned address (%p).", s_Address);

        // Validate the address
        if (s_Address == 0)
        {
            klog("allocated address is invalid (%p).", s_Address);
            break;
        }

        // Insert the new stuff map
        s_Result = vm_map_insert(s_VmMap, NULL, 0, s_Address, s_Address + Size, Protection, Protection, 0);
        if (s_Result != 0)
        {
            klog("vm_map_insert returned (%d).", s_Result);
            break;
        }

    } while (false);

    vm_map_unlock(s_VmMap);

    return reinterpret_cast<uint8_t*>(s_Address);
}

void FreeProcessMemory(struct proc* p_Process, void* p_Pointer, uint32_t p_Size)
{
    struct vmspace* s_VmSpace = p_Process->p_vmspace;
    if (s_VmSpace == nullptr)
    {
        klog("could not get vmspace.");
        return;
    }

    struct vm_map* s_VmMap = &s_VmSpace->vm_map;

    vm_map_lock(s_VmMap, __FILE__, __LINE__);

    auto s_Ret = vm_map_delete(s_VmMap, reinterpret_cast<uint64_t>(p_Pointer), p_Size);

    vm_map_unlock(s_VmMap);

    if (s_Ret != 0)
        klog("could not delete from vm map (%d).", s_Ret);
}

bool ReadWriteProcessMemory(struct proc* p_TargetProcess, void* p_TargetAddress, void* p_Data, uint32_t p_DataLength, bool p_Write)
{
    // Validate process
    if (p_TargetProcess == nullptr)
    {
        klog("invalid process.");
        return false;
    }

    thread* s_ProcMainThread = p_TargetProcess->p_singlethread ? p_TargetProcess->p_singlethread : p_TargetProcess->p_threads.tqh_first;
    if (s_ProcMainThread == nullptr)
    {
        klog("could not get process main thread.");
        return false;
    }

    // Validate the target address
    if (p_TargetAddress == nullptr)
    {
        klog("invalid target address.");
        return false;
    }

    // Validate the data
    if (p_Data == nullptr ||
        p_DataLength == 0)
    {
        klog("invalid data.");
        return false;
    }

    struct iovec s_Vec;
    memset(&s_Vec, 0, sizeof(s_Vec));
    s_Vec.iov_base = p_Data;
    s_Vec.iov_len = p_DataLength;
    
    struct uio s_Uio;
    memset(&s_Uio, 0, sizeof(s_Uio));
    s_Uio.uio_iov = &s_Vec;
	s_Uio.uio_iovcnt = 1;
	s_Uio.uio_offset = (uint64_t)p_TargetAddress;
	s_Uio.uio_resid = (uint64_t)p_DataLength;
	s_Uio.uio_segflg = UIO_SYSSPACE;
	s_Uio.uio_rw = p_Write ? UIO_WRITE : UIO_READ;
	s_Uio.uio_td = s_ProcMainThread;

	auto s_Ret = proc_rwmem(p_TargetProcess, &s_Uio);
    if (s_Ret != 0)
    {
        klog("could not proc_rwmem (%d).", s_Ret);
        return false;
    }

    return true;
}

bool ReadProcessMemory(struct proc* p_TargetProcess, void* p_TargetAddress, void* p_Data, uint32_t p_DataLength)
{
    return ReadWriteProcessMemory(p_TargetProcess, p_TargetAddress, p_Data, p_DataLength, false);
}

bool WriteProcessMemory(struct proc* p_TargetProcess, void* p_TargetAddress, void* p_Data, uint32_t p_DataLength)
{
    return ReadWriteProcessMemory(p_TargetProcess, p_TargetAddress, p_Data, p_DataLength, true);
}

int MountNullFS(char* where, char* what, int flags)
{
    struct mntarg* ma = NULL;

    ma = mount_argf(ma, "fstype", "%s", "nullfs");
    ma = mount_argf(ma, "fspath", "%s", where);
    ma = mount_argf(ma, "target", "%s", what);

    if (ma == NULL) {
    	klog("Something is wrong, ma value is null after argument\n");
    	return 50;
    }

    return kern_mount(ma, flags);
}

bool MountDir(thread* td, char* Sandbox, char* what, int flags)
{
    if(!td)
    {
        klog("Thread was NULL...");
        return false;
    }

	char s_fulldir[0x200];
	snprintf(s_fulldir, sizeof(s_fulldir), "%s%s", Sandbox, what);

	klog("Mount: %s -> %s", s_fulldir, what);

	if(kern_mkdir(td, s_fulldir, 0, 0777) != 0)
        return false;

	if(MountNullFS(s_fulldir, what, flags) != 0)
        return false;
    
    return true;
}

bool UnMountDir(thread* td, char* Sandbox, char* what, int flags)
{
    if(!td)
    {
        klog("Thread was NULL...");
        return false;
    }

	char s_fulldir[0x200];
	snprintf(s_fulldir, sizeof(s_fulldir), "%s%s", Sandbox, what);

	klog("Un-Mount: %s -> %s", s_fulldir, what);

	if(kunmount(s_fulldir, flags, td) != 0)
        return false;

	if(kern_rmdir(td, s_fulldir, 0) != 0)
        return false;

    return true;
}

bool MountShellUIDirs(proc* p, vnode* jdir, bool Mount)
{
    klog("%s dirs { system, data, host, hostapp } on process %s", Mount ? "Mounting" : "Un-Mounting", p->p_comm);

    //Get first thread in proc.
    thread* td = curthread();//p->p_threads.tqh_first;

    //Get the sandbox path.
    char* s_SandboxPath = nullptr;
    char* s_Freepath = nullptr;
    vn_fullpath(td, jdir, &s_SandboxPath, &s_Freepath);
    klog("%s -> %s\n", p->p_comm, s_SandboxPath);

    if(Mount)
    {
        if(!MountDir(td, s_SandboxPath, "/system", MNT_SYNCHRONOUS))
        {
            klog("Failed to Mount /System.");
            return false;
        }

        if(!MountDir(td, s_SandboxPath, "/data", MNT_SYNCHRONOUS))
        {
            klog("Failed to Mount /data.");
            return false;
        }

        if(!MountDir(td, s_SandboxPath, "/host", MNT_SYNCHRONOUS))
        {
            klog("Failed to Mount /host.");
            return false;
        }

        if(!MountDir(td, s_SandboxPath, "/hostapp", MNT_SYNCHRONOUS))
        {
            klog("Failed to Mount /hostapp.");
            return false;
        }

        if(!MountDir(td, s_SandboxPath, "/dev", MNT_SYNCHRONOUS))
        {
            klog("Failed to Mount /dev.");
            return false;
        }

        kchmod("/dev", 0777, td);

        return true;
    }
    else
    {
        if(!UnMountDir(td, s_SandboxPath, "/system", MNT_FORCE))
        {
            klog("Failed to Un-Mount /system.");
            return false;
        }

        if(!UnMountDir(td, s_SandboxPath, "/data", MNT_FORCE))
        {
            klog("Failed to Un-Mount /data.");
            return false;
        }

        if(!UnMountDir(td, s_SandboxPath, "/host", MNT_FORCE))
        {
            klog("Failed to Un-Mount /host.");
            return false;
        }

        if(!UnMountDir(td, s_SandboxPath, "/hostapp", MNT_FORCE))
        {
            klog("Failed to Un-Mount /hostapp.");
            return false;
        }

        if(!UnMountDir(td, s_SandboxPath, "/dev", MNT_FORCE))
        {
            klog("Failed to Un-Mount /dev.");
            return false;
        }

        return true;
    }
}

bool DoShellUIMount(proc* p, bool Mount)
{
    if(!strcmp(p->titleId, "NPXS20001") && (!strcmp(p->p_comm, "SecureUIProcess.self") || !strcmp(p->p_comm, "SceShellUI")))
    {
        // Jailbreak the process.
        Backup_Jail bkJail;
        proc_Jailbreak(p, &bkJail);

        // Un-Mount the dirs for ShellUI
        bool res = MountShellUIDirs(p, bkJail.fd_jdir, Mount);

        // Restore previous jail.
        proc_RestoreJail(p, bkJail);

        if(!strcmp(p->p_comm, "SecureUIProcess.self"))
        {
            while(FindProcessByName("SceShellUI") == nullptr) { Sleep(10); }

            Sleep(3000);

            LoadSPRX("SceShellUI", "/data/Orbis Toolbox/OrbisToolbox-2.0.sprx");
        }

        return res;
    }

    return false;
}