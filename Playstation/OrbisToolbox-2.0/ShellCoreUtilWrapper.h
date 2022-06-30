#pragma once
#include "Common.h"

class ShellCoreUtilWrapper
{
public:
	static int sceShellCoreUtilGetFreeSizeOfUserPartition(uint64_t* free, uint64_t* total);
};