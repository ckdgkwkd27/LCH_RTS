
using System.Net;
using LCH_RTS_BOT_TEST;
using LCH_RTS_CORE_LIB.Network;

internal class Program
{
    public static void Main(string[] args)
    {
        const int botCnt = 1;
        Thread.Sleep(3000);
        var host = Dns.GetHostName();
        var ipHost = Dns.GetHostEntry(host);
        var ipAddr = ipHost.AddressList[0];
        const int port = 8001;
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
