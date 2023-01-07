#pragma once
#include "Proc.h"
#include "Apps.h"
#include "Debug.h"
#include "Target.h"
#include "SocketListener.h"
#include "APIHelper.h"

class API
{
private:
	static Proc* Proc;
	static Apps* Apps;
	static Debug* Debug;
	static Target* Target;
	static SocketListener* Listener;

	static void ListenerCallback(void* tdParam, OrbisNetId s, OrbisNetInAddr sin_addr);

public:
	static bool Running;
	static void Init();
	static void Term();
};