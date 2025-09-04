
using System.Net;
using LCH_COMMON;
using LCH_RTS_BOT_TEST;
using LCH_RTS_CORE_LIB.Network;

static class Global
{
    public const int BotTestCnt = 2;
    public static int ConnectedCnt = 0;
    public static long LastSentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public static bool IsAllConnected()
    {
        return BotTestCnt == ConnectedCnt;
    }
}

internal class Program
{
    public static void Main(string[] args)
    {
        var host = Dns.GetHostName();
        var ipHost = Dns.GetHostEntry(host);
        var ipAddr = ipHost.AddressList[0];
        const int port = (int)EPortInfo.MATCHING_CLIENT_PORT;
        var matchingEndPoint = new IPEndPoint(ipAddr, port);

        var connector = new Connector();
        connector.Connect(matchingEndPoint, () => BotSessionManager.Instance.GenerateMatching(), Global.BotTestCnt);
        Console.WriteLine($"Connecting to Matching Server at {matchingEndPoint}");

        /////////////////////////////////////////////////////////////////////
        while (true)
        {
            if (Global.IsAllConnected())
            {
                var nowSec = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (nowSec - Global.LastSentTime <= 1)
                {
                    BotSessionManager.Instance.ForEachSend();
                    Global.LastSentTime = nowSec;
                }
            }
        }
    }
}
