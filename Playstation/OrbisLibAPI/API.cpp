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
	auto packet = RecievePacket<APIPacket>(s);

	// Did we recieve a packet?
	if (packet == nullptr)
	{
		return;
	}

	// Validate Packet
	if (strcmp(packet->PacketMagic, "ORBIS_SUITE") || packet->PacketVersion != PACKET_VERSION)
	{
		klog("Invalid Packet with Magic '%s' and Version %i\nExpected 'ORBIS_SUITE' and %i\n", packet->PacketMagic, packet->PacketVersion, PACKET_VERSION);

		return;
	}

	//if (Packet->Command > 0 && Packet->Command <= ARRAY_COUNT(CommandList))
	//{
	//	klog("Recieved the Command %i(%s)\n", Packet->Command, CommandList[Packet->Command]);
	//}

	// Add host to the host list.
	Events::AddHost(sin_addr.s_addr);

	// Send out the command to the right places.
	switch (packet->Command)
	{
	default:
		klog("API: Invalid Command %i...\n", packet->Command);
		break;

	case APICommands::PROC_START ... APICommands::PROC_END:
		Proc->HandleAPI(s, packet);
		break;

	case APICommands::APP_START ... APICommands::APP_END:
		Apps->HandleAPI(s, packet);
		break;

	case APICommands::DBG_START ... APICommands::DBG_END:
		Debug->HandleAPI(s, packet);
		break;

	case APICommands::KERN_START ... APICommands::KERN_END:
		klog("Kernel API Call\n");

		break;

	case APICommands::TARGET_START ... APICommands::TARGET_END:
		Target->HandleAPI(s, packet);
		break;

	}
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