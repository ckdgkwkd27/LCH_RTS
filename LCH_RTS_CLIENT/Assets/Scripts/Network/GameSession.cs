using UnityEngine;
using System.Net;
using System;

public class GameSession : PacketSession
{
    public PlayerController PlayerController;

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketProcessor.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnConnected(EndPoint endPoint)
    {
        Send(PacketUtil.CS_LOGIN_Packet(Util.PlayerId, Util.MatchId));
        Debug.Log($"[GameServer OnConnected] {endPoint.ToString()}");
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