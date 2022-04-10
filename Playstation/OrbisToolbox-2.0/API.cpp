#include "Common.h"
#include "API.h"

void API::ListenerCallback(void* tdParam, OrbisNetId s)
{
	auto API = (class API*)tdParam;
	auto Packet = RecievePacket<APIPacket>(s);

	if (Packet != nullptr)
	{
		// Make sure were getting the proper packet version.
		if (Packet->PacketVersion != PACKET_VERSION)
		{
			//klog("Packet version %i does not match our expected %i version!\n", Packet->PacketVersion, PACKET_VERSION);

			free(Packet);

			return;
		}

		// Send out the command to the right places.
		switch (Packet->Command)
		{
		default:
			klog("API: Invalid Command %i...\n", Packet->Command);
			break;

		case APICommands::PROC_START ... APICommands::PROC_END:
			API->Proc->HandleAPI(s, Packet);
			break;

		case APICommands::DBG_START ... APICommands::DBG_END:
			API->Debug->HandleAPI(s, Packet);
			break;

		case APICommands::KERN_START ... APICommands::KERN_END:
			klog("Kernel API Call\n");

			break;

		case APICommands::TARGET_START ... APICommands::TARGET_END:
			API->Target->HandleAPI(s, Packet);
			break;

		}
	}

	// Clean up. :)
	free(Packet);
}

API::API()
{
	klog("API Startup.\n");
	Proc = new class Proc();
	Debug = new class Debug();
	Target = new class Target();
	Listener = new SocketListener(ListenerCallback, this, 6900);
}

API::~API()
{
	delete Proc;
	delete Debug;
	delete Target;
	delete Listener;
}