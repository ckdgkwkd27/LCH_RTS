using System.Net.Sockets;

namespace LCH_RTS_CORE_LIB.Network;

public static class SessionManager
{
    private static List<PacketSession> _activeSessions = new List<PacketSession?>();
    private static List<PacketSession> _sessionPool = new List<PacketSession?>();
    private static uint _issuedCnt = 0;
    private static Func<PacketSession> _sessionBuilder;
    private static readonly Lock _poolLock = new Lock();
    
    public static void PrepareSessions(uint maxSessionCnt, Func<PacketSession> sessionBuilder)
    {
        using (_poolLock.EnterScope())     
        {
            _sessionBuilder = sessionBuilder;
            for (var idx = 0; idx < maxSessionCnt; idx++)
            {
                var session = _sessionBuilder.Invoke();
                _sessionPool.Add(session);
            }
        }
    }

    public static void ReturnToPool(PacketSession session)
    {
        using (_poolLock.EnterScope())     
        {
            _activeSessions.Remove(session);
            _sessionPool.Add(session);
            --_issuedCnt;
        }
    }

    public static PacketSession AcquireFromPool()
    {
        using (_poolLock.EnterScope())     
        {
            if (_sessionPool.Count == 0)
            {
                PrepareSessions(100, _sessionBuilder);
            }
            
            var session = _sessionPool[0];
            _sessionPool.RemoveAt(0);
            ++_issuedCnt;
            return session;
        }
    }
    
    public static void Broadcast(PacketBuffer buffer)
    {
        using (_poolLock.EnterScope())     
        {
            foreach (var session in _activeSessions)
            {
                session.Send(buffer.WriteSegment);
            }
        }
    }

    public static void ForEach(Action<PacketSession> action)
    {
        using (_poolLock.EnterScope())     
        {
            foreach (var session in _activeSessions)
            {
                action.Invoke(session);
            }
        }
    }
}