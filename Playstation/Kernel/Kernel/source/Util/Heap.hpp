#pragma once

void* kmalloc(long unsigned int size);
void kfree(void* addr);
void* operator new(long unsigned int size);
void* operator new(long unsigned int size, void * addr);
void* operator new[] (long unsigned int size);
void operator delete(void* addr) noexcept;
void operator delete[](void* addr) noexcept;