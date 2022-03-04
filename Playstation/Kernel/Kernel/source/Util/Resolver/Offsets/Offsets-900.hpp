#pragma once

#pragma once

#if defined(SOFTWARE_VERSION_900)

/* Util */
#define addr_Xfast_syscall		                0x000001C0
#define addr_sysvec                             0x01528E30
#define addr_kernel_map                         0x02268D48
#define addr_prison0                            0x0111F870 
#define addr_rootvnode                          0x021EFF20
#define addr_copyin								0x002716A0
#define addr_copyout							0x002715B0

/* STD Lib */
#define addr_M_TEMP                             0x015621E0
#define addr_M_MOUNT                            0x015279F0
#define addr_malloc								0x00301B20
#define addr_free					    		0x00301CE0
#define addr_memcpy								0x002714B0
#define addr_memset								0x001496C0
#define addr_memcmp								0x00271E20
#define addr_strlen                             0x0030F450
#define addr_strcpy								0x00189F80
#define addr_strncpy                            0x0041E380
#define addr_strcmp                             0x0040E700
#define addr_strncmp                            0x00124750 
#define addr_strstr                             0x00487AB0
#define addr_sprintf                            0x000B7C70
#define addr_snprintf                           0x000B7D30
#define addr_vsprintf                           0x000B7D00
#define addr_vprintf                            0x000B7AA0
#define addr_sscanf                             0x0026C8D0
#define addr_strdup                             0x00278540
#define addr_realloc                            0x00301DE0
#define addr_kprintf                            0x000B7A30

/* Kproc */
#define addr_kproc_create					    0x000969E0
#define addr_kproc_exit                         0x00096C50
#define addr_kproc_resume                       0x00096DA0
#define addr_kproc_shutdown                     0x0029AC80
#define addr_kproc_start                        0x00096960
#define addr_kproc_suspend                      0x00096D00
#define addr_kproc_suspend_check                0x00096E30
#define addr_kproc_kthread_add				    0x00097750
#define addr_pause                              0x00453EA0
#define addr_kthread_add						0x00096F40
#define addr_kthread_exit						0x00097230
#define addr_kthread_suspend					0x0
#define addr_kthread_suspend_check				0x00097640
#define addr_kthread_set_affinity				0x000978A0

/* Module Offsets */
#define addr_thr_initial_libkernel              0x0008E430
#define addr_thr_initial_libkernel_web          0x0008E430
#define addr_thr_initial_libkernel_sys          0x0008E830

/* Proc */
#define addr_allproc_lock                       0x01B94680
#define addr_allproc						    0x01B946E0
#define addr_proc_kill                          0x00029780
#define addr_proc_rwmem							0x0041EB00
#define addr_create_thread                      0x001ED670

/* ptrace */
#define addr_kptrace                            0x0041F410
#define addr_kpsignal                           0x002F9BA0
#define addr_kwait                              0x00174110
#define addr_kDelay                             0x0018A6B0

/* Virtual Memory */
#define addr_vmspace_acquire_ref				0x0007B9E0
#define addr_vmspace_free						0x0007B810
#define addr_vm_map_lock_read					0x0007BB80
#define addr_vm_map_unlock_read					0x0007BBD0
#define addr_vm_map_lookup_entry				0x0007C1C0
#define addr_vm_map_findspace					0x0007EC40
#define addr_vm_map_insert						0x0007CD80 
#define addr_vm_map_lock						0x0007BA30
#define addr_vm_map_unlock 						0x0007BAA0
#define addr_vm_map_delete						0x0007E680
#define addr_vm_map_protect						0x000809C0

/*Mutex Locks*/
#define addr_mtx_init                           0x002EF960
#define addr_mtx_destroy                        0x002EF9D0
#define addr_mtx_lock_flags                     0x002EEEB0
#define addr_mtx_unlock_flags                   0x002EF170

#define addr__sx_slock                          0x0043E1A0
#define addr__sx_sunlock                        0x0043E710

/* Critical Sections */
#define addr_EnterCriticalSection               0x0
#define addr_ExitCriticalSection                0x0

/* Event Handling */
#define addr_eventhandler_register              0x000F8370
#define addr_eventhandler_deregister            0x000F8700
#define addr_eventhandler_find_list             0x000F88F0

/* Trap Hooks */
#define addr_trapHook                           0x0
#define addr_trap_fatalHook                     0x0

/* Registry Functions */
#define addr_sceRegMgrGetStr                    0x004EA5F0
#define addr_sceRegMgrSetStr                    0x004F65B5
#define addr_sceRegMgrGetInt                    0x004E9DD0
#define addr_sceRegMgrSetInt                    0x004E8B10
#define addr_sceRegMgrGetBin                    0x004EA770
#define addr_sceRegMgrSetBin                    0x004EA6C0

/* Flash & NVS */
#define addr_icc_nvs_read                       0x0010B310
#define addr_icc_nvs_write                      0x0

/* Driver */
#define addr_make_dev_p                         0x001EF590
#define addr_destroy_dev                        0x001EFAB0

/* kmem */
#define addr_kmem_alloc                         0x0037BE70
#define addr_kmem_free                          0x0037C040
#define addr_kernel_map                         0x02268D48

/* File IO */
#define addr_vn_fullpath                        0x002648C0
#define addr_kern_rmdir                         0x001DF3A0
#define addr_kern_mkdir                         0x001DF020
#define addr_kern_open                          0x001D9EE0
#define addr_kern_mount                         0x0004DF50
#define addr_mount_argf                         0x0004DDB0

#endif