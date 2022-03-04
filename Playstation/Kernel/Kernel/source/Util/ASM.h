#pragma once
#include "../Common.hpp"

extern "C"
{
    extern thread* curthread();
    extern uint64_t Readmsr(int Reg);
    extern void cpu_enable_wp();
    extern void cpu_disable_wp();
}
