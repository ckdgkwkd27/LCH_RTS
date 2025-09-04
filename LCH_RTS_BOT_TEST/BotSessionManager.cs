using System.Numerics;

namespace LCH_RTS_BOT_TEST;

public class BotSessionManager
{
    public static BotSessionManager Instance { get; } = new BotSessionManager();

    private readonly HashSet<ServerSession> _sessions = new();
    private readonly HashSet<GameSession> _gameSessions = new();
    private readonly Lock _lock = new();

    public ServerSession GenerateMatching()
    {
        using(_lock.EnterScope())
        {
            var session = new ServerSession();
            _sessions.Add(session);
            Console.WriteLine($"Connected-Matching Total:({_sessions.Count}) Players");
            return session;
        }
    }

    public GameSession GenerateGame()
    {
        using(_lock.EnterScope())
        {
            var session = new GameSession();
            _gameSessions.Add(session);
            Console.WriteLine($"Connected-Game Total:({_gameSessions.Count}) Players");
            Global.ConnectedCnt++;
            return session;
        }
    }
    
    public void RemoveMatchingSession(ServerSession session)
    {
        using(_lock.EnterScope())
        {
            if (_sessions.Remove(session))
            {
                Console.WriteLine($"Disconnected-Matching Total:({_sessions.Count}) Players");
            }
        }
    }
    
    public void RemoveGameSession(GameSession session)
    {
        using(_lock.EnterScope())
        {
            if (_gameSessions.Remove(session))
            {
                Global.ConnectedCnt--;
                Console.WriteLine($"Disconnected-Game Total:({_gameSessions.Count}) Players");
            }
        }
    }

    public void ForEachSend()
    {
        _gameSessions.ToList().ForEach(ss => ss.Send(PacketUtil.CS_UNIT_SPAWN_Packet(ss.RoomId, unitType: 1, new Vector2(20, 30))));
    }
}