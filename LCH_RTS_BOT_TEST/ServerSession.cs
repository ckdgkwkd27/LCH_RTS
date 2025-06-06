using System.Net;
using Google.FlatBuffers;
using LCH_RTS_CORE_LIB.Network;

namespace LCH_RTS_BOT_TEST;

public class ServerSession : PacketSession
{
    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        
    }
    
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected : {endPoint}");
        Send(PacketUtil.CS_GREET_PACKET("What is this"));
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisconnected : {endPoint}");
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"Transferred bytes: {numOfBytes}");
    }
    
    public override void FlushSend()
    {
        throw new NotImplementedException();
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