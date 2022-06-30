#include "ShellCoreUtilWrapper.h"
#include "Common.h"

int ShellCoreUtilWrapper::sceShellCoreUtilGetFreeSizeOfUserPartition(uint64_t* free, uint64_t* total)
{
	MonoClass* ShellCoreUtilWrapper_Class = Mono::Get_Class(Mono::ShellCoreUtilWrapper, "Sce.Vsh", "ShellCoreUtilWrapper");
	auto result = Mono::Invoke<int>(Mono::ShellCoreUtilWrapper, ShellCoreUtilWrapper_Class, nullptr, "sceShellCoreUtilGetFreeSizeOfUserPartition", free, total);

	return result;
}