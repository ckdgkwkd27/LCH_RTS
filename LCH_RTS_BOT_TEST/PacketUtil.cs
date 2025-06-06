using Google.FlatBuffers;

namespace LCH_RTS_BOT_TEST;

public static class PacketUtil
{
    public static byte[] CS_GREET_PACKET(string data)
    {
        var builder = new FlatBufferBuilder(1024);
        var dataOffset = builder.CreateString(data);
        CS_GREET.StartCS_GREET(builder);
        CS_GREET.AddData(builder, dataOffset);
        var offset = CS_GREET.EndCS_GREET(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();
        
        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.CS_GREET), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }
}