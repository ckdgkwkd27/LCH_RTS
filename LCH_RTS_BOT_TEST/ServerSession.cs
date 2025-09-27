using System.Net;
using Google.FlatBuffers;
using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;

namespace LCH_RTS_BOT_TEST;

public class ServerSession : PacketSession
{
    private GameSession _gameSession;
    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketProcessor.Instance.OnRecvPacket(this, buffer);
    }
    
    public override void OnConnected(EndPoint endPoint)
    {
        Send(PacketUtil.CM_MATCH_START_Packet());
        Logger.Log(ELogType.Console, ELogLevel.Info, $"OnConnected MatchingServer: {endPoint}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Logger.Log(ELogType.Console, ELogLevel.Info, $"OnDisconnected MatchingServer: {endPoint}");
        BotSessionManager.Instance.RemoveMatchingSession(this);
    }

    public override void OnSend(int numOfBytes)
    {
        Logger.Log(ELogType.Console, ELogLevel.Info, $"Transferred bytes: {numOfBytes}");
    }
    
    public override void FlushSend()
    {
    }
    
    public void Send(PACKET_ID id, IFlatbufferObject packet)
    {
        var bodySize = packet.ByteBuffer.Length;
        var packetSize = bodySize + sizeof(ushort) * 2;
        var sendBuffer = new byte[packetSize];
        Array.Copy(BitConverter.GetBytes((ushort)(packetSize)), 0, sendBuffer, 0, sizeof(ushort));
        Array.Copy(BitConverter.GetBytes((ushort)id), 0, sendBuffer, 2, sizeof(ushort));
        Array.Copy(packet.ByteBuffer.ToArray(0, bodySize), 0, sendBuffer, 4, packetSize);
        Send(sendBuffer);
    }
}