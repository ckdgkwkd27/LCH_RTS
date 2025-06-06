using UnityEngine;
using System;
using System.Collections.Generic;

public static class SessionManager
{
    private static List<PacketSession> _activeSessions = new List<PacketSession>();
    private static List<PacketSession> _sessionPool = new List<PacketSession>();
    private static uint _issuedCnt = 0;
    private static Func<PacketSession> _sessionBuilder;
    private static readonly object _poolLock = new();
    
    public static void PrepareSessions(uint maxSessionCnt, Func<PacketSession> sessionBuilder)
    {
        lock (_poolLock)    
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
        lock (_poolLock)
        {
            _activeSessions.Remove(session);
            _sessionPool.Add(session);
            --_issuedCnt;
        }
    }

    public static PacketSession AcquireFromPool()
    {
        lock (_poolLock)
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
        lock (_poolLock)
        {
            foreach (var session in _activeSessions)
            {
                session.Send(buffer.WriteSegment);
            }
        }
    }

    public static void ForEach(Action<PacketSession> action)
    {
        lock (_poolLock)
        {
            foreach (var session in _activeSessions)
            {
                action.Invoke(session);
            }
        }
    }
}