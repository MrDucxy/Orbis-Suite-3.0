#pragma once

class ShellCoreUtil
{
public:
	static int Init();
	static int sceShellCoreUtilGetFreeSizeOfUserPartition(uint64_t* free, uint64_t* total);

private:
	static uint64_t LibraryBaseAddress;
	static int(*_sceShellCoreUtilGetFreeSizeOfUserPartition)(uint64_t* free, uint64_t* total);
};
