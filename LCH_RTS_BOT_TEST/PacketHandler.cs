using Google.FlatBuffers;
using System;
using System.Net;

namespace LCH_RTS_BOT_TEST;

public class PacketHandler
{
    public static void SC_LOGIN_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_LOGIN.GetRootAsSC_LOGIN(new ByteBuffer(buffer.Array, buffer.Offset));
        var roomId = packet.RoomId;
        (session as GameSession).RoomId = roomId;
        session.Send(PacketUtil.CS_ENTER_GAME_Packet(PlayerId.Value, roomId));
    }

    public static void SC_ENTER_GAME_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_ENTER_GAME.GetRootAsSC_ENTER_GAME(new ByteBuffer(buffer.Array, buffer.Offset));
        tempPacket = packet;

        var gs = session as GameSession;
        gs.PlayerId = packet.PlayerId;
    }

    public static void SC_UNIT_SPAWN_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_UNIT_SPAWN.GetRootAsSC_UNIT_SPAWN(new ByteBuffer(buffer.Array, buffer.Offset));
        var gs = session as GameSession;
        Console.WriteLine($"RoomId={packet.RoomId}, UnitType={packet.UnitType} UnitRemoved");
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
        var ss = session as GameSession;
        ss.Disconnect();
        session = null;
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

        Debug.Log($"IP={packet.Ip}, Port={packet.Port}");
        var endPoint = new IPEndPoint(IPAddress.Parse(packet.Ip), packet.Port);
        Connector connector = new Connector();
        connector.Connect(endPoint, () => new GameSession());
    }

    public static long? PlayerId;
    public static void MC_PLAYER_REGISTERED_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = MC_PLAYER_REGISTERED.GetRootAsMC_PLAYER_REGISTERED(new ByteBuffer(buffer.Array, buffer.Offset));
        var playerId = packet.PlayerId;
        var ss = session as ServerSession;
        ss.PlayerId = playerId;
        PlayerId = packet.PlayerId;
        Debug.Log($"PlayerRegistered PlayerId={ss.PlayerId}");
    }
}