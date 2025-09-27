using System.Net;
using Google.FlatBuffers;
using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;

namespace LCH_RTS_MATCHING.Network;

public class GameSession : PacketSession
{
    public GameSession()
    {
        sessionCategory = EPacketSessionCategory.GameSession;
    }
    
    private readonly Lock _lock = new();
    
    //Packet 모아보내기
    private int _reservedSendBytes;
    private long _lastSendTick;
    private List<ArraySegment<byte>> _reserveQueue = [];
    
    public override void OnConnected(EndPoint endPoint)
    {
        MatchingServerSessionManager.AddSession(this);
        Logger.Log(ELogType.Console, ELogLevel.Info, $"GameSession OnConnected : {endPoint}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        MatchingServerSessionManager.RemoveSession(this);
        Logger.Log(ELogType.Console, ELogLevel.Info,$"GameSession OnDisconnected : {endPoint}");
    }

    public override void OnSend(int sendBytes)
    {
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
    }

    public override void FlushSend()
    {
        List<ArraySegment<byte>> sendList;

        using (_lock.EnterScope())          
        {
            // 0.1초가 지났거나, 너무 패킷이 많이 모일 때 (1만 바이트)
            var delta = (System.Environment.TickCount64 - _lastSendTick);
            if (delta < 100 && _reservedSendBytes < 10000)
                return;

            // 패킷 모아 보내기
            _reservedSendBytes = 0;
            _lastSendTick = System.Environment.TickCount64;

            sendList = _reserveQueue;
            _reserveQueue = [];
        }

        Send(sendList);
    }

    public void Send(PACKET_ID id, IFlatbufferObject packet)
    {
        var bodySize = packet.ByteBuffer.Length;
        var packetSize = bodySize + sizeof(ushort) * 2;
        var sendBuffer = new byte[packetSize];
        Array.Copy(BitConverter.GetBytes((ushort)(packetSize)), 0, sendBuffer, 0, sizeof(ushort));
        Array.Copy(BitConverter.GetBytes((ushort)id), 0, sendBuffer, 2, sizeof(ushort));
        Array.Copy(packet.ByteBuffer.ToArray(0, bodySize), 0, sendBuffer, 4, packetSize);

        using (_lock.EnterScope())  
        {
            _reserveQueue.Add(sendBuffer);
            _reservedSendBytes += sendBuffer.Length;
        }
    }
}
