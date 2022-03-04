#include <stdint.h>
#include <stdarg.h>

#include "Util/Types.hpp"
#include "Util/Settings.hpp"
#include "Util/Resolver/Resolver.hpp"
#include "Util/Kernel.hpp"
#include "Util/kproc.hpp"
#include "Util/ASM.h"
#include "Util/Heap.hpp"

#include "OrbisLib/OrbisLib.hpp"
#include "OrbisLib/KDriver/KDriver.hpp"

#include "DriverDefinitions.h"

class OrbisLib;

extern "C"
{
    #include <sys/param.h>
    #include <sys/lock.h>
    #include <sys/mutex.h>

    #include <sys/jail.h>
	#include <sys/sysproto.h>
	#include <sys/proc.h>
    #include <sys/sysent.h>
    #include <sys/syscall.h>
    #include <sys/ptrace.h>
    #include <sys/uio.h>
    #include <sys/mman.h>
    #include <sys/pcpu.h>
    #include <vm/vm.h>
    #include <vm/pmap.h>
    #include <vm/vm_map.h>

    #include <errno.h>
    #include <sys/elf_common.h>
    #include <sys/elf64.h>
    #include <sys/eventhandler.h>
    #include <wait.h>
    #include <machine/reg.h>
    #include <fcntl.h>
    #include <time.h>
    #include <dirent.h>

    #include <sys/stat.h>
    #include "sys/mount.h"

    #include <sys/conf.h>
	#include <fs/devfs/devfs.h>
}

#define offsetof(TYPE, MEMBER) __builtin_offsetof (TYPE, MEMBER)
#define	LIST_FOREACH(var, head, field)					\
	for ((var) = ((head)->lh_first);				\
		(var);							\
		(var) = ((var)->field.le_next))
#define	FOREACH_PROC_IN_SYSTEM(p)					\
	LIST_FOREACH((p), _allproc, p_list)
#define mtx_lock(m)		_mtx_lock_flags((m), 0, __FILE__, __LINE__)
#define	PROC_LOCK(p)	mtx_lock(&(p)->p_mtx)
#define mtx_unlock(m)		_mtx_unlock_flags((m), 0, __FILE__, __LINE__)
#define	PROC_UNLOCK(p)	mtx_unlock(&(p)->p_mtx)
#define Sleep(u) (pause("", u))

extern OrbisLib* OrbisLibInstance;
extern KDriver_Info* KDriverInfo;