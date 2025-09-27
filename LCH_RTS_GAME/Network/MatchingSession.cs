using System;
using System.Net;
using Google.FlatBuffers;
using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS.Network;

namespace LCH_RTS;

public class MatchingSession : PacketSession
{
    public MatchingSession()
    {
        sessionCategory = EPacketSessionCategory.MatchingSesion;
    }
    
    private readonly Lock _lock = new();
    
    //Packet batching
    private int _reservedSendBytes;
    private long _lastSendTick;
    private List<ArraySegment<byte>> _reserveQueue = new();

    public override void OnConnected(EndPoint endPoint)
    {
        GameServerSessionManager.AddSession(this);
        Logger.Log(ELogType.Console, ELogLevel.Info, $"[MatchingSession] Connected to Matching Server: {endPoint}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        GameServerSessionManager.RemoveSession(this);
        Logger.Log(ELogType.Console, ELogLevel.Info, $"[MatchingSession] Disconnected from Matching Server: {endPoint}");
    }

    public override void OnSend(int sendBytes)
    {
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketProcessor.Instance.OnRecvPacket(this, buffer);
    }

    public override void FlushSend()
    {
        List<ArraySegment<byte>> sendList;

        using (_lock.EnterScope())
        {
            var delta = (Environment.TickCount64 - _lastSendTick);
            if (delta < 100 && _reservedSendBytes < 10000)
                return;

            _reservedSendBytes = 0;
            _lastSendTick = Environment.TickCount64;

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
        Array.Copy(BitConverter.GetBytes((ushort)packetSize), 0, sendBuffer, 0, sizeof(ushort));
        Array.Copy(BitConverter.GetBytes((ushort)id), 0, sendBuffer, 2, sizeof(ushort));
        Array.Copy(packet.ByteBuffer.ToArray(0, bodySize), 0, sendBuffer, 4, packetSize);

        using (_lock.EnterScope())
        {
            _reserveQueue.Add(sendBuffer);
            _reservedSendBytes += sendBuffer.Length;
        }
    }
}
