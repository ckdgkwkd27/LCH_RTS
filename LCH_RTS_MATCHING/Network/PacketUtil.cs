using Google.FlatBuffers;

namespace LCH_RTS_MATCHING;

public static class PacketUtil
{
    public static byte[] MG_GAME_READY_PACKET(long matchId, long playerId1, long playerId2)
    {
        var builder = new FlatBufferBuilder(1024);

        MG_GAME_READY.StartMG_GAME_READY(builder);
        MG_GAME_READY.AddMatchId(builder, matchId);
        MG_GAME_READY.AddPlayerId1(builder, playerId1);
        MG_GAME_READY.AddPlayerId2(builder, playerId2);
        
        var offset = MG_GAME_READY.EndMG_GAME_READY(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();
        
        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.MG_GAME_READY), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] MC_MATCH_JOIN_INFO_PACKET(long playerId, long matchId, string ip, int port)
    {
        var builder = new FlatBufferBuilder(1024);

        var ipOffset = builder.CreateString(ip);
        
        MC_MATCH_JOIN_INFO.StartMC_MATCH_JOIN_INFO(builder);
        MC_MATCH_JOIN_INFO.AddPlayerId(builder, playerId);
        MC_MATCH_JOIN_INFO.AddMatchId(builder, matchId);
        MC_MATCH_JOIN_INFO.AddIp(builder, ipOffset);
        MC_MATCH_JOIN_INFO.AddPort(builder, port);

        var offset = MC_MATCH_JOIN_INFO.EndMC_MATCH_JOIN_INFO(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();

        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.MC_MATCH_JOIN_INFO), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] MC_PLAYER_REGISTERED_PACKET(long playerId)
    {
        var builder = new FlatBufferBuilder(1024);

        MC_PLAYER_REGISTERED.StartMC_PLAYER_REGISTERED(builder);
        MC_PLAYER_REGISTERED.AddPlayerId(builder, playerId);

        var offset = MC_PLAYER_REGISTERED.EndMC_PLAYER_REGISTERED(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();

        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.MC_PLAYER_REGISTERED), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }
}