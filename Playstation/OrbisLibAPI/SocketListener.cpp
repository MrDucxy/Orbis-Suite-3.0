#include "Common.h"
#include "SocketListener.h"

struct ClientThreadParams
{
	SocketListener* socketListener;
	OrbisNetId Sock;
};

void* SocketListener::ClientThread(void* tdParam)
{
	ClientThreadParams* Params = (ClientThreadParams*)tdParam;
	SocketListener* socketListener = Params->socketListener;
	OrbisNetId Sock = Params->Sock;

	socketListener->ClientCallBack(socketListener->tdParam, Sock);

	sceNetSocketClose(Sock);
	free(Params);

	// Kill our thread and exit.
	scePthreadExit(NULL);
	return nullptr;
}

void* SocketListener::DoWork()
{
	OrbisNetSockaddrIn addr = { 0 };
	addr.sin_family = ORBIS_NET_AF_INET;
	addr.sin_addr.s_addr = ORBIS_NET_INADDR_ANY; // Any incoming address
	addr.sin_port = sceNetHtons(this->Port); // Our desired listen port

	// Make new TCP Socket
	this->Socket = sceNetSocket("Listener Socket", ORBIS_NET_AF_INET, ORBIS_NET_SOCK_STREAM, ORBIS_NET_IPPROTO_TCP);

	// Set Sending and reciving time out to 1000 ms
	int sock_timeout = 10000;
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
			goto Cleanup;
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
				// Set up thread params.
				ClientThreadParams* Params = new ClientThreadParams();
				Params->socketListener = this;
				Params->Sock = ClientSocket;

				// Create Thread to handle connection.
				OrbisPthread* Thread;
				scePthreadCreate(&Thread, NULL, &ClientThread, Params, "Client Thread");

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

	// Kill our thread and exit.
	scePthreadExit(NULL);
	return nullptr;
}

void* SocketListener::ListenThread(void* tdParam)
{
	return ((SocketListener*)tdParam)->DoWork();
}


SocketListener::SocketListener(void(*ClientCallBack)(void* tdParam, OrbisNetId Sock), void* tdParam, unsigned short Port)
{
	klog("Socket Listener.\n");
	this->ClientCallBack = ClientCallBack;
	this->tdParam = tdParam;
	this->ServerRunning = true;
	this->ThreadCleanedUp = false;
	this->Port = Port;

	scePthreadCreate(&ListenThreadHandle, NULL, &ListenThread, this, "Listen Thread");
}

SocketListener::~SocketListener()
{
	klog("~Socket Listener.\n");

	this->ServerRunning = false;
	while (!this->ThreadCleanedUp) { sceKernelUsleep(10); }

	klog("Destruction sucessful.\n");
}