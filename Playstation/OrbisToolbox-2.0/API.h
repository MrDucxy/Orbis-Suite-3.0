#pragma once
#include "Proc.h"
#include "Debug.h"
#include "Target.h"
#include "SocketListener.h"
#include "APIHelper.h"

class API
{
private:
	Proc* Proc;
	Debug* Debug;
	Target* Target;
	SocketListener* Listener;

	static void ListenerCallback(void* tdParam, OrbisNetId s);

public:
	API();
	~API();
};