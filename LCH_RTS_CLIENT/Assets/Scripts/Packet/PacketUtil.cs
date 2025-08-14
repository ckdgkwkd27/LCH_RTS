using Google.FlatBuffers;
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

    public static byte[] CM_MATCH_START_Packet(long playerId, int playerMmr)
    {
        var builder = new FlatBufferBuilder(1024);

        CM_MATCH_START.StartCM_MATCH_START(builder);
        CM_MATCH_START.AddPlayerId(builder, playerId);
        CM_MATCH_START.AddPlayerMmr(builder, playerMmr);
        var offset = CM_MATCH_START.EndCM_MATCH_START(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();

        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.CM_MATCH_START), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }
}