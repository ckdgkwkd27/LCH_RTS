using UnityEngine;
using System.Net;
using System;

public class ServerSession : PacketSession
{
    public long PlayerId { get; set; }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketProcessor.Instance.OnRecvPacket(this, buffer);
    }
    
    public override void OnConnected(EndPoint endPoint)
    {
        Debug.Log($"[Matching OnConnected] {endPoint.ToString()}");
        Managers.Network.SendToMatch(PacketUtil.CM_MATCH_START_Packet());
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Debug.Log($"[OnDisconnected] {endPoint.ToString()}");
    }

    public override void OnSend(int numOfBytes)
    {
    }
    
    public override void FlushSend()
    {
    }
}