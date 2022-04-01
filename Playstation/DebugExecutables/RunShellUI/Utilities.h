#pragma once
#include <sys/uio.h>

#define	MNT_UPDATE	0x0000000000010000ULL

//Modules.
extern bool(*Jailbreak)();
bool LoadModules();

//Misc
unsigned long Syscall(unsigned int n, ...);
void Notify(const char* MessageFMT, ...);
void klog(const char* fmt, ...);
int GetUpdateVersion();
void InstallDaemon(const char* Daemon);
void InstallOrbisToolbox();

//File IO.
static void build_iovec(iovec** iov, int* iovlen, const char* name, const void* val, size_t len);
int nmount(struct iovec* iov, uint32_t niov, int flags);
int unmount(const char* dir, int flags);
int mount_large_fs(const char* device, const char* mountpoint, const char* fstype, const char* mode, unsigned int flags);
void CopyFile(const char* File, const char* Destination);
int MakeDir(char* Dir, ...);

#define CR0_WP (1 << 16) // write protect

static inline __attribute__((always_inline)) uint64_t __readmsr(uint32_t __register) {
	uint32_t __edx, __eax;

	__asm__ volatile (
		"rdmsr"
		: "=d"(__edx),
		"=a"(__eax)
		: "c"(__register)
		);

	return (((uint64_t)__edx) << 32) | (uint64_t)__eax;
}

static inline __attribute__((always_inline)) uint64_t __readcr0(void) {
	uint64_t cr0;

	__asm__ volatile (
		"movq %0, %%cr0"
		: "=r" (cr0)
		: : "memory"
		);

	return cr0;
}
static inline __attribute__((always_inline)) void __writecr0(uint64_t cr0) {
	__asm__ volatile (
		"movq %%cr0, %0"
		: : "r" (cr0)
		: "memory"
		);
}