using System.Diagnostics;
using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS_MATCHING.Network;

namespace LCH_RTS_MATCHING.MatchMake;

public readonly record struct MatcherInfo(long playerId, int mmr, PacketSession session, long msec)
{
    public long PlayerId { get; } = playerId;
    public int Mmr { get; } = mmr;
    public PacketSession Session { get; } = session;
    public long MatchReqMsec { get; } = msec;
}

public class MatchManager
{
    private static readonly Lazy<MatchManager> _instance = new(() => new MatchManager());
    public static MatchManager Instance => _instance.Value;

    private readonly List<MatcherInfo> _matchQueue = new();
    private const int MatchWindow = 100;
    private long _currMatchId = 1;
    private readonly Lock _lock = new();
    private readonly AutoResetEvent _event = new(false);
    
    public void Enqueue(MatcherInfo info)
    {
        using (_lock.EnterScope())
        {
            _matchQueue.Add(info);
        }
        _event.Set();
    }

    public void ProcessMatching()
    {
        var sw = Stopwatch.StartNew();

        using (_lock.EnterScope())
        {
            if (_matchQueue.Count == 0)
            {
                _event.WaitOne();
            }
        }

        var matches = new List<MatchAssignment>();
        var needToRemoveIndexes = new List<int>();
        var allPlayersMatched = false;
        using (_lock.EnterScope())
        {
            if (_matchQueue.Count >= 2)
            {
                var matchedCache = new bool[_matchQueue.Count];
                for (var i = 0; i < _matchQueue.Count; i++)
                {
                    if (matchedCache[i])
                    {
                        continue;
                    }

                    var firstPlayer = _matchQueue[i];
                    var nowMsec = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    for (var j = i + 1; j < _matchQueue.Count; j++)
                    {
                        if (matchedCache[j])
                        {
                            continue;
                        }

                        var candidate = _matchQueue[j];
                        if (Math.Abs(firstPlayer.Mmr - candidate.Mmr) > CalcMatchWindow(nowMsec, firstPlayer.MatchReqMsec, candidate.MatchReqMsec))
                        {
                            continue;
                        }

                        var gameServer = MatchingServerSessionManager.GetFirstGameServer();
                        if (gameServer is null)
                        {
                            sw.Stop();
                            Logger.Log(ELogType.Console, ELogLevel.Error, $"no game server");
                            return;
                        }

                        var matchId = _currMatchId++;
                        matchedCache[i] = true; matchedCache[j] = true;
                        needToRemoveIndexes.Add(i); needToRemoveIndexes.Add(j);
                        matches.Add(new MatchAssignment(firstPlayer, candidate, gameServer, matchId, nowMsec));
                        break;
                    }
                }
            }

            if (needToRemoveIndexes.Count > 0)
            {
                needToRemoveIndexes.Sort((a, b) => b.CompareTo(a));
                foreach (var index in needToRemoveIndexes)
                {
                    _matchQueue.RemoveAt(index);
                }
            }
            
            if (_matchQueue.Count > 0)
            {
                _event.Set();
            }

            if (matches.Count > 0 && _matchQueue.Count == 0)
            {
                allPlayersMatched = true;
            }
        }

        var ip = NetConfig.Ip;
        var port = NetConfig.GetPort(EPortInfo.GAMESERVER_CLIENT_PORT);
        foreach (var match in matches)
        {
            match.GameServer.Send(PacketUtil.MG_GAME_READY_PACKET(match.MatchId, match.First.PlayerId, match.Second.PlayerId));
            Logger.Log(ELogType.Console, ELogLevel.Info, $"[INFO] MatchId={match.MatchId}, Sent EndPoint: {ip}:{port}");

            if (match.First.Session is ClientSession firstClient)
            {
                if (firstClient.PlayerId == 0)
                {
                    Logger.Log(ELogType.Console, ELogLevel.Warning, $"[WARN] MatchId={match.MatchId}, Missing PlayerId for first client session.");
                }
                else
                {
                    firstClient.Send(PacketUtil.MC_MATCH_JOIN_INFO_PACKET(firstClient.PlayerId, match.MatchId, ip, port));
                }
            }

            if (match.Second.Session is not ClientSession secondClient) 
                continue;
            
            if (secondClient.PlayerId == 0)
            {
                Logger.Log(ELogType.Console, ELogLevel.Warning, $"[WARN] MatchId={match.MatchId}, Missing PlayerId for second client session.");
            }
            else
            {
                secondClient.Send(PacketUtil.MC_MATCH_JOIN_INFO_PACKET(secondClient.PlayerId, match.MatchId, ip, port));
            }
        }

        sw.Stop();
        if (!allPlayersMatched || matches.Count == 0)
        {
            return;
        }

        Logger.Log(ELogType.Console, ELogLevel.Debug, $"Elapsed={sw.Elapsed.TotalMilliseconds:F3}ms");
    }

    private readonly record struct MatchAssignment(MatcherInfo First, MatcherInfo Second, GameSession GameServer, long MatchId, long MatchAddedAt);

    private static long CalcMatchWindow(long nowMsec, long msec1, long msec2)
    {
        const int oneSecInMs = 1000;
        const int matchWindowInc = 100;
        var minMsec = Math.Min(msec1, msec2);
        var matchThreshold = long.Abs(nowMsec - minMsec);
        var value = MatchWindow + matchThreshold / oneSecInMs * matchWindowInc;
        return value;
    }
}
