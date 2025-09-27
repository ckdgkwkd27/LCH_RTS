using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS_MATCHING.Network;

namespace LCH_RTS_MATCHING.MatchMake;

public readonly struct MatcherInfo(long playerId, int mmr, PacketSession session)
{
    public long PlayerId { get; } = playerId;
    public int Mmr { get; } = mmr;
    public PacketSession Session { get; } = session;
}

public class MatchManager
{
    private static readonly Lazy<MatchManager> _instance = new(() => new MatchManager());
    public static MatchManager Instance => _instance.Value;

    private readonly List<MatcherInfo> _matchQueue = new();
    private int _matchWindow = 100;
    private long _lastMatchTriedMSec = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    private long _currMatchId = 1;
    private readonly Lock _lock = new();
    
    public void Enqueue(MatcherInfo info)
    {
        using (_lock.EnterScope())
        {
            _matchQueue.Add(info);
        }
    }

    private const int MATCH_INTERVAL_MSEC = 100;
    public void ProcessMatching()
    {
        if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastMatchTriedMSec < MATCH_INTERVAL_MSEC)
        {
            return;
        }

        if (_matchQueue.Count < 2)
        {
            return;
        }

        var initialWindow = _matchWindow;
        var currentWindow = _matchWindow;

        while (_matchQueue.Count >= 2)
        {
            var firstPlayer = _matchQueue.First();

            MatcherInfo? matchedPlayer = null;
            for(var idx = 1; idx < _matchQueue.Count; idx++)
            {
                var player2 = _matchQueue.ElementAt(idx);
                if (Math.Abs(firstPlayer.Mmr - player2.Mmr) <= currentWindow)
                {
                    matchedPlayer = player2;
                    break;
                }
                idx++;
            }

            if (matchedPlayer.HasValue)
            {
                currentWindow = initialWindow;

                var resultMatchId = _currMatchId++;
                var gs = MatchingServerSessionManager.GetFirstGameServer();
                if (gs is null) return;
                gs.Send(PacketUtil.MG_GAME_READY_PACKET(resultMatchId, firstPlayer.PlayerId, matchedPlayer.Value.PlayerId));

                var ip = NetConfig.Ip;
                var port = NetConfig.GetPort(EPortInfo.GAMESERVER_CLIENT_PORT);
                Logger.Log(ELogType.Console, ELogLevel.Info, $"[INFO] MatchId={resultMatchId}, Sent EndPoint: {ip}:{port}");

                var firstPlayerId = (firstPlayer.Session as ClientSession)!.PlayerId;
                Debug.Assert(firstPlayerId != 0);
                firstPlayer.Session.Send(PacketUtil.MC_MATCH_JOIN_INFO_PACKET(firstPlayerId, resultMatchId, ip, port));
                
                var matchedPlayerId = (matchedPlayer.Value.Session as ClientSession)!.PlayerId;
                Debug.Assert(matchedPlayerId != 0);
                matchedPlayer.Value.Session.Send(PacketUtil.MC_MATCH_JOIN_INFO_PACKET(matchedPlayerId, resultMatchId, ip, port));

                using (_lock.EnterScope())
                {
                    _matchQueue.Remove(firstPlayer);
                    _matchQueue.Remove(matchedPlayer.Value);
                }
            }
            else
            {
                currentWindow += 100;
                break;
            }
        }

        _matchWindow = currentWindow;
        _lastMatchTriedMSec = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}