using System;
using System.Collections.Generic;
using System.Linq;

public class PacketDispatcher
{
    public static PacketDispatcher Instance { get; } = new();

    private static readonly Queue<Tuple<Action<PacketSession, ArraySegment<byte>, PACKET_ID>, PacketSession, ArraySegment<byte>, PACKET_ID>> buffers = new();
    private static object _lock = new();

    public void Enqueue(Tuple<Action<PacketSession, ArraySegment<byte>, PACKET_ID>, PacketSession, ArraySegment<byte>, PACKET_ID> data)
    {
        byte[] copied = data.Item3.AsSpan().ToArray();
        var newData = new Tuple<Action<PacketSession, ArraySegment<byte>, PACKET_ID>, PacketSession, ArraySegment<byte>, PACKET_ID>(data.Item1, data.Item2, copied, data.Item4);

        lock (_lock)
        {
            buffers.Enqueue(newData);
        }
    }

    public List<Tuple<Action<PacketSession, ArraySegment<byte>, PACKET_ID>, PacketSession, ArraySegment<byte>, PACKET_ID>> PopAll()
    {
        lock (_lock)
        {
            var list = new List<Tuple<Action<PacketSession, ArraySegment<byte>, PACKET_ID>, PacketSession, ArraySegment<byte>, PACKET_ID>>();
            while (buffers.Count > 0)
            {
                list.Add(buffers.Dequeue());
            }
            return list;
        }
    }
}
