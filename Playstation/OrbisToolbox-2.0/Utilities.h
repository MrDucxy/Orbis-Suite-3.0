#pragma once

#define ARRAY_COUNT(arry) sizeof(arry) / sizeof(arry[0])

typedef unsigned char vm_prot_t;	/* protection codes */

#define	VM_PROT_NONE		((vm_prot_t) 0x00)
#define	VM_PROT_READ		((vm_prot_t) 0x01)
#define	VM_PROT_WRITE		((vm_prot_t) 0x02)
#define	VM_PROT_EXECUTE		((vm_prot_t) 0x04)
#define	VM_PROT_COPY		((vm_prot_t) 0x08)	/* copy-on-read */

#define	VM_PROT_ALL		(VM_PROT_READ|VM_PROT_WRITE|VM_PROT_EXECUTE)
#define VM_PROT_RW		(VM_PROT_READ|VM_PROT_WRITE)
#define	VM_PROT_DEFAULT		VM_PROT_ALL

#define	MNT_UPDATE	0x00010000 /* not real mount, just update */

void klog(const char* fmt, ...);
void Notify(const char* MessageFMT, ...);
void Notify_Custom(const char* IconURI, const char* MessageFMT, ...);

int mount_large_fs(const char* device, const char* mountpoint, const char* fstype, const char* mode, unsigned int flags);
void DisableUpdates();
void Get_Page_Table_Stats(int vm, int type, int* Used, int* Free, int* Total);

bool SockSendInt(OrbisNetId Sock, int val);
bool SockRecvInt(OrbisNetId Sock, int* val);
int RecieveInt(OrbisNetId Sock);