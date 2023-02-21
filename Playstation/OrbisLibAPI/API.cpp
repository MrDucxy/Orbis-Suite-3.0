#include "Common.h"
#include "API.h"
#include "Events.h"

Proc* API::Proc;
Apps* API::Apps;
Debug* API::Debug;
Target* API::Target;
SocketListener* API::Listener;
bool API::Running = false;

void API::ListenerCallback(void* tdParam, OrbisNetId s, OrbisNetInAddr sin_addr)
{
	// Deserialize the packet.
	auto Packet = RecievePacket<APIPacket>(s);

	// Did we recieve a packet?
	if (Packet == nullptr)
	{
		return;
	}

	// Validate Packet
	if (strcmp(Packet->PacketMagic, "ORBIS_SUITE") || Packet->PacketVersion != PACKET_VERSION)
	{
		klog("Invalid Packet with Magic '%s' and Version %i\nExpected 'ORBIS_SUITE' and %i\n", Packet->PacketMagic, Packet->PacketVersion, PACKET_VERSION);

		free(Packet);

		return;
	}

	//if (Packet->Command > 0 && Packet->Command <= ARRAY_COUNT(CommandList))
	//{
	//	klog("Recieved the Command %i(%s)\n", Packet->Command, CommandList[Packet->Command]);
	//}

	// Add host to the host list.
	Events::AddHost(sin_addr.s_addr);

	// Send out the command to the right places.
	switch (Packet->Command)
	{
	default:
		klog("API: Invalid Command %i...\n", Packet->Command);
		break;

	case APICommands::PROC_START ... APICommands::PROC_END:
		Proc->HandleAPI(s, Packet);
		break;

	case APICommands::APP_START ... APICommands::APP_END:
		Apps->HandleAPI(s, Packet);
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

	// Clean up. :)
	free(Packet);
}

void API::Init()
{
	if (!Running)
	{
		klog("API Startup.\n");
		Proc = new class Proc();
		Apps = new class Apps();
		Debug = new class Debug();
		Target = new class Target(Debug);
		Listener = new SocketListener(ListenerCallback, NULL, API_PORT);
		Running = true;
	}

	while (Running)
	{
		sceKernelSleep(1);
	}
}

void API::Term()
{
	if (Running)
	{
		delete Proc;
		delete Apps;
		delete Debug;
		delete Target;
		delete Listener;
		Running = false;
	}
}