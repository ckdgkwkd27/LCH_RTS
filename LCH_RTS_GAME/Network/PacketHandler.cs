using Google.FlatBuffers;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS.Contents;
using LCH_RTS.Contents.Units;

namespace LCH_RTS.Network;

public abstract class PacketHandler
{
    public static void CS_GREET_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = CS_GREET.GetRootAsCS_GREET(new ByteBuffer(buffer.Array, buffer.Offset));
        Console.WriteLine($"CS_GREET : {packet.Data}");
    }

    public static void CS_LOGIN_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = CS_LOGIN.GetRootAsCS_LOGIN(new ByteBuffer(buffer.Array, buffer.Offset));
        session.Send(PacketUtil.SC_LOGIN_PACKET(packet.MatchId));
        Console.WriteLine($"LOGIN: {packet.PlayerId}");
    }
    
    public static void CS_ENTER_GAME_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = CS_ENTER_GAME.GetRootAsCS_ENTER_GAME(new ByteBuffer(buffer.Array, buffer.Offset));
        
        var player = PlayerManager.Instance.AddPlayer((session as ClientSession)!, packet.PlayerId);
        var room = GameRoomManager.Instance.GetRoom(packet.RoomId);
        if(room is null) return;
        
        var deck = new PlayerDeck();
        deck.MakeTestDeck();
        var hands = deck.ShuffleAndTake(PlayerDeck.MAX_CARD_LIST);
        
        room.Push(room.AddPlayer, player, deck, hands);
        room.Push(room.GameReady);
    }

    public static void CS_UNIT_SPAWN_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = CS_UNIT_SPAWN.GetRootAsCS_UNIT_SPAWN(new ByteBuffer(buffer.Array, buffer.Offset));
        var room = GameRoomManager.Instance.GetRoom(packet.RoomId);
        if (room == null)
        {
            Console.WriteLine($"[ERROR] Not Exist RoomId: {packet.RoomId}");
            session.Disconnect();
            return;
        }

        if (packet.CreatePos == null)
        {
            Console.WriteLine("[ERROR] Pos is NULL");
            session.Disconnect();
            return;
        }

        if (!UnitUtil.IsValidSpawnSpace(packet.CreatePos.Value))
        {
            Console.WriteLine($"[Warning] Pos is Not Valid=({packet.CreatePos.Value.X},{packet.CreatePos.Value.Y})");
            return;
        }

        if (room.GetRoomState() != ERoomState.Start)
        {
            Console.WriteLine($"[ERROR] Not Able RoomState");
            session.Disconnect();
            return;
        }
        
        var unitId = room.IssueUnitId();
        var player = PlayerManager.Instance.GetPlayer((session as ClientSession)!);
        if (player is null)
        {
            Console.WriteLine($"[ERROR] Not Exist Player");
            session.Disconnect();
            return;
        }

        var unitStat = UnitUtil.GetUnitStatConfig(packet.UnitType);
        var playerSide = room.GetPlayerSide(player);
        if (playerSide == EPlayerSide.Max)
        {
            Console.WriteLine($"[ERROR] PlayerSide is Max");
            session.Disconnect();
            return;
        }
        
        var playerCost = room.GetPlayerCost(playerSide);
        if (playerCost < unitStat.Cost) 
        {
            Console.WriteLine($"[WARNING] Player {player.PlayerId} insufficient cost. Required: {unitStat.Cost}, Current: {playerCost}");
            return;
        }
        
        if (!room.HasCardInHand(playerSide, packet.UnitType))
        {
            Console.WriteLine($"[WARNING] Player {player.PlayerId} does not have card with UnitType {packet.UnitType}");
            return;
        }

        var createPos = packet.CreatePos.Value;
        var unitType = packet.UnitType;
        var playerIdLong = player.PlayerId;
        var playerSideLocal = playerSide;
        var unitIdLocal = unitId;
        var unitStatLocal = unitStat;

        room.Push(() =>
        {
            room.AddUnit(unitIdLocal, playerSideLocal, createPos, unitStatLocal, unitType); 
            room.Broadcast(PacketUtil.SC_UNIT_SPAWN_PACKET(unitIdLocal, playerSideLocal, unitType, createPos, unitStatLocal));
            var remainCostLocal = room.DecreaseCost(playerSideLocal, unitStatLocal.Cost);
            session.Send(PacketUtil.SC_PLAYER_COST_UPDATE_PACKET(room.RoomId, remainCostLocal));
            room.HandsUpdate(playerSideLocal, playerIdLong, unitType);
        });
    }

    public static void MG_GAME_READY_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = MG_GAME_READY.GetRootAsMG_GAME_READY(new ByteBuffer(buffer.Array, buffer.Offset));
        var matchId = packet.MatchId;
        var room = GameRoomManager.Instance.GetRoom(matchId);
        if (room is null)
        {
            room = new GameRoom(matchId);
            GameRoomManager.Instance.Add(room);
        }
        Console.WriteLine($"[MG_GAME_READY] GameRoom {matchId} created for match {matchId} with players: {packet.PlayerId1}, {packet.PlayerId2}");
    }
}