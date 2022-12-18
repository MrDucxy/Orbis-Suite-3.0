#pragma once

extern "C"
{
    enum { CWD_KEEP, CWD_ROOT, CWD_RESET };

    struct LibraryInfo
    {
        char Path[256];
        uint64_t ModuleHandle;
        uint64_t map_base;
        size_t map_size;
        size_t text_size;
        uint64_t data_base;
        size_t data_size;
    };

    void jbc_run_as_root(void(*fn)(void* arg), void* arg, int cwd_mode);
    int jbc_mount_in_sandbox(const char* system_path, const char* mnt_name);
    int jbc_unmount_in_sandbox(const char* mnt_name);
    int jbc_set_proc_name(const char* New_Name);
    int jbc_get_proc_libraries(struct LibraryInfo* out, int maxCount);

    struct jbc_cred
    {
        uid_t uid;
        uid_t ruid;
        uid_t svuid;
        gid_t rgid;
        gid_t svgid;
        uintptr_t prison;
        uintptr_t cdir;
        uintptr_t rdir;
        uintptr_t jdir;
        uint64_t sceProcType;
        uint64_t sonyCred;
        uint64_t sceProcCap;
    };


    int jbc_get_cred(struct jbc_cred*);
    int jbc_jailbreak_cred(struct jbc_cred*);
    int jbc_set_cred(const struct jbc_cred*);
}