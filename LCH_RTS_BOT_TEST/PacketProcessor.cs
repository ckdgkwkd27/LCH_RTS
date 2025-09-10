using Google.FlatBuffers;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using LCH_RTS_CORE_LIB.Network;

namespace LCH_RTS_BOT_TEST;
public class PacketProcessor
{
    public static PacketProcessor Instance { get; } = new();

    private PacketProcessor()
    {
        Register();
    }

    private Dictionary<PACKET_ID, Action<PacketSession, ArraySegment<byte>, PACKET_ID>> _deserializer = new();
    private Dictionary<PACKET_ID, Action<PacketSession, ArraySegment<byte>>> _handler = new();

    private void Register()
    {
        {
            _handler.Add(PACKET_ID.SC_LOGIN, PacketHandler.SC_LOGIN_Handler);
            _handler.Add(PACKET_ID.SC_ENTER_GAME, PacketHandler.SC_ENTER_GAME_Handler);
            _handler.Add(PACKET_ID.SC_START_GAME, PacketHandler.SC_START_GAME_Handler);
            _handler.Add(PACKET_ID.SC_UNIT_SPAWN, PacketHandler.SC_UNIT_SPAWN_Handler);
            _handler.Add(PACKET_ID.SC_UNIT_MOVE, PacketHandler.SC_UNIT_MOVE_Handler);
            _handler.Add(PACKET_ID.SC_UNIT_ATTACK, PacketHandler.SC_UNIT_ATTACK_Handler);
            _handler.Add(PACKET_ID.SC_REMOVE_UNIT, PacketHandler.SC_REMOVE_UNIT_Handler);
            _handler.Add(PACKET_ID.SC_END_GAME, PacketHandler.SC_END_GAME_Handler);
            _handler.Add(PACKET_ID.SC_PLAYER_COST_UPDATE, PacketHandler.SC_PLAYER_COST_UPDATE_Handler);
            _handler.Add(PACKET_ID.SC_PLAYER_HAND_UPDATE, PacketHandler.SC_PLAYER_HAND_UPDATE_Handler);

            _handler.Add(PACKET_ID.MC_MATCH_JOIN_INFO, PacketHandler.MC_MATCH_JOIN_INFO_Handler);
            _handler.Add(PACKET_ID.MC_PLAYER_REGISTERED, PacketHandler.MC_PLAYER_REGISTERED_Handler);
        }

        {
            _deserializer.Add(PACKET_ID.SC_LOGIN, MakePacket<SC_LOGIN>);
            _deserializer.Add(PACKET_ID.SC_ENTER_GAME, MakePacket<SC_ENTER_GAME>);
            _deserializer.Add(PACKET_ID.SC_START_GAME, MakePacket<SC_START_GAME>);
            _deserializer.Add(PACKET_ID.SC_UNIT_SPAWN, MakePacket<SC_UNIT_SPAWN>);
            _deserializer.Add(PACKET_ID.SC_UNIT_MOVE, MakePacket<SC_UNIT_MOVE>);
            _deserializer.Add(PACKET_ID.SC_UNIT_ATTACK, MakePacket<SC_UNIT_ATTACK>);
            _deserializer.Add(PACKET_ID.SC_REMOVE_UNIT, MakePacket<SC_REMOVE_UNIT>);
            _deserializer.Add(PACKET_ID.SC_END_GAME, MakePacket<SC_END_GAME>);
            _deserializer.Add(PACKET_ID.SC_PLAYER_COST_UPDATE, MakePacket<SC_PLAYER_COST_UPDATE>);
            _deserializer.Add(PACKET_ID.SC_PLAYER_HAND_UPDATE, MakePacket<SC_PLAYER_HAND_UPDATE>);

            _deserializer.Add(PACKET_ID.MC_MATCH_JOIN_INFO, MakePacket<MC_MATCH_JOIN_INFO>);
            _deserializer.Add(PACKET_ID.MC_PLAYER_REGISTERED, MakePacket<MC_PLAYER_REGISTERED>);
        }
    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        if (buffer.Array is null) return;

        ushort count = 0;
        var size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        if (buffer.Array.Length < size)
        {
            Console.WriteLine("[ERROR] Packet size is too small.");
            return;
        }

        var id = (PACKET_ID)BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        var bodyBuffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + count, buffer.Count - count);
        if (_deserializer.TryGetValue(id, out var handler))
            handler.Invoke(session, bodyBuffer, id);
    }

    private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, PACKET_ID id) where T : IFlatbufferObject, new()
    {
        Action<PacketSession, ArraySegment<byte>> action;
        if (_handler.TryGetValue(id, out action))
            action.Invoke(session, buffer);
    }
}