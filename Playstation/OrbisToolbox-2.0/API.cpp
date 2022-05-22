#include "Common.h"
#include "API.h"

Proc* API::Proc;
Debug* API::Debug;
Target* API::Target;
SocketListener* API::Listener;
bool API::Running = false;

void API::ListenerCallback(void* tdParam, OrbisNetId s)
{
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
			Proc->HandleAPI(s, Packet);
			break;

		case APICommands::DBG_START ... APICommands::DBG_END:
			Debug->HandleAPI(s, Packet);
			break;

		case APICommands::KERN_START ... APICommands::KERN_END:
			klog("Kernel API Call\n");

			break;

		case APICommands::TARGET_START ... APICommands::TARGET_END:
			Target->HandleAPI(s, Packet);
			break;

		}
	}

	// Clean up. :)
	free(Packet);
}

void API::Init()
{
	if (!Running)
	{
		klog("API Startup.\n");
		Proc = new class Proc();
		Debug = new class Debug();
		Target = new class Target();
		Listener = new SocketListener(ListenerCallback, NULL, 6900);
		Running = true;
	}
}

void API::Term()
{
	if (Running)
	{
		delete Proc;
		delete Debug;
		delete Target;
		delete Listener;
		Running = false;
	}
}