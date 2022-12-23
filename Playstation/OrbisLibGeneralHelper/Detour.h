#pragma once

class Detour
{
private:

	void* StubPtr = 0;
	size_t StubSize = 0;

	void* FunctionPtr = 0;
	void* HookPtr = 0;

public:
	template <typename result, typename... Args>
	result Stub(Args... args)
	{
		result(*Stub_internal)(Args... args) = decltype(Stub_internal)(StubPtr);
		return Stub_internal(args...);
	}

	void WriteJump(void* Address, void* Destination);
	void* DetourFunction(uint64_t FunctionPtr, void* HookPtr);
	void RestoreFunction();

	Detour();
	~Detour();
};