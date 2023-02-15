#include "Common.h"
#include "SystemMonitor.h"

OrbisPthread SystemMonitor::ThreadId;
int SystemMonitor::Thread_Count = 0;
int SystemMonitor::Busy_Core = 0;
float SystemMonitor::Usage[8] = { 0 };
float SystemMonitor::Average_Usage;
int SystemMonitor::CPU_Temp;
int SystemMonitor::SOC_Temp;
SystemMonitor::Memory SystemMonitor::RAM;
SystemMonitor::Memory SystemMonitor::VRAM;

bool SystemMonitor::Should_Run_Thread = true;
Proc_Stats SystemMonitor::Stat_Data[3072];
SystemMonitor::threadUsage SystemMonitor::gThread_Data[2];

void SystemMonitor::CalculateCpuUsage(unsigned int idle_tid[8], threadUsage* cur, threadUsage* prev, float usage_out[8])
{
	if (cur->Thread_Count <= 0 || prev->Thread_Count <= 0) //Make sure our banks have threads
		return;

	float CurrentMax = 0.0f;

	//Calculate the Current time difference from the last bank to the current bank.
	float Current_Time_Total = ((prev->current_time.tv_sec + (prev->current_time.tv_nsec / 1000000000.0f)) - (cur->current_time.tv_sec + (cur->current_time.tv_nsec / 1000000000.0f)));

	//Here this could use to be improved but essetially what its doing is finding the thread information for the idle threads using their thread Index stored from before.
	struct Data_s
	{
		Proc_Stats* Cur;
		Proc_Stats* Prev;
	}Data[8];

	for (int i = 0; i < cur->Thread_Count; i++)
	{
		for (int j = 0; j < 8; j++)
		{
			if (idle_tid[j] == cur->Threads[i].td_tid)
				Data[j].Cur = &cur->Threads[i];
		}
	}

	for (int i = 0; i < prev->Thread_Count; i++)
	{
		for (int j = 0; j < 8; j++)
		{
			if (idle_tid[j] == prev->Threads[i].td_tid)
				Data[j].Prev = &prev->Threads[i];
		}
	}

	//Here we loop through each core to calculate the total usage time as its split into user/sustem
	for (int i = 0; i < 8; i++)
	{
		float Prev_Usage_Time = (Data[i].Prev->system_cpu_usage_time.tv_sec + (Data[i].Prev->system_cpu_usage_time.tv_nsec / 1000000.0f));
		Prev_Usage_Time += (Data[i].Prev->user_cpu_usage_time.tv_sec + (Data[i].Prev->user_cpu_usage_time.tv_nsec / 1000000.0f));

		float Cur_Usage_Time = (Data[i].Cur->system_cpu_usage_time.tv_sec + (Data[i].Cur->system_cpu_usage_time.tv_nsec / 1000000.0f));
		Cur_Usage_Time += (Data[i].Cur->user_cpu_usage_time.tv_sec + (Data[i].Cur->user_cpu_usage_time.tv_nsec / 1000000.0f));

		//We calculate the usage using usage time difference between the two samples divided by the current time difference.
		float Idle_Usage = ((Prev_Usage_Time - Cur_Usage_Time) / Current_Time_Total);

		if (Idle_Usage > 1.0f)
			Idle_Usage = 1.0f;

		if (Idle_Usage < 0.0f)
			Idle_Usage = 0.0f;

		//Get inverse of idle percentage and express in percent.
		usage_out[i] = (1.0f - Idle_Usage) * 100.0f;

		if (usage_out[i] > CurrentMax)
		{
			CurrentMax = usage_out[i];
			Busy_Core = i;
		}
	}
}

void SystemMonitor::Get_Page_Table_Stats(int vm, int type, int* Used, int* Free, int* Total)
{
	int _Total = 0, _Free = 0;

	if (get_page_table_stats(vm, type, &_Total, &_Free) == -1) {
		klog("get_page_table_stats() Failed.\n");
		return;
	}

	if (Used)
		*Used = (_Total - _Free);

	if (Free)
		*Free = _Free;

	if (Total)
		*Total = _Total;
}

