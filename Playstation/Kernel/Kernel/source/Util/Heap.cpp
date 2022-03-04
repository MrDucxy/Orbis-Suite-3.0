#include "../Common.hpp"
#include "Heap.hpp"

void* kmalloc(long unsigned int size) {
	return malloc(size, M_TEMP, 2);
}

void kfree(void* addr)
{
    if (addr == nullptr)
		return;

	free(addr, M_TEMP);
}

void* operator new(long unsigned int size)
{
	if (size == 0)
		return nullptr;

	return malloc(size, M_TEMP, M_ZERO | M_NOWAIT);
}

void* operator new(long unsigned int size, void * addr)
{
    return addr;
}

void* operator new[] (long unsigned int size)
{
	return ::operator new(size);
}

void operator delete(void* addr) noexcept
{
	if (addr == nullptr)
		return;

	free(addr, M_TEMP);
}

void operator delete[](void* addr) noexcept
{
	::operator delete(addr);
}