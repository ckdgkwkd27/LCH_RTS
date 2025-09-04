using System.Net;
using LCH_RTS_CORE_LIB.Network;

namespace LCH_RTS_BOT_TEST;

public class GameSession : PacketSession
{
    public long PlayerId { get; set; }
    public long RoomId { get; set; }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketProcessor.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"[GameServer OnConnected] {endPoint.ToString()}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"[OnDisconnected] {endPoint.ToString()}");
        BotSessionManager.Instance.RemoveGameSession(this);
    }

    public override void OnSend(int numOfBytes)
    {
    }

    public override void FlushSend()
    {
    }
}