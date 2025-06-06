
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

        var sessionBuilder = () => new ServerSession();
        SessionManager.PrepareSessions(100, sessionBuilder);
        if (SessionManager.AcquireFromPool() is not ServerSession serverSession)
        {
            Console.WriteLine("ServerSession is null");
            return;
        }

        const int port = 8888;
        var endPoint = new IPEndPoint(ipAddr, port);

        var connector = new Connector();
        connector.Connect(endPoint, serverSession, botCnt);
        Console.WriteLine("Bot is running...");

        /////////////////////////////////////////////////////////////////////
        while (true)
        {
            Thread.Sleep(10000);
        }
    }
}
