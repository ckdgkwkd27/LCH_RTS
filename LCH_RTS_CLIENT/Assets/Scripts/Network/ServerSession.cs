using UnityEngine;
using System.Net;
using System;

public class ServerSession : PacketSession
{
    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketProcessor.Instance.OnRecvPacket(this, buffer);
    }
    
    public override void OnConnected(EndPoint endPoint)
    {
        Debug.Log($"[OnConnected] {endPoint.ToString()}");
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
    
    //public void Send(PACKET_ID id, IFlatbufferObject packet)
    //{
    //   //#TODO 
    //}
}