void* SystemMonitor::MonitorThread(void* args)
{
	//klog("[System Monitor] Thread Started\n");

	unsigned int Idle_Thread_ID[8];

	int Thread_Count = 3072;
	if (!sceKernelGetCpuUsage((Proc_Stats*)&Stat_Data, (int*)&Thread_Count) && Thread_Count > 0)
	{
		char Thread_Name[0x40];
		int Core_Count = 0;
		for (int i = 0; i < Thread_Count; i++)
		{
			if (!sceKernelGetThreadName(Stat_Data[i].td_tid, Thread_Name) && sscanf(Thread_Name, "SceIdleCpu%d", &Core_Count) == 1 && Core_Count <= 7)
			{
				//klog("[System Monitor][SceIdleCpu%d] -> %i\n", Core_Count, Stat_Data[i].td_tid);

				Idle_Thread_ID[Core_Count] = Stat_Data[i].td_tid;
			}
		}
	}

	//klog("[System Monitor] Starting Monitor...\n");
	int Current_Bank = 0;
	while (Should_Run_Thread)
	{
		//grab thread data with max threads of 3072.
		gThread_Data[Current_Bank].Thread_Count = 3072;
		if (!sceKernelGetCpuUsage((Proc_Stats*)&gThread_Data[Current_Bank].Threads, &gThread_Data[Current_Bank].Thread_Count))
		{
			//Store the thread count.
			SystemMonitor::Thread_Count = gThread_Data[Current_Bank].Thread_Count;

			//klog("ThreadCount[%i] = %i\n", Current_Bank, gThread_Data[Current_Bank].Thread_Count);

			//Set the current time.
			sceKernelClockGettime(4, &gThread_Data[Current_Bank].current_time);

			//flip to other bank.
			Current_Bank = !Current_Bank;

			//make sure bank has threads
			if (gThread_Data[Current_Bank].Thread_Count <= 0)
				continue;

			//Calculate usage using thread data.
			CalculateCpuUsage(Idle_Thread_ID, &gThread_Data[!Current_Bank], &gThread_Data[Current_Bank], Usage);

			/*klog("CPU Utilization: %2.0f%% %2.0f%% %2.0f%% %2.0f%% %2.0f%% %2.0f%% %2.0f%% %2.0f%%\n",
				Usage[0], Usage[1], Usage[2], Usage[3],
				Usage[4], Usage[5], Usage[6], Usage[7]);*/

			SystemMonitor::Average_Usage = ((Usage[0] + Usage[1] + Usage[2] + Usage[3] + Usage[4] + Usage[5] + Usage[6] + Usage[7]) / 8.0f);
		}

		// Die temperatures
		sceKernelGetCpuTemperature(&CPU_Temp);
		sceKernelGetSocSensorTemperature(0, &SOC_Temp);

		// Ram Usage.
		Get_Page_Table_Stats(-1, 1, &RAM.Used, &RAM.Free, &RAM.Total);
		RAM.Percentage = (((float)RAM.Used / (float)RAM.Total) * 100.0f);

		// Video Ram Usage.
		Get_Page_Table_Stats(-1, 2, &VRAM.Used, &VRAM.Free, &VRAM.Total);
		VRAM.Percentage = (((float)VRAM.Used / (float)VRAM.Total) * 100.0f);

		sceKernelSleep(2);
	}

	Should_Run_Thread = true;
	klog("[System Monitor] Thread Shutdown.\n");
	void* res;
	scePthreadExit(res);
	return res;
}

void SystemMonitor::Init()
{
	klog("[System Monitor] Starting System Monitor Thread...\n");
	
	scePthreadCreate(&ThreadId, nullptr, MonitorThread, NULL, "System Monitor Thread");
	scePthreadDetach(ThreadId);
}

void SystemMonitor::Term()
{
	Should_Run_Thread = false;
	scePthreadJoin(ThreadId, nullptr);
}