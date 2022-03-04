#include "../../Common.hpp"
#include "Resolver.hpp"
#include "../ASM.h"

uint8_t* gpKernelBase = nullptr;

/* Util */
sysentvec* sysvec = nullptr;
prison* prison0 = nullptr;
vnode* rootvnode = nullptr;
int (*copyout)(const void *kaddr, void *udaddr, size_t len) = nullptr;
int (*copyin)(const void *uaddr, void *kaddr, size_t len) = nullptr;

/* STD Lib */
void *M_TEMP = nullptr;
void* M_MOUNT = nullptr;
void *(*malloc)(unsigned long size, void *type, int flags) = nullptr;
void (*free)(void *addr, void *type) = nullptr;
void (*memcpy)(void *dst, const void *src, size_t len) = nullptr;
void *(*memset)(void *ptr, int value, size_t num) = nullptr;
int (*memcmp)(const void *ptr1, const void *ptr2, size_t num) = nullptr;
size_t (*strlen)(const char *str) = nullptr;
int (*strcpy)(char * str1, char * str2) = nullptr;
char* (*strncpy)(char *destination, const char *source, size_t num) = nullptr;
int (*strcmp)(const char * str1, const char * str2) = nullptr;
int (*strncmp)(const char * str1, const char * str2, size_t) = nullptr;
char* (*strstr)(const char * str1, const char * str2) = nullptr;
int (*sprintf)(char* dst, const char *fmt, ...) = nullptr;
int (*snprintf)(char *str, size_t size, const char *format, ...) = nullptr;
int (*vsprintf)(char* dst, const char* fmt, va_list ap) = nullptr;
int (*vprintf)(const char *fmt, va_list arg) = nullptr;
int(*sscanf)(const char *str, const char *format, ...) = nullptr;
char *(*strdup)(const char *s, void* type) = nullptr;
char *(*realloc)(void *addr, unsigned long size, void* mtp, int flags) = nullptr;
void(*kprintf)(const char* fmt, ...) = nullptr;

/* Kproc */
int (*kproc_create)(void (*func)(void *), void *arg, struct proc **newpp, int flags, int pages, const char *fmt, ...) = nullptr;
int (*kproc_exit)(int code) = nullptr;
//kproc_resume
//kproc_shutdown
//kproc_start
//kproc_suspend
//kproc_suspend_check
int (*kproc_kthread_add)(void (*func)(void *), void *arg, struct proc **procptr, struct thread **tdptr, int flags, int pages, char * procname, const char *fmt, ...) = nullptr;
void (*pause)(const char *wmesg, int timo) = nullptr;
int (*kthread_add)(void (*func)(void *), void *arg, struct proc *procp, struct thread **newtdpp, int flags, int pages, const char *fmt, ...) = nullptr;
void (*kthread_exit)(void) = nullptr;
//void (*kthread_suspend)(thread *td, int timo);
void (*kthread_suspend_check)(void) = nullptr;
void (*kthread_set_affinity)(const char *tdname, uint64_t prio, uint64_t cpuset, uint64_t unknown) = nullptr;

/* Proc */
sx* allproc_lock = nullptr;
proc *allproc = nullptr;
int (*proc_kill)(proc *p, char* why) = nullptr;
int (*proc_rwmem)(proc *p, uio *uio) = nullptr;
int (*create_thread)(thread * td, uint64_t ctx, void* start_func, void *arg, char *stack_base, size_t stack_size, char *tls_base, long * child_tid, long * parent_tid, uint64_t flags, uint64_t rtp) = nullptr;

/* ptrace */
int (*kptrace)(thread * td, int req, int pid, void * addr, int data) = nullptr;
int (*kpsignal)(proc* proc, int sig) = nullptr;
int (*kwait)(thread *td, int wpid, int *status, int options, void *rusage) = nullptr;
int (*kDelay)(uint64_t time) = nullptr;

/* Virtual memory */
vmspace *(*vmspace_acquire_ref)(proc *p) = nullptr;
void (*vmspace_free)(vmspace* vm) = nullptr;
void (*vm_map_lock_read)(vm_map* map) = nullptr;
void (*vm_map_unlock_read)(vm_map* map) = nullptr;
int (*vm_map_lookup_entry)(vm_map* map, uint64_t address, vm_map_entry **entries) = nullptr;
int (*vm_map_findspace)(vm_map* map, uint64_t start, uint64_t length, uint64_t *addr) = nullptr;
int (*vm_map_insert)(vm_map* map, uint64_t object, uint64_t offset, uint64_t start, uint64_t end, int prot, int max, int cow) = nullptr;
void (*vm_map_lock)(vm_map* map, const char* file, int line) = nullptr;
void (*vm_map_unlock)(vm_map* map) = nullptr;
int (*vm_map_delete)(vm_map* map, uint64_t start, uint64_t end) = nullptr;
int (*vm_map_protect)(vm_map* map, uint64_t start, uint64_t end, int new_prot, uint64_t set_max) = nullptr;

