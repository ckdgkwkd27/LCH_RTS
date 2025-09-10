using System.Net;
using System.Collections.Generic;
using LCH_RTS_CORE_LIB.Network;

namespace LCH_RTS_BOT_TEST;

public class GameSession : PacketSession
{
    public long PlayerId { get; set; }
    public long RoomId { get; set; }
    public long MatchId { get; set; }
    public byte PlayerSide { get; set; }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketProcessor.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnConnected(EndPoint endPoint)
    {
        Send(PacketUtil.CS_LOGIN_Packet(PlayerId, MatchId));
        Console.WriteLine($"OnConnected GameServer: {endPoint}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisConnected GameServer: {endPoint}");
        BotSessionManager.Instance.RemoveGameSession(this);
    }

    public override void OnSend(int numOfBytes)
    {
    }

    public override void FlushSend()
    {
    }
}