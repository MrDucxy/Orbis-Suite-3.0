#include "../Common.hpp"
#include "Kernel.hpp"

extern "C"
{
    #include <sys/sysproto.h>
    #include <sys/sysent.h>
    #include <sys/pcpu.h>
    #include <sys/proc.h>
    #include <vm/vm.h>

    #include <sys/_iovec.h>
    #include <sys/uio.h>

    #include <fcntl.h>
}


#ifndef MAP_FAILED
#define MAP_FAILED      ((void *)-1)
#endif

int kern_errorno = 0;

int ksetuid(uid_t uid, thread* td)
{
	setuid_args uap;
	sysent* sysents = sysvec->sv_table;
	auto sys_setuid = (int(*)(thread*, setuid_args*))sysents[23].sy_call;

	// clear errors
	td->td_retval[0] = 0;

	// Set up Params
	uap.uid = uid;

	// Call System call
	kern_errorno = sys_setuid(td, &uap);
	if(kern_errorno)
		return -kern_errorno;

	// success
	return td->td_retval[0];
}

void klog(char* fmt, ...)
{
	char Buffer[0x2000];

	va_list args;
	va_start(args, fmt);
	vsprintf(Buffer, fmt, args);
	va_end(args);

	kprintf("%s\n", Buffer);
}

caddr_t kmap(caddr_t addr, size_t len, int prot, int flags, int fd, off_t pos, struct thread* td)
{
	struct mmap_args uap;
	sysent* sysents = sysvec->sv_table;
	auto sys_mmap  = (int(*)(thread*, mmap_args*))sysents[477].sy_call;
	if (!sys_mmap)
		return (caddr_t)-1;

	// clear errors
	td->td_retval[0] = 0;

	// Set up Params
	uap.addr = addr;
	uap.len = len;
	uap.prot = prot;
	uap.flags = flags;
	uap.fd = fd;
	uap.pos = pos;

	// Call System call
	kern_errorno = sys_mmap(td, &uap);
	if(kern_errorno)
		return (caddr_t)-kern_errorno;

	// success
	return (caddr_t)td->td_retval[0];
}

int kmunmap(void *addr, size_t len, struct thread* td)
{
	munmap_args uap;
	sysent* sysents = sysvec->sv_table;
	auto sys_munmap = (int(*)(thread*, munmap_args*))sysents[73].sy_call;

	// clear errors
	td->td_retval[0] = 0;

	// Set up Params
	uap.addr = addr;
	uap.len = len;

	// Call System call
	kern_errorno = sys_munmap(td, &uap);
	if(kern_errorno)
		return -kern_errorno;

	// success
	return td->td_retval[0];
}

int kunmount(const char* path, int flags, struct thread* td)
{
	unmount_args uap;
	sysent* sysents = sysvec->sv_table;
	auto sys_unmount = (int(*)(thread*, unmount_args*))sysents[22].sy_call;

	// clear errors
	td->td_retval[0] = 0;

	// Set up Params
	uap.path = (char*)path;
	uap.flags = flags;

	// Call System call
	kern_errorno = sys_unmount(td, &uap);
	if(kern_errorno)
		return -kern_errorno;

	// success
	return td->td_retval[0];
}