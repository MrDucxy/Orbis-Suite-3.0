#pragma once
#include <mutex>
#include <vector>
#include <queue>
#include <functional>
#include <thread> 

class ThreadPool
{
public:
	static void Init(int poolSize);
	static void Term();
	static void QueueJob(const std::function<void()>& job);

private:
	static void WorkingLoop();

	static bool ShouldRun;
	static std::mutex JobQueueMtx;
	static std::condition_variable MtxCondition;
	static std::vector<std::thread> ThreadsPool;
	static std::queue<std::function<void()>> JobQueue;
};
