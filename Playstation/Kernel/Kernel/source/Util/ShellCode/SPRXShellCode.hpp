#pragma once

struct OrbisLoader_header
{
    uint32_t magic;
    uint64_t entry;

    uint64_t thr_initial;
    uint8_t ShellCodeComplete;
    uint8_t ShouldCallEntry;

    char Path[100];
    uint64_t ModuleHandle;
}__attribute__((packed));

struct OrbisUnLoader_header
{
    uint32_t magic;
    uint64_t entry;

    uint64_t thr_initial;
    uint8_t ShellCodeComplete;
    uint8_t ShouldCallExit;

    uint64_t ModuleHandle;
    uint64_t Result;
}__attribute__((packed));

int LoadSPRX(const char* ProcessName, const char* Path, bool ShouldCallEntry = true);
int UnloadSPRX(const char* ProcessName, int Handle, bool ShouldCallExit = true);
int UnloadSPRX(const char* ProcessName, const char* Name, bool ShouldCallExit = true);