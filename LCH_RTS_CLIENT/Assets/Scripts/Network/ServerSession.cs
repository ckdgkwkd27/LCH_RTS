using UnityEngine;
using System.Net;
using System;

public class ServerSession : PacketSession
{
    public PlayerController PlayerController;

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketProcessor.Instance.OnRecvPacket(this, buffer);
    }
    
    public override void OnConnected(EndPoint endPoint)
    {
        Debug.Log($"[OnConnected] {endPoint.ToString()}");

        long randPlayerId = new System.Random().Next(100);
        int randMmr = new System.Random().Next(500);
        Managers.Network.Send(PacketUtil.CM_MATCH_START_Packet(randPlayerId, randMmr));
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