#pragma once
#include "sys/types.h"

#ifndef NULL
#define NULL 0
#endif

#ifndef true
#define true 1
#endif

#ifndef false
#define false 0
#endif

enum Auth_ID
{
    JitBase = 0x3100000000000000,
    CoreDump = 0x3800000000000006,
    SysCore = 0x3800000000000007,
    ShellUI = 0x380000000000000F,
    Shell3D = 0x3800000000000009,
    ShellCore = 0x3800000000000010,
    DECID = 0x3800000000010003,
};