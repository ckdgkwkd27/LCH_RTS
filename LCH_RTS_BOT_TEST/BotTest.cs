
using System.Net;
using LCH_RTS_BOT_TEST;
using LCH_RTS_CORE_LIB.Network;

static class Global
{
    public static const int BotTestCnt = 1;
    public static int ConnectedCnt = 0;
    public static var lastSentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public bool IsAllConnected()
    {
        return BotTestCnt == ConnectedCnt;
    }
}

internal class Program
{

    private static void TestThread()
    {
        while (Global.IsAllConnected())
        {
            PacketUtil.CS_UNIT_SPAWN_Packet() ~~~~~~~~~~~
            Thread.Sleep(0);
        }
    }

    public static void Main(string[] args)
    {
        const int botCnt = BotTestCnt;
        Thread.Sleep(3000);
        var host = Dns.GetHostName();
        var ipHost = Dns.GetHostEntry(host);
        var ipAddr = ipHost.AddressList[0];
        const int port = 8002;
        var matchingEndPoint = new IPEndPoint(ipAddr, port);

        var connector = new Connector();
        connector.Connect(matchingEndPoint, () => new ServerSession(), 1);
        Console.WriteLine($"Connecting to Matching Server at {matchingEndPoint}...");

        Console.WriteLine("Bot is running...");

        /////////////////////////////////////////////////////////////////////
        while (true)
        {
            Thread.Sleep(10000);
        }
    }
}
