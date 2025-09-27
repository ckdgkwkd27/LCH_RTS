using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;

namespace LCH_RTS.Network;

public static class GameServerSessionManager
{
    private static readonly List<PacketSession> AllSessions = [];
    private static readonly List<ClientSession> Clients = [];
    private static MatchingSession? Matching;
    private static readonly Lock Lock = new();
    
    public static void ForEach(Action<PacketSession> func)
    {
        using (Lock.EnterScope())
        {
            var validSessions = AllSessions.Where(s => true).ToList();
            
            foreach (var session in validSessions)
            {
                try
                {
                    func(session);
                }
                catch (Exception ex)
                {
                    Logger.Log(ELogType.Console, ELogLevel.Error, $"[ERROR] Session FlushSend failed: {ex.Message}");
                    RemoveInvalidSession(session);
                }
            }
        }
    }

    private static void RemoveInvalidSession(PacketSession session)
    {
        AllSessions.Remove(session);

        switch (session)
        {
            case ClientSession clientSession:
                Clients.Remove(clientSession);
                break;
            case MatchingSession:
                Matching = null;
                break;
        }
    }

    public static void AddSession(ClientSession session)
    {
        using (Lock.EnterScope())
        {
            AllSessions.Add(session);
            Clients.Add(session);
        }
    }

    public static void AddSession(MatchingSession session)
    {
        using (Lock.EnterScope())
        {
            AllSessions.Add(session);
            Matching = session;
        }
    }

    public static void RemoveSession(ClientSession session)
    {
        using (Lock.EnterScope())
        {
            AllSessions.Remove(session);
            Clients.Remove(session);
        }
    }
    
    public static void RemoveSession(MatchingSession session)
    {
        using (Lock.EnterScope())
        {
            AllSessions.Remove(session);
            Matching = null;
        }
    }
}