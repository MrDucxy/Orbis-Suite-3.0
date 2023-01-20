#include "Common.h"
#include "LocalSocketListener.h"
#include <sys/un.h>

void* LocalSocketListener::ClientThread(void* tdParam)
{
	ClientThreadParams* Params = (ClientThreadParams*)tdParam;
	LocalSocketListener* localSocketListener = Params->LocalSocketListener;
	OrbisNetId Sock = Params->Sock;

	localSocketListener->ClientCallBack(localSocketListener->tdParam, Sock);

	sceNetSocketClose(Sock);
	delete Params;

	// Kill our thread and exit.
	scePthreadDetach(scePthreadSelf());
	scePthreadExit(NULL);
	return nullptr;
}

void* LocalSocketListener::DoWork()
{
	Jailbreak();

	OrbisNetSockaddrUn addr = { 0 };
	addr.sun_family = ORBIS_NET_AF_LOCAL;
	strncpy(addr.sun_path, this->ServerAddress, sizeof(addr.sun_path));

	// Make new local Socket
	this->Socket = sceNetSocket("Local Listener Socket", ORBIS_NET_AF_LOCAL, ORBIS_NET_SOCK_STREAM, 0);

	// Set Sending and reciving time out to 1000 ms
	int sock_timeout = 10000;
	sceNetSetsockopt(this->Socket, ORBIS_NET_SOL_SOCKET, ORBIS_NET_SO_SNDTIMEO, &sock_timeout, sizeof(sock_timeout));
	sceNetSetsockopt(this->Socket, ORBIS_NET_SOL_SOCKET, ORBIS_NET_SO_RCVTIMEO, &sock_timeout, sizeof(sock_timeout));

	// Make sure every time we can rebind to the port.
	int reusePort = 1;
	sceNetSetsockopt(this->Socket, ORBIS_NET_SOL_SOCKET, ORBIS_NET_SO_REUSEPORT, &reusePort, sizeof(reusePort));

	auto bindError = sceNetBind(this->Socket, (OrbisNetSockaddr*)&addr, SUN_LEN(&addr));
	if (bindError != 0)
	{
		klog("Failed to bind Listener to address %s\nError: %X", this->ServerAddress, bindError);

		goto Cleanup;
	}

	RestoreJail();

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
				Params->LocalSocketListener = this;
				Params->Sock = ClientSocket;

				// Create Thread to handle connection.
				OrbisPthread* Thread;
				scePthreadCreate(&Thread, NULL, &ClientThread, Params, "Client Thread");
				scePthreadDetach(*Thread);

				// Reset ClientSocket.
				ClientSocket = -1;
			}
		}
	}

Cleanup:
	klog("Listener Thread Exiting!\n");

	// Clean up.
	sceNetSocketClose(this->Socket);
	sceKernelUnlink(this->ServerAddress);

	// Kill our thread and exit.
	scePthreadExit(NULL);
	return nullptr;
}

void* LocalSocketListener::ListenThread(void* tdParam)
{
	return ((LocalSocketListener*)tdParam)->DoWork();
}

LocalSocketListener::LocalSocketListener(void(*ClientCallBack)(void* tdParam, OrbisNetId Sock), void* tdParam, char* ServerAddress)
{
	klog("Socket Listener.\n");
	this->ClientCallBack = ClientCallBack;
	this->tdParam = tdParam;
	this->ServerRunning = true;
	strcpy(this->ServerAddress, ServerAddress);

	scePthreadCreate(&ListenThreadHandle, NULL, &ListenThread, this, "Local Listen Thread");
	scePthreadDetach(*ListenThreadHandle);
}

LocalSocketListener::~LocalSocketListener()
{
	klog("~Socket Listener.\n");

	this->ServerRunning = false;
	scePthreadJoin(*ListenThreadHandle, nullptr);

	klog("Destruction sucessful.\n");
}