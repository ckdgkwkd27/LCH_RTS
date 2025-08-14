using System.Net;
using Google.FlatBuffers;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS.Contents;
using LCH_RTS.Network;

namespace LCH_RTS;

public class ClientSession : PacketSession
{
    public ClientSession()
    {
        sessionCategory = EPacketSessionCategory.ClientSession;
    }
    
    private readonly Lock _lock = new();
    
    //Packet 모아보내기
    private int _reservedSendBytes;
    private long _lastSendTick;
    private List<ArraySegment<byte>> _reserveQueue = [];
    
    public override void OnConnected(EndPoint endPoint)
    { 
        GameServerSessionManager.AddSession(this);
        Console.WriteLine($"OnConnected : {endPoint}");

        var player = PlayerManager.Instance.AddPlayer(this);
        var room = GameRoomManager.Instance.GetRoom(1);
        if(room is null) return;
        Send(PacketUtil.SC_LOGIN_PACKET(player.PlayerId));
        
        var deck = new PlayerDeck();
        deck.MakeTestDeck();

        var hands = deck.ShuffleAndTake(PlayerDeck.MAX_CARD_LIST);
        var playerSide = room.AddPlayer(player, deck, hands);

        Send(PacketUtil.SC_ENTER_GAME_PACKET(1, room.GetPlayerId(playerSide), (byte)playerSide, 0, CardUtil.ConvertToCardInfos(hands).ToArray()));
        
        room.GameReady();
    }
    
    public override void OnDisconnected(EndPoint endPoint)
    {
        GameServerSessionManager.RemoveSession(this);
        Console.WriteLine($"OnDisconnected : {endPoint}");
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
