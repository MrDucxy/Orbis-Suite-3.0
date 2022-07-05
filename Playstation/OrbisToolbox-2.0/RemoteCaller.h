#pragma once

/// <summary>
/// Class to wrap function calls to a remote thread.
/// </summary>
class RemoteCaller
{
private:
	std::stack<std::function<void()>> CallList;

public:
	template<typename ReturnValue, class Func, typename... Args>
	ReturnValue RemoteCall(Func method, Args... args)
	{
		bool isWaiting = true;
		ReturnValue result = NULL;
		auto internalMethod = std::bind(method, args...); // Bind our call to pass into the lambda easier.

		// Set up lambda for function call.
		auto resultPtr = &result;
		auto isWaitingPtr = &isWaiting;
		CallList.push([resultPtr, internalMethod, isWaitingPtr]() -> void
			{
				*resultPtr = internalMethod();
				*isWaitingPtr = false;
			});

		// Wait while the call is happening.
		while (isWaiting) { sceKernelUsleep(10); }

		// Return our result.
		return result;
	}

	void DoRemoteCalls();
};
