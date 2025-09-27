using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;

namespace LCH_RTS_MATCHING.Network;

public static class MatchingServerSessionManager
{
    private static readonly List<PacketSession> AllSessions = [];
    private static readonly List<GameSession> GameServers = [];
    private static readonly List<ClientSession> Clients = [];
    private static readonly Lock Lock = new();

    public static void ForEach(Action<PacketSession> func)
    {
        using (Lock.EnterScope())
        {
            var validSessions = AllSessions.Where(s => s != null).ToList();
            
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
        using (Lock.EnterScope())
        {
            AllSessions.Remove(session);
            
            if (session is ClientSession clientSession)
            {
                Clients.Remove(clientSession);
            }
            else if (session is GameSession gameSession)
            {
                GameServers.Remove(gameSession);
            }
        }
    }

    public static void AddSession(GameSession session)
    {
        using (Lock.EnterScope())
        {
            AllSessions.Add(session);
            GameServers.Add(session);
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
    
    public static void RemoveSession(ClientSession session)
    {
        using (Lock.EnterScope())
        {
            AllSessions.Remove(session);
            Clients.Remove(session);
        }
    }
    
    public static void RemoveSession(GameSession session)
    {
        using (Lock.EnterScope())
        {
            AllSessions.Remove(session);
            GameServers.Remove(session);
        }
    }

    public static GameSession? GetFirstGameServer()
    {
        using (Lock.EnterScope())
        {
            if (GameServers.Count == 0)
            {
                return null;
            }

            return GameServers[0];
        }
    }
}
