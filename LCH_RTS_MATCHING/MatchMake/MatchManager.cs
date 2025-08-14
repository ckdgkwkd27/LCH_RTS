using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS_MATCHING.Network;

namespace LCH_RTS_MATCHING.MatchMake;

public struct MatcherInfo(long playerId, int mmr, PacketSession session)
{
    public long PlayerId { get; } = playerId;
    public int Mmr { get; } = mmr;
    public PacketSession Session { get; } = session;
}

public class MatchManager
{
    // ReSharper disable once InconsistentNaming
    private static readonly MatchManager _instance = new();
    private MatchManager() { }
    public static readonly MatchManager Instance = _instance;

    private readonly Queue<MatcherInfo> _matchQueue = new();
    private int _matchWindow = 100; // 초기 매칭 윈도우
    private int _lastMatchTriedSecond = DateTime.UtcNow.Second;
    private long _currMatchId = 0;
    
    public void Enqueue(MatcherInfo info)
    {
        _matchQueue.Enqueue(info);
    }

    private const int MATCH_INTERVAL = 3;
    public List<(MatcherInfo, MatcherInfo)>? ProcessMatching()
    {
        if (DateTime.UtcNow.Second - _lastMatchTriedSecond < MATCH_INTERVAL)
        {
            return null;
        }

        if (_matchQueue.Count < 2)
        {
            return null;
        }
        
        var results = new List<(MatcherInfo, MatcherInfo)>();
        var initialWindow = _matchWindow;
        var currentWindow = _matchWindow;
        
        // 큐에 2명 이상의 플레이어가 있을 때만 매칭 시도
        var player1 = _matchQueue.Peek();
        _matchQueue.Dequeue();

        // 현재 윈도우 내에서 매칭 가능한 두 번째 플레이어를 찾음
        MatcherInfo? matchedPlayer = null;
        var tempQueue = new Queue<MatcherInfo>();

        while (_matchQueue.Count > 0)
        {
            var player2 = _matchQueue.Peek();
            _matchQueue.Dequeue();

            // MMR 차이가 현재 윈도우 이내인지 확인
            if (Math.Abs(player1.Mmr - player2.Mmr) <= currentWindow)
            {
                matchedPlayer = player2;
                break;
            }
            else
            {
                // 매칭되지 않은 플레이어는 임시 큐에 저장
                tempQueue.Enqueue(player2);
            }
        }

        // 매칭된 플레이어가 있으면 결과에 추가
        if (matchedPlayer.HasValue)
        {
            // 임시 큐에 저장된 나머지 플레이어들을 원래 큐로 복사
            while (tempQueue.Count > 0)
            {
                _matchQueue.Enqueue(tempQueue.Dequeue());
            }

            // 매칭 성공 시 윈도우를 초기값으로 리셋
            currentWindow = initialWindow;
            
            results.Add((player1, matchedPlayer.Value));
            var resultMatchId = _currMatchId++;
            GameSession.GameServer.Send(PacketUtil.MG_GAME_READY_PACKET(resultMatchId, player1.PlayerId, matchedPlayer.Value.PlayerId));

            //#TODO: GameServer의 MatchId, EndPoint 정보를 전달한다.
            var localEndpoint = GameSession.GameServer.GetLocalEndPoint();
            if (localEndpoint is null)
            {
                Console.WriteLine("[ERROR] GameServer Session is null");
                return null;
            }
            
            var ip = (localEndpoint as IPEndPoint)!.Address.ToString();
            var port = (localEndpoint as IPEndPoint)!.Port;
            player1.Session.Send(PacketUtil.MC_MATCH_JOIN_INFO_PACKET(resultMatchId, ip, port));
            matchedPlayer.Value.Session.Send(PacketUtil.MC_MATCH_JOIN_INFO_PACKET(resultMatchId, ip, port));
        }
        else
        {
            // 매칭되지 않은 플레이어를 다시 큐에 추가
            _matchQueue.Enqueue(player1);

            // 임시 큐에 저장된 나머지 플레이어들을 원래 큐로 복사
            while (tempQueue.Count > 0)
            {
                _matchQueue.Enqueue(tempQueue.Dequeue());
            }

            // 더 이상 현재 윈도우에서 매칭할 수 없으면 윈도우를 확장
            currentWindow += 100;
        }
        
        // 매칭 윈도우 업데이트
        _matchWindow = currentWindow;

        _lastMatchTriedSecond = DateTime.UtcNow.Second;
        return results;
    }
}