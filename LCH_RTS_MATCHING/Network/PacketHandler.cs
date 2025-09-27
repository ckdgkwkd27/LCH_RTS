using Google.FlatBuffers;
using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS_MATCHING.MatchMake;

namespace LCH_RTS_MATCHING.Network;

public abstract class PacketHandler
{
    public static void CM_MATCH_START_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        CM_MATCH_START.GetRootAsCM_MATCH_START(new ByteBuffer(buffer.Array, buffer.Offset));

        var playerId = (session as ClientSession)!.PlayerId;
        var playerMmr = new Random().Next(500);
        MatchManager.Instance.Enqueue(new MatcherInfo(playerId, playerMmr, session));
        Logger.Log(ELogType.Console, ELogLevel.Info, $"NewMatch Add. Player={playerId}, MMR={playerMmr}");
    }
}