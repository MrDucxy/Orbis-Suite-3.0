#pragma once
#include "Proc.h"
#include "Debug.h"
#include "Target.h"
#include "SocketListener.h"
#include "APIHelper.h"

class API
{
private:
	static Proc* Proc;
	static Debug* Debug;
	static Target* Target;
	static SocketListener* Listener;

	static void ListenerCallback(void* tdParam, OrbisNetId s);

public:
	static void Init();
	static void Term();
};