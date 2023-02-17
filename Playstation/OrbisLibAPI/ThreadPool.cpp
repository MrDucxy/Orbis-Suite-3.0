#include "Common.h"
#include "ThreadPool.h"

bool ThreadPool::ShouldRun;
std::mutex ThreadPool::JobQueueMtx;
std::condition_variable ThreadPool::MtxCondition;
std::vector<std::thread> ThreadPool::ThreadsPool;
std::queue<std::function<void()>> ThreadPool::JobQueue;

void ThreadPool::WorkingLoop()
{
	while (true)
	{
		std::function<void()> job;
		{
			std::unique_lock<std::mutex> lock(JobQueueMtx);
			MtxCondition.wait(lock,
				[]
				{
					return !JobQueue.empty() || !ShouldRun;
				});

			if (!ShouldRun)
				return;

			job = JobQueue.front();
			JobQueue.pop();
		}
		job();
	}
}

void ThreadPool::Init(int poolSize)
{
	ShouldRun = true;
	ThreadsPool.resize(poolSize);
	for (int i = 0; i < poolSize; i++)
	{
		ThreadsPool.at(i) = std::thread(WorkingLoop);
	}
}

void ThreadPool::Term()
{
	{
		std::unique_lock<std::mutex> lock(JobQueueMtx);
		ShouldRun = false;
	}

	MtxCondition.notify_all();

	for (std::thread& activeThread : ThreadsPool)
	{
		activeThread.join();
	}

	ThreadsPool.clear();
}

void ThreadPool::QueueJob(const std::function<void()>& job)
{
	{
		std::unique_lock<std::mutex> lock(JobQueueMtx);
		JobQueue.push(job);
	}
	MtxCondition.notify_one();
}