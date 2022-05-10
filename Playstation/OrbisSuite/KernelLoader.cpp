#include "Common.h"
#include "KernelLoader.h"
#include "kELF.h"
#include "Utilities.h"
#include "../../Misc/DriverDefinitions.h"
#include "OrbisDriver.h"

_OffsetTable* OffsetTable;

typedef int vm_prot_t;
typedef uint64_t vm_offset_t;
typedef uint64_t vm_map_t;
typedef uint64_t vm_size_t;

uint64_t getkernbase() {
	return __readmsr(0xC0000082) - OffsetTable->Xfast_syscall;
}

struct InstallArgs {
	uint64_t sycall;
	void* payload;
	size_t psize;
};

//Crashes because its using userland function calls for memcpy memcmp and memset etc...
int install_orbis(struct thread* td, struct InstallArgs* args)
{
	uint64_t kernbase = getkernbase();
	auto kmem_alloc = (vm_offset_t(*)(vm_map_t, vm_size_t))(kernbase + OffsetTable->kmem_alloc);
	auto kernel_map = *(vm_map_t*)(kernbase + OffsetTable->kernel_map);

	size_t msize = 0;
	if (elf_mapped_size(args->payload, &msize)) {
		return 1;
	}
	
	int s = (msize + 0x3FFFull) & ~0x3FFFull;
	void* payloadbase = (void*)kmem_alloc(kernel_map, s);
	if (!payloadbase) {
		return 1;
	}

	KDriver_Info DriverInfo;
	DriverInfo.ELFBase = payloadbase;
	DriverInfo.Size = s;

	int r = 0;
	if ((r = load_elf(args->payload, args->psize, payloadbase, msize, (void**)&DriverInfo.Entry))) {
		return r;
	}

	if (DriverInfo.Entry(&DriverInfo)) {
		return 1;
	}

	return 0;
}

struct RemoveArgs {
	uint64_t sycall;
	KDriver_Info* Info;
};

int Remove_Orbis(thread* td, RemoveArgs* Args)
{
	uint64_t kernbase = getkernbase();
	auto kmem_free = (void(*)(void* map, void* addr, size_t size))(kernbase + OffsetTable->kmem_free);
	auto kernel_map = *(vm_map_t*)(kernbase + OffsetTable->kernel_map);

	int res = Args->Info->Shutdown();
	if (res != 0)
		return 1;

	kmem_free((void*)kernel_map, Args->Info->ELFBase, Args->Info->Size);

	return 0;
}

void CopyKernelELF(int Firmware)
{
	char InputKernelPath[0x100];
	sprintf(InputKernelPath, KERNELPATH, (Firmware >> 16));

	char KernelPath[0x100];
	sprintf(KernelPath, DEBUGKERNELPATH, (Firmware >> 16));

	CopyFile(InputKernelPath, KernelPath);
}

bool SetKernelVersion(int Firmware)
{
	OffsetTable = new _OffsetTable();
	switch (Firmware >> 16)
	{
	default:
		klog("Unknown Firmware %01X.%02X.\n", (Firmware >> 24) & 0xFF, (Firmware >> 16) & 0xFF);
		return false;
		break;

	case 0x505:
		klog("Loading Offsets for 5.05\n");
		OffsetTable->Init505();
		break;

	case 0x672:
		OffsetTable->Init672();
		break;

	case 0x702:
		OffsetTable->Init702();
		break;

	case 0x755:
		OffsetTable->Init755();
		break;

	case 0x900:
		OffsetTable->Init900();
		break;
	}

	return true;
}

bool LoadKernel(char* Path, int Firmware)
{
	klog("Loading Kernel \"%s\"\n", Path);

	int FileHandle = sceKernelOpen(Path, ORBIS_KERNEL_O_RDONLY, 0);

	if (FileHandle < 0)
	{
		if (FileHandle == 0x80020002)
			Notify("Firmware Version %01X.%02X is not supported.", (Firmware >> 24) & 0xFF, (Firmware >> 16) & 0xFF);
		else
			Notify("Error 0x%08X", FileHandle);

		return false;
	}

	//Get File size
	OrbisKernelStat Stats;
	sceKernelFstat(FileHandle, &Stats);

	if (Stats.st_size == 0)
	{
		klog("[OrbisLib] Failed to get file size.");

		//Cleanup
		sceKernelClose(FileHandle);

		return false;
	}

	//Allocate space to read data.
	char* File_Data = (char*)malloc(Stats.st_size);

	//ReadFile.
	size_t Bytes_Read = sceKernelRead(FileHandle, File_Data, Stats.st_size);

	if (Bytes_Read != Stats.st_size)
	{
		//Cleanup
		sceKernelClose(FileHandle);
		free(File_Data);

		return false;
	}

	int res = syscall(11, install_orbis, File_Data, Stats.st_size);
	if (res != 0)
	{
		klog("LoadKernel syscall failed with %i\n", res);

		//Cleanup
		sceKernelClose(FileHandle);
		free(File_Data);

		return false;
	}

	//Cleanup
	sceKernelClose(FileHandle);
	free(File_Data);

	return true;
}

bool UnloadKernel()
{
	KDriver_Info Info;
	if (OrbisDriver::GetDriverInfo(&Info))
	{
		int res = syscall(11, Remove_Orbis, &Info);
		if (res != 0)
		{
			klog("LoadKernel syscall failed with %i\n", res);
			return false;
		}

		return true;
	}
	else
		return false;
}