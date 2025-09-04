using Google.FlatBuffers;
using JetBrains.Annotations;
using System;
using UnityEngine;

public class PacketUtil
{
    public static byte[] CS_GREET_Packet(string data)
    {
        var builder = new FlatBufferBuilder(1024);
        var dataOffset = builder.CreateString(data);
        CS_GREET.StartCS_GREET(builder);
        CS_GREET.AddData(builder, dataOffset);
        var offset = CS_GREET.EndCS_GREET(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();

        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.CS_GREET), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] CS_LOGIN_Packet(long playerId, long matchId)
    {
        var builder = new FlatBufferBuilder(1024);

        CS_LOGIN.StartCS_LOGIN(builder);
        CS_LOGIN.AddPlayerId(builder, playerId);
        CS_LOGIN.AddMatchId(builder, matchId);
        var offset = CS_LOGIN.EndCS_LOGIN(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();

        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.CS_LOGIN), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] CS_UNIT_SPAWN_Packet(long roomId, int unitType, Vector2 pos)
    {
        var builder = new FlatBufferBuilder(1024);

        var vecOffset = Vec2.CreateVec2(builder, pos.x, pos.y);

        CS_UNIT_SPAWN.StartCS_UNIT_SPAWN(builder);
        CS_UNIT_SPAWN.AddRoomId(builder, roomId);
        CS_UNIT_SPAWN.AddUnitType(builder, unitType);
        CS_UNIT_SPAWN.AddCreatePos(builder, vecOffset);
        var offset = CS_UNIT_SPAWN.EndCS_UNIT_SPAWN(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();

        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.CS_UNIT_SPAWN), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] CM_MATCH_START_Packet()
    {
        var builder = new FlatBufferBuilder(1024);

        CM_MATCH_START.StartCM_MATCH_START(builder);
        var offset = CM_MATCH_START.EndCM_MATCH_START(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();

        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.CM_MATCH_START), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] CS_ENTER_GAME_Packet(long playerId, long roomId)
    {
        var builder = new FlatBufferBuilder(1024);

        CS_ENTER_GAME.StartCS_ENTER_GAME(builder);
        CS_ENTER_GAME.AddPlayerId(builder, playerId);
        CS_ENTER_GAME.AddRoomId(builder, roomId);
        var offset = CS_ENTER_GAME.EndCS_ENTER_GAME(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();

        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.CS_ENTER_GAME), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }
}