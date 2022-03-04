#pragma once

char* basename(const char *filename);
proc* FindProcessByName(const char* p_Name);
proc* FindProcessByPID(pid_t pid) ;
uint8_t* AllocateProcessMemory(proc* Process, uint32_t Size, uint32_t Protection);
void FreeProcessMemory(struct proc* p_Process, void* p_Pointer, uint32_t p_Size);

bool ReadWriteProcessMemory(struct proc* p_TargetProcess, void* p_TargetAddress, void* p_Data, uint32_t p_DataLength, bool p_Write);
bool ReadProcessMemory(struct proc* p_TargetProcess, void* p_TargetAddress, void* p_Data, uint32_t p_DataLength);
bool WriteProcessMemory(struct proc* p_TargetProcess, void* p_TargetAddress, void* p_Data, uint32_t p_DataLength);

int MountNullFS(char* where, char* what, int flags);
bool MountDir(thread* td, char* Sandbox, char* what, int flags);
bool UnMountDir(thread* td, char* Sandbox, char* what, int flags);
bool MountShellUIDirs(proc* p, vnode* jdir, bool Mount);
bool DoShellUIMount(proc* p, bool Mount);