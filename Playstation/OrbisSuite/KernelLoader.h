#pragma once

struct _OffsetTable
{
	uint64_t Xfast_syscall;
	uint64_t kmem_alloc;
	uint64_t kmem_free;
	uint64_t kernel_map;
	uint64_t _memcmp;
	uint64_t _memcpy;
	uint64_t _memset;

	void Init505()
	{
		Xfast_syscall = 0x1C0;
		kmem_alloc = 0xFCC80;
		kmem_free = 0xFCE50;
		kernel_map = 0x1AC60E0;
		_memcmp = 0x50AC0;
		_memcpy = 0x1EA530;
		_memset = 0x3205C0;
	}

	void Init672()
	{
		Xfast_syscall = 0x1C0;
		kmem_alloc = 0x250730;
		kmem_free = 0x250900;
		kernel_map = 0x220DFC0;
		_memcmp = 0x207E40;
		_memcpy = 0x3C15B0;
		_memset = 0x1687D0;
	}

	void Init702()
	{
		Xfast_syscall = 0x1C0;
		kmem_alloc = 0x1170F0;
		kmem_free = 0x1172C0;
		kernel_map = 0x21C8EE0;
		_memcmp = 0x207500;
		_memcpy = 0x2F040;
		_memset = 0x2DFC20;
	}

	void Init755()
	{
		Xfast_syscall = 0x1C0;
		kmem_alloc = 0x1753E0;
		kmem_free = 0x1755B0;
		kernel_map = 0x21405B8;
		_memcmp = 0x31D250;
		_memcpy = 0x28F800;
		_memset = 0x8D6F0;
	}

	void Init900()
	{
		Xfast_syscall = 0x1C0;
		kmem_alloc = 0x37BE70;
		kmem_free = 0x37C040;
		kernel_map = 0x2268D48;
		_memcmp = 0x271E20;
		_memcpy = 0x2714B0;
		_memset = 0x1496C0;
	}

};

extern _OffsetTable* OffsetTable;

bool SetKernelVersion(int Firmware);
bool LoadKernel(char* Path, int Firmware);
bool UnloadKernel();