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