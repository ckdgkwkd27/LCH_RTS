using Google.FlatBuffers;
using System;
using System.Diagnostics;
using System.Net;
using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;

namespace LCH_RTS_BOT_TEST;

public class PacketHandler
{
    public static void SC_LOGIN_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_LOGIN.GetRootAsSC_LOGIN(new ByteBuffer(buffer.Array, buffer.Offset));
        var roomId = packet.RoomId;
        if (session is not GameSession gs) return;
        gs.RoomId = roomId;
        session.Send(PacketUtil.CS_ENTER_GAME_Packet(gs.PlayerId, roomId));
    }
    
    public static void SC_ENTER_GAME_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_ENTER_GAME.GetRootAsSC_ENTER_GAME(new ByteBuffer(buffer.Array, buffer.Offset));
        var gs = session as GameSession;
        gs.PlayerId = packet.PlayerId;
        gs.PlayerSide = packet.PlayerSide;
    }
    
    public static void SC_START_GAME_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_START_GAME.GetRootAsSC_START_GAME(new ByteBuffer(buffer.Array, buffer.Offset));
        Global.IncStartedCnt();
        if (Global.IsAllGameStarted())
        {
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    public static void SC_UNIT_SPAWN_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_UNIT_SPAWN.GetRootAsSC_UNIT_SPAWN(new ByteBuffer(buffer.Array, buffer.Offset));
        var gs = session as GameSession;
        
        // 타워(unitType 2,3)는 제외하고 자신이 소환한 유닛에 대해서만 로그 출력
        if (gs.PlayerSide == packet.PlayerSide && packet.UnitType != 2 && packet.UnitType != 3)
        {
            Logger.Log(ELogType.Console, ELogLevel.Info, $"RoomId={gs.RoomId}, PlayerId={gs.PlayerId}, PlayerSide={gs.PlayerSide}, spawned UnitType={packet.UnitType}");
        }
    }

    public static void SC_UNIT_MOVE_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
    }

    public static void SC_UNIT_ATTACK_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
    }

    public static void SC_REMOVE_UNIT_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
    }

    public static void SC_END_GAME_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_END_GAME.GetRootAsSC_END_GAME(new ByteBuffer(buffer.Array, buffer.Offset));
        Global.DecStartedCnt();
        var gs = session as GameSession;
        gs.Disconnect();
        Logger.Log(ELogType.Console, ELogLevel.Info, $"RoomId={packet.RoomId}, Winner={packet.WinnerPlayerSide}, Loser={packet.LoserPlayerSide}");
    }

    public static void SC_PLAYER_COST_UPDATE_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
    }

    public static void SC_PLAYER_HAND_UPDATE_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
    }

    public static void MC_MATCH_JOIN_INFO_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = MC_MATCH_JOIN_INFO.GetRootAsMC_MATCH_JOIN_INFO(new ByteBuffer(buffer.Array, buffer.Offset));

        var playerId = packet.PlayerId;
        var matchId = packet.MatchId;
        Logger.Log(ELogType.Console, ELogLevel.Info, $"PlayerId={playerId}, MatchId={matchId}, IP={packet.Ip}, Port={packet.Port}");
        var endPoint = new IPEndPoint(IPAddress.Parse(packet.Ip), packet.Port);
        var connector = new Connector();
        connector.Connect(endPoint, () => BotSessionManager.Instance.GenerateGame(playerId, matchId));
    }

    public static long? PlayerId;
    public static void MC_PLAYER_REGISTERED_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = MC_PLAYER_REGISTERED.GetRootAsMC_PLAYER_REGISTERED(new ByteBuffer(buffer.Array, buffer.Offset));
        var playerId = packet.PlayerId;
        var ss = session as ServerSession;
        PlayerId = packet.PlayerId;
    }
}