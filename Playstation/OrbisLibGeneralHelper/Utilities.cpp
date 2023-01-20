#include "Common.h"
#include "Utilities.h"

#define KLOG_FMT "[OrbisHelper] %s"

void klog(const char* fmt, ...)
{
	char Buffer[0x200];
	char Buffer2[0x200];

	//Create full string from va list.
	va_list args;
	va_start(args, fmt);
	vsprintf(Buffer, fmt, args);
	va_end(args);

	sprintf(Buffer2, KLOG_FMT, Buffer);

	sceKernelDebugOutText(0, Buffer2);
}

jbc_cred BackupCred;
bool Jailbroken = false;

void Jailbreak()
{
	if (!Jailbroken)
	{
		jbc_get_cred(&BackupCred);

		jbc_cred jbCred;
		jbc_get_cred(&jbCred);

		jbc_jailbreak_cred(&jbCred);

		jbc_set_cred(&jbCred);

		Jailbroken = true;
	}
}

void RestoreJail()
{
	if (Jailbroken)
	{
		jbc_set_cred(&BackupCred);
	}
}

#define ModuleMax 200
bool GetModuleFromAddress(uint64_t Address, OrbisKernelModuleInfo* out)
{
	OrbisKernelModule List[ModuleMax];
	size_t moduleCount;
	auto res = sceKernelGetModuleList(List, sizeof(List), &moduleCount);
	if (res != 0)
	{
		klog("GetModuleFromAddress(): Failed to get Module List: %llX\n", res);

		return false;
	}

	for (int i = 0; i < ModuleMax; i++)
	{
		out->size = sizeof(OrbisKernelModuleInfo);
		res = sceKernelGetModuleInfo(List[i], out);

		if (res == 0)
		{
			for (int j = 0; j < out->segmentCount; j++)
			{
				auto baseAddress = (uint64_t)out->segmentInfo[j].address;
				auto segSize = out->segmentInfo[j].size;
				if (Address >= baseAddress && Address < (baseAddress + segSize))
				{
					return true;
				}
			}
		}
	}

	return false;
}

bool IsMemoryMapped(uint64_t address)
{
	OrbisKernelVirtualQueryInfo info;
	return sceKernelVirtualQuery((void*)address, 0, &info, sizeof(info)) == 0;
}