/*Mutex Locks*/
void (*mtx_init)(mtx *m, const char *name, const char *type, int opts);
void (*mtx_destroy)(mtx *mutex);
void (*mtx_lock_flags)(mtx *mutex, int flags);
void (*mtx_unlock_flags)(mtx *mutex, int flags);
void (*_mtx_lock_flags)(mtx *mutex, int flags, const char *file, int line);
void (*_mtx_unlock_flags)(mtx *mutex, int flags, const char *file, int line);

int (*_sx_slock)(sx *sx, int opts, const char *file, int line);
void (*_sx_sunlock)(sx *sx, const char *file, int line);

/*Critical Sections*/
void (*EnterCriticalSection)() = nullptr;
void (*ExitCriticalSection)() = nullptr;

/* Event Handling */
#if defined(SOFTWARE_VERSION_505) || defined(SOFTWARE_VERSION_NA)
eventhandler_tag (*eventhandler_register)(eventhandler_list *list, const char *name, void *func, void *arg, int priority) = nullptr;
#endif
#if defined(SOFTWARE_VERSION_672) || defined(SOFTWARE_VERSION_702) || defined(SOFTWARE_VERSION_755) || defined(SOFTWARE_VERSION_900) //5.5X -> 9.00
eventhandler_tag (*eventhandler_register)(eventhandler_list *list, const char *name, void *func, const char* unk, void *arg, int priority) = nullptr;
#endif
void (*eventhandler_deregister)(eventhandler_list* a, eventhandler_entry* b) = nullptr;
eventhandler_list* (*eventhandler_find_list)(const char *name) = nullptr;

/* Flash & NVS */
int (*icc_nvs_read)(uint32_t block, uint32_t offset, uint32_t size, uint8_t* value) = nullptr;
int (*icc_nvs_write)(uint32_t block, uint32_t offset, uint32_t size, uint8_t* value) = nullptr;

/* Registry */
int (*sceRegMgrGetStr)(uint64_t RegID, char* Value, int len) = nullptr;
int (*sceRegMgrSetStr)(uint64_t RegID, char* Value, int len) = nullptr;
int (*sceRegMgrGetInt)(uint64_t RegID, int32_t* Value) = nullptr;
int (*sceRegMgrSetInt)(uint64_t RegID, int32_t Value) = nullptr;
int (*sceRegMgrGetBin)(uint64_t RegID, char* Value, int size) = nullptr;
int (*sceRegMgrSetBin)(uint64_t RegID, char* Value, int size) = nullptr;

/* Driver */
int(*make_dev_p)(int _flags, cdev **_cdev, cdevsw *_devsw, ucred *_cr, uid_t _uid, gid_t _gid, int _mode, const char *_fmt, ...) = nullptr;
void(*destroy_dev)(cdev *_dev) = nullptr;

/* kmem */
vm_offset_t(*kmem_alloc)(vm_map_t map, vm_size_t size) = nullptr;
void(*kmem_free)(void* map, void* addr, size_t size) = nullptr;
vm_map_t kernel_map;

/* FileIO */
int (*vn_fullpath)(struct thread *td, struct vnode *vp, char **retbuf, char **freebuf) = nullptr;
int (*kern_rmdir)(thread* td, char *path, int flags) = nullptr;
int (*kern_mkdir)(thread* td, char *path, int pathseg, int mode) = nullptr;
int (*kern_open)(thread* td, char *path, int pathseg, int flags, int mode) = nullptr;
int (*kern_mount)(struct mntarg	*ma, int flags) = nullptr;
struct mntarg*(*mount_argf)(struct mntarg *ma, const char *name, const char *fmt, ...) = nullptr;

#define NATIVE_RESOLVE(_Ty) _Ty = (decltype(_Ty))(void*)((uint8_t *)&gpKernelBase[addr_ ## _Ty]);

