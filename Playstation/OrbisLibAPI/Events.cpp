#include "Common.h"
#include "Events.h"

std::vector< OrbisNetInAddr_t> Events::HostList;
OrbisPthreadMutex Events::HostListMutex;

bool Events::Init()
{
	// Create the mutex to protect our host list.
	auto res = scePthreadMutexInit(&HostListMutex, nullptr, "EventsHostListMutex");

	if (res != 0)
	{
		klog("Failed to create mutex for Events Host list. Err: 0x%llX\n", res);

		return false;
	}
	else
	{
		return true;
	}
}

void Events::Term()
{
	// Destroy for clean up.
	scePthreadMutexDestroy(&HostListMutex);
}

void Events::AddHost(OrbisNetInAddr_t HostAddress)
{
	// Aquire a lock for the list.
	scePthreadMutexLock(&HostListMutex);

	// Add the host to the list if it does not exist already.
	if (std::find(HostList.begin(), HostList.end(), HostAddress) == HostList.end()) 
	{
		klog("New host (%i.%i.%i.%i)!\n", HostAddress & 0xFF, (HostAddress >> 8) & 0xFF, (HostAddress >> 16) & 0xFF, (HostAddress >> 24) & 0xFF);
		HostList.push_back(HostAddress);
	}

	// Revoke the lock on the list.
	scePthreadMutexUnlock(&HostListMutex);
}

void Events::RemoveHost(OrbisNetInAddr_t HostAddress)
{
	// Aquire a lock for the list.
	scePthreadMutexLock(&HostListMutex);

	// Remove this host if it exists in the list.
	if (std::find(HostList.begin(), HostList.end(), HostAddress) != HostList.end()) 
	{
		klog("Lost host (%i.%i.%i.%i)!\n", HostAddress & 0xFF, (HostAddress >> 8) & 0xFF, (HostAddress >> 16) & 0xFF, (HostAddress >> 24) & 0xFF);
		std::remove(HostList.begin(), HostList.end(), HostAddress);
	}

	// Revoke the lock on the list.
	scePthreadMutexUnlock(&HostListMutex);
}

OrbisNetId Events::Connect(OrbisNetInAddr_t HostAddress)
{
	// Set up socket params.
	OrbisNetSockaddrIn addr = { 0 };
	addr.sin_family = ORBIS_NET_AF_INET;
	addr.sin_addr.s_addr = HostAddress;
	addr.sin_port = sceNetHtons(EVENT_PORT);

	// Create socket.
	auto sock = sceNetSocket("SendEventSock", ORBIS_NET_AF_INET, ORBIS_NET_SOCK_STREAM, ORBIS_NET_IPPROTO_TCP);
	if (sock < 0)
	{
		klog("Events::Connect() Failed to allocate sock: %llX %llX\n", sock, *sceNetErrnoLoc());
		return NULL;
	}

	int nopipe = 1;
	sceNetSetsockopt(sock, ORBIS_NET_SOL_SOCKET, ORBIS_NET_SO_NOSIGPIPE, &nopipe, sizeof(nopipe));

	// Set connection time out to 4s.
	int sock_timeout = 4000000;
	sceNetSetsockopt(sock, ORBIS_NET_SOL_SOCKET, ORBIS_NET_SO_CONNECTTIMEO, &sock_timeout, sizeof(sock_timeout));
	sceNetSetsockopt(sock, ORBIS_NET_SOL_SOCKET, ORBIS_NET_SO_SNDTIMEO, &sock_timeout, sizeof(sock_timeout));
	sceNetSetsockopt(sock, ORBIS_NET_SOL_SOCKET, ORBIS_NET_SO_RCVTIMEO, &sock_timeout, sizeof(sock_timeout));

	auto res = sceNetConnect(sock, (OrbisNetSockaddr*)&addr, sizeof(addr));
	if (!res)
		return sock;
	else
	{
		klog("Events::Connect() sceNetConnect(): %llX %llX\n", res, *sceNetErrnoLoc());
		return NULL;
	}
}

void Events::SendEvent(int EventId, int pid)
{
	klog("SendEvent()\n");

	if (HostList.empty())
	{
		klog("SendEvent(): Host List Empty :(\n");
		return;
	}

	for (const auto& host : HostList)
	{
		klog("SendEvent(): Sending for host %i.%i.%i.%i\n", host & 0xFF, (host >> 8) & 0xFF, (host >> 16) & 0xFF, (host >> 24) & 0xFF);

		// Aquire a lock for the list.
		scePthreadMutexLock(&HostListMutex);

		auto sock = Connect(host);
		if (sock)
		{
			klog("Sending Event: %i\n", EventId);

			// Send EventId
			Sockets::SendInt(sock, EventId);

			if (EventId == EVENT_ATTACH && pid != -1)
			{
				Sockets::SendInt(sock, pid);
			}

			// Close the socket.
			sceNetSocketClose(sock);

			// Revoke the lock on the list.
			scePthreadMutexUnlock(&HostListMutex);
		}
		else
		{
			klog("SendEvent(): Failed to connect to host %i.%i.%i.%i\n", host & 0xFF, (host >> 8) & 0xFF, (host >> 16) & 0xFF, (host >> 24) & 0xFF);

			// Revoke the lock on the list.
			scePthreadMutexUnlock(&HostListMutex);

			RemoveHost(host);
		}
	}
}