#pragma once

class Events
{
public:
    enum EventId
    {
        EVENT_EXCEPTION,
        EVENT_CONTINUE,
        EVENT_DIE,
        EVENT_ATTACH,
        EVENT_DETACH,
        EVENT_SUSPEND,
        EVENT_RESUME,
        EVENT_SHUTDOWN,
    };

    static bool Init();
    static void Term();

    static void AddHost(OrbisNetInAddr_t HostAddress);
    static void RemoveHost(OrbisNetInAddr_t HostAddress);
    static void SendEvent(int EventId, int pid = -1);

private:
    static std::vector< OrbisNetInAddr_t> HostList;
    static OrbisPthreadMutex HostListMutex;

    static OrbisNetId Connect(OrbisNetInAddr_t HostAddress);
};

