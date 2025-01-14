#include "Common.h"
#include "ThreadPool.h"
#include "SocketListener.h"

void SocketListener::ListenThread()
{
	OrbisNetSockaddrIn addr = { 0 };
	addr.sin_family = ORBIS_NET_AF_INET;
	addr.sin_addr.s_addr = ORBIS_NET_INADDR_ANY; // Any incoming address
	addr.sin_port = sceNetHtons(this->Port); // Our desired listen port

	// Make new TCP Socket
	this->Socket = sceNetSocket("Listener Socket", ORBIS_NET_AF_INET, ORBIS_NET_SOCK_STREAM, ORBIS_NET_IPPROTO_TCP);

	// Set Sending and reciving time out to 2s
	int sock_timeout = 2000000;
	sceNetSetsockopt(this->Socket, ORBIS_NET_SOL_SOCKET, ORBIS_NET_SO_SNDTIMEO, &sock_timeout, sizeof(sock_timeout));
	sceNetSetsockopt(this->Socket, ORBIS_NET_SOL_SOCKET, ORBIS_NET_SO_RCVTIMEO, &sock_timeout, sizeof(sock_timeout));

	// Make sure every time we can rebind to the port.
	int reusePort = 1;
	sceNetSetsockopt(this->Socket, ORBIS_NET_SOL_SOCKET, ORBIS_NET_SO_REUSEPORT, &reusePort, sizeof(reusePort));

	auto bindError = sceNetBind(this->Socket, (OrbisNetSockaddr*)&addr, sizeof(addr));
	if (bindError != 0)
	{
		klog("Failed to bind Listener to port %i\nError: %X", this->Port, bindError);

		goto Cleanup;
	}

	if (sceNetListen(this->Socket, 100) != 0)
	{
		klog("Failed to start listening on Socket.\n");

		goto Cleanup;
	}

	while (this->ServerRunning)
	{
		fd_set set;
		struct timeval timeout;
		FD_ZERO(&set); /* clear the set */
		FD_SET(this->Socket, &set); /* add our file descriptor to the set */

		timeout.tv_sec = 2;
		timeout.tv_usec = 0;

		// Wait for incoming connections.
		auto rv = select((int)this->Socket + 1, &set, NULL, NULL, &timeout);
		if (rv == -1)
		{
			goto Cleanup;
		}
		else if (rv == 0)
		{
			if (!this->ServerRunning)
				goto Cleanup;
			continue;
		}
		else 
		{
			if (!this->ServerRunning)
				goto Cleanup;

			OrbisNetSockaddrIn ClientAddr = { 0 };
			OrbisNetSocklen_t addrlen = sizeof(OrbisNetSockaddrIn);
			auto ClientSocket = sceNetAccept(this->Socket, (OrbisNetSockaddr*)&ClientAddr, &addrlen);

			if (ClientSocket != -1)
			{
				//klog("New Connection from %i.%i.%i.%i!\n", ClientAddr.sin_addr.s_addr & 0xFF, (ClientAddr.sin_addr.s_addr >> 8) & 0xFF, (ClientAddr.sin_addr.s_addr >> 16) & 0xFF, (ClientAddr.sin_addr.s_addr >> 24) & 0xFF);

				int optval = 1;
				sceNetSetsockopt(ClientSocket, ORBIS_NET_SOL_SOCKET, ORBIS_NET_SO_NOSIGPIPE, &optval, sizeof(optval));

				ThreadPool::QueueJob([=]
					{
						auto sock = ClientSocket;
						auto addr = ClientAddr.sin_addr;

						ClientCallBack(tdParam, sock, addr);
						sceNetSocketClose(sock);
					});

				// Reset ClientSocket.
				ClientSocket = -1;
			}
		}
	}

Cleanup:
	klog("Listener Thread Exiting!\n");

	// Clean up.
	this->ThreadCleanedUp = true;

	// Clean up.
	sceNetSocketClose(this->Socket);
}

SocketListener::SocketListener(void(*ClientCallBack)(void* tdParam, OrbisNetId Sock, OrbisNetInAddr sin_addr), void* tdParam, unsigned short Port)
{
	this->ClientCallBack = ClientCallBack;
	this->tdParam = tdParam;
	this->ServerRunning = true;
	this->ThreadCleanedUp = false;
	this->Port = Port;

	ThreadPool::QueueJob([this] { ListenThread(); });
}

SocketListener::~SocketListener()
{
	klog("~Socket Listener.\n");

	this->ServerRunning = false;

	klog("Destruction sucessful.\n");
}