void ResolveFunctions()
{
    gpKernelBase = (uint8_t*)Readmsr(0xC0000082) - addr_Xfast_syscall;

    /* Util */
    NATIVE_RESOLVE(sysvec);
    NATIVE_RESOLVE(prison0);
    NATIVE_RESOLVE(rootvnode);
    NATIVE_RESOLVE(copyin);
    NATIVE_RESOLVE(copyout);

    /* STD Lib */
    NATIVE_RESOLVE(M_TEMP);
    NATIVE_RESOLVE(M_MOUNT);
    NATIVE_RESOLVE(malloc);
    NATIVE_RESOLVE(free);
    NATIVE_RESOLVE(memcpy);
    NATIVE_RESOLVE(memset);
    NATIVE_RESOLVE(memcmp);
    NATIVE_RESOLVE(strlen);
    NATIVE_RESOLVE(strcpy);
    NATIVE_RESOLVE(strncpy);
    NATIVE_RESOLVE(strcmp);
    NATIVE_RESOLVE(strncmp);
    NATIVE_RESOLVE(strstr);
    NATIVE_RESOLVE(sprintf);
    NATIVE_RESOLVE(snprintf);
    NATIVE_RESOLVE(vsprintf);
    NATIVE_RESOLVE(vprintf);
    NATIVE_RESOLVE(sscanf);
    NATIVE_RESOLVE(strdup);
    NATIVE_RESOLVE(realloc);
    NATIVE_RESOLVE(kprintf);

    /* Kproc */
    NATIVE_RESOLVE(kproc_create);
    NATIVE_RESOLVE(kproc_exit);
    NATIVE_RESOLVE(kproc_kthread_add);
    NATIVE_RESOLVE(pause);
    NATIVE_RESOLVE(kthread_add);
    NATIVE_RESOLVE(kthread_exit);
    NATIVE_RESOLVE(kthread_suspend_check);
    NATIVE_RESOLVE(kthread_set_affinity);

    /* Proc */
    NATIVE_RESOLVE(allproc_lock);
    NATIVE_RESOLVE(allproc);
    NATIVE_RESOLVE(proc_kill);
    NATIVE_RESOLVE(proc_rwmem);
    NATIVE_RESOLVE(create_thread);

    /* ptrace */
    NATIVE_RESOLVE(kptrace);
    NATIVE_RESOLVE(kpsignal);
    NATIVE_RESOLVE(kwait);
    NATIVE_RESOLVE(kDelay);

    /* Virtual Memory */
    NATIVE_RESOLVE(vmspace_acquire_ref);
    NATIVE_RESOLVE(vmspace_free);
    NATIVE_RESOLVE(vm_map_lock_read);
    NATIVE_RESOLVE(vm_map_unlock_read);
    NATIVE_RESOLVE(vm_map_lookup_entry);
    NATIVE_RESOLVE(vm_map_findspace);
    NATIVE_RESOLVE(vm_map_insert);
    NATIVE_RESOLVE(vm_map_lock);
    NATIVE_RESOLVE(vm_map_unlock);
    NATIVE_RESOLVE(vm_map_delete);
    NATIVE_RESOLVE(vm_map_protect);

    /*Mutex Locks*/
    NATIVE_RESOLVE(mtx_init);
    NATIVE_RESOLVE(mtx_destroy);
    NATIVE_RESOLVE(mtx_lock_flags);
    NATIVE_RESOLVE(mtx_unlock_flags);

    NATIVE_RESOLVE(_sx_slock);
    NATIVE_RESOLVE(_sx_sunlock);

    /* Critical Sections */
    NATIVE_RESOLVE(EnterCriticalSection);
    NATIVE_RESOLVE(ExitCriticalSection);

    /* Event Handling */
    NATIVE_RESOLVE(eventhandler_register);
    NATIVE_RESOLVE(eventhandler_deregister);
    NATIVE_RESOLVE(eventhandler_find_list);

    /* Registry Functions */
    NATIVE_RESOLVE(sceRegMgrGetStr);
    NATIVE_RESOLVE(sceRegMgrSetStr);
    NATIVE_RESOLVE(sceRegMgrGetInt);
    NATIVE_RESOLVE(sceRegMgrSetInt);
    NATIVE_RESOLVE(sceRegMgrGetBin);
    NATIVE_RESOLVE(sceRegMgrSetBin);

    /* Flash & NVS */
    NATIVE_RESOLVE(icc_nvs_read);
    NATIVE_RESOLVE(icc_nvs_write);

    /* Driver */
    NATIVE_RESOLVE(make_dev_p);
    NATIVE_RESOLVE(destroy_dev);

    /* kmem */
    NATIVE_RESOLVE(kmem_alloc);
    NATIVE_RESOLVE(kmem_free);
    NATIVE_RESOLVE(kernel_map);

    /* FileIO */
    NATIVE_RESOLVE(vn_fullpath);
    NATIVE_RESOLVE(kern_rmdir);
    NATIVE_RESOLVE(kern_mkdir);
    NATIVE_RESOLVE(kern_open);
    NATIVE_RESOLVE(kern_mount);
    NATIVE_RESOLVE(mount_argf);

}