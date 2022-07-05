#include "Common.h"
#include "RemoteCaller.h"

void RemoteCaller::DoRemoteCalls()
{
	// Make sure the stack has some value.
	if (CallList.empty())
		return;

	// Pull the call from the top of the stack.
	auto method = CallList.top();

	// Execute the call in this context.
	method();

	// Pop this call from the stack.
	CallList.pop();
}