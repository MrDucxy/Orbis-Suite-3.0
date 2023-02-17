#pragma once
#include "Common.h"

class SystemMonitor
{
public:
	struct Memory
	{
		int Used;
		int Free;
		int Total;
		float Percentage;
	};

	static OrbisPthread ThreadId;
	static int Thread_Count;
	static int Busy_Core;
	static float Usage[8];
	static float Average_Usage;
	static int CPU_Temp;
	static int SOC_Temp;
	static Memory RAM;
	static Memory VRAM;

	static void Init();
	static void Term();

private:
	struct threadUsage
	{
		OrbisKernelTimespec current_time;	//0x00
		int Thread_Count;					//0x10
		char padding0[0x4];					//0x14
		Proc_Stats Threads[3072];			//0x18
	};

	static bool Should_Run_Thread;
	static Proc_Stats Stat_Data[3072];
	static threadUsage gThread_Data[2];

	static void CalculateCpuUsage(unsigned int idle_tid[8], threadUsage* cur, threadUsage* prev, float usage_out[8]);
	static void Get_Page_Table_Stats(int vm, int type, int* Used, int* Free, int* Total);
	static void MonitorThread();

};