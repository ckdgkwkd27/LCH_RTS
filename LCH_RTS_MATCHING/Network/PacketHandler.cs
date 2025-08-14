using Google.FlatBuffers;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS_MATCHING.MatchMake;

namespace LCH_RTS_MATCHING.Network;

public abstract class PacketHandler
{
    public static void CM_MATCH_START_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = CM_MATCH_START.GetRootAsCM_MATCH_START(new ByteBuffer(buffer.Array, buffer.Offset));
        MatchManager.Instance.Enqueue(new MatcherInfo(packet.PlayerId, packet.PlayerMmr, session));
        Console.WriteLine($"NewMatch Add. Player={packet.PlayerId}, MMR={packet.PlayerMmr}");
    }
}