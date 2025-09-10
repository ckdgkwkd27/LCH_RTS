using System.Net;
using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;

namespace LCH_RTS_BOT_TEST;

internal static class Global
{
    public const int BotTestCnt = 7000;
    public static int GameStartedCnt = 0;
    public static long LastSentMsec = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public static bool IsAllGameStarted()
    {
        if (BotTestCnt != GameStartedCnt)
        {
            Console.WriteLine($"bot={BotTestCnt},started={GameStartedCnt}");
        }
        return BotTestCnt == GameStartedCnt;
    }
}

internal class Program
{
    public static void Main(string[] args)
    {
        Thread.Sleep(3000);
        
        var host = Dns.GetHostName();
        var ipHost = Dns.GetHostEntry(host);
        var ipAddr = ipHost.AddressList[0];
        const int port = (int)EPortInfo.MATCHING_CLIENT_PORT;
        var matchingEndPoint = new IPEndPoint(ipAddr, port);

        var connector = new Connector();
        connector.Connect(matchingEndPoint, () => BotSessionManager.Instance.GenerateMatching(), Global.BotTestCnt);
        /////////////////////////////////////////////////////////////////////
         
        while (true)
        {
            if (!Global.IsAllGameStarted()) 
                continue;
            
            var nowMSec = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (nowMSec - Global.LastSentMsec < 1000) 
                continue;
            
            BotSessionManager.Instance.ForEachSend();
            Global.LastSentMsec = nowMSec;
        }
    }
}