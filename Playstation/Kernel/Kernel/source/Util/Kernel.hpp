#pragma once
#include "../Common.hpp"

int ksetuid(uid_t uid, thread* td);
void klog(char* fmt, ...);

caddr_t kmap(caddr_t addr, size_t len, int prot, int flags, int fd, off_t pos, struct thread* td);
int kmunmap(void *addr, size_t len, struct thread* td);
int kunmount(const char* path, int flags, struct thread* td);