using Google.FlatBuffers;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS.Network;

namespace LCH_RTS;

public class PacketProcessor
{
    public static PacketProcessor Instance { get; } = new();

    private PacketProcessor()
    {
        Register();
    }

    private readonly Dictionary<PACKET_ID, Action<PacketSession, ArraySegment<byte>, PACKET_ID>> _deserializer = new();
    private readonly Dictionary<PACKET_ID, Action<PacketSession, ArraySegment<byte>>> _handler = new();

    private void Register()
    {
        {
            _handler.Add(PACKET_ID.CS_GREET, PacketHandler.CS_GREET_Handler);
            _handler.Add(PACKET_ID.CS_LOGIN, PacketHandler.CS_LOGIN_Handler);
            _handler.Add(PACKET_ID.CS_ENTER_GAME, PacketHandler.CS_ENTER_GAME_Handler);
            _handler.Add(PACKET_ID.CS_UNIT_SPAWN, PacketHandler.CS_UNIT_SPAWN_Handler);

            _handler.Add(PACKET_ID.MG_GAME_READY, PacketHandler.MG_GAME_READY_Handler);
        }

        {
            _deserializer.Add(PACKET_ID.CS_GREET, MakePacket<CS_GREET>);
            _deserializer.Add(PACKET_ID.CS_LOGIN, MakePacket<CS_LOGIN>);
            _deserializer.Add(PACKET_ID.CS_ENTER_GAME, MakePacket<CS_ENTER_GAME>);
            _deserializer.Add(PACKET_ID.CS_UNIT_SPAWN, MakePacket<CS_UNIT_SPAWN>);

            _deserializer.Add(PACKET_ID.MG_GAME_READY, MakePacket<MG_GAME_READY>);
        }
    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        if(buffer.Array is null) return;
        
        ushort count = 0;
        var size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        if (buffer.Array.Length < size)
        {
            Console.WriteLine("[ERROR] Packet size is too small.");
            return;
        }
        
        var id = (PACKET_ID)BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;
        
        var bodyBuffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + count, buffer.Count - count);
        if(_deserializer.TryGetValue(id, out var handler))
            handler.Invoke(session, bodyBuffer, id);
    }

    private void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, PACKET_ID id) where T : IFlatbufferObject, new()
    {
        Action<PacketSession, ArraySegment<byte>>? action = null;
        if (_handler.TryGetValue(id, out action))
            action.Invoke(session, buffer);
    }
}