using System.Numerics;
using System.Collections.Generic;
using LCH_COMMON;

namespace LCH_RTS_BOT_TEST;

public class BotSessionManager
{
    public static BotSessionManager Instance { get; } = new();

    private readonly HashSet<ServerSession> _sessions = new();
    private readonly HashSet<GameSession> _gameSessions = new();
    private readonly Lock _lock = new();

    public ServerSession GenerateMatching()
    {
        using(_lock.EnterScope())
        {
            var session = new ServerSession();
            _sessions.Add(session);
            Logger.Log(ELogType.Console, ELogLevel.Info, $"Connected-Matching Total BotCount={_sessions.Count}");
            return session;
        }
    }

    public GameSession GenerateGame(long playerId, long matchId)
    {
        using(_lock.EnterScope())
        {
            var session = new GameSession
            {
                PlayerId = playerId,
                MatchId = matchId
            };
            _gameSessions.Add(session);
            Logger.Log(ELogType.Console, ELogLevel.Info, $"Connected-Game Total:({_gameSessions.Count}) Players");
            return session;
        }
    }
    
    public void RemoveMatchingSession(ServerSession session)
    {
        using(_lock.EnterScope())
        {
            if (_sessions.Remove(session))
            {
                Logger.Log(ELogType.Console, ELogLevel.Info, $"Disconnected-Matching Total:({_sessions.Count}) Players");
            }
        }
    }
    
    public void RemoveGameSession(GameSession session)
    {
        using(_lock.EnterScope())
        {
            if (_gameSessions.Remove(session))
            {
                Logger.Log(ELogType.Console, ELogLevel.Info, $"Disconnected-Game Total:({_gameSessions.Count}) Players");
            }
        }
    }

    public void ForEachSend()
    {
        _gameSessions.ToList().ForEach(ss =>
        {
            if (ss.RoomId != 0)
            {
                ss.Send(PacketUtil.CS_UNIT_SPAWN_Packet(ss.RoomId, unitType: 1, new Vector2(20, 30)));
            }
        });
    }
}