using Google.FlatBuffers;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS.Contents;
using LCH_RTS.Contents.Units;

namespace LCH_RTS;

public abstract class PacketHandler
{
    public static void CS_GREET_Handler(ClientSession session, ArraySegment<byte> buffer)
    {
        var packet = CS_GREET.GetRootAsCS_GREET(new ByteBuffer(buffer.Array, buffer.Offset));
        Console.WriteLine($"CS_GREET : {packet.Data}");
    }

    public static void CS_UNIT_SPAWN_Handler(ClientSession session, ArraySegment<byte> buffer)
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
        
        var unitId = room.IssueUnitId();
        var player = PlayerManager.Instance.GetPlayer(session);
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
        
        room.AddUnit(unitId, playerSide, packet.CreatePos.Value, unitStat, packet.UnitType); 
        session.Send(PacketUtil.SC_UNIT_SPAWN_PACKET(unitId, playerSide, packet.UnitType, packet.CreatePos.Value, unitStat));
    }
}