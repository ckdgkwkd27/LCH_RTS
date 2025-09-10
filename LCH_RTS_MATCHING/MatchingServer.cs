using System.Net;
using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS_MATCHING;
using LCH_RTS_MATCHING.MatchMake;
using LCH_RTS_MATCHING.Network;

internal class Program
{
    private static void NetworkThread()
    {
        while (true)
        {
            MatchingServerSessionManager.ForEach((session) => session.FlushSend());
            Thread.Sleep(0);
        }
    }

    private static void MatchThread()
    {
        while (true)
        {
            MatchManager.Instance.ProcessMatching();
            Thread.Sleep(0);
        }
    }
    
    public static void Main(string[] args)
    {
        var host = Dns.GetHostName();
        var ipHost = Dns.GetHostEntry(host);
        var ipAddr = ipHost.AddressList[0];

        //Client Accept
        {
            var endPoint = new IPEndPoint(ipAddr, NetConfig.GetPort(EPortInfo.MATCHING_CLIENT_PORT));

            var acceptor = new Acceptor();
            acceptor.Init(endPoint, () => new ClientSession(), 1000, 1000);
        }
        
        //GameServer Accept
        {
            var endPoint = new IPEndPoint(ipAddr, NetConfig.GetPort(EPortInfo.MATCHING_GAMESERVER_PORT));

            var acceptor = new Acceptor();
            acceptor.Init(endPoint, () => new GameSession());
        }
        
        //Networking
        {
            var t = new Thread(NetworkThread)
            {
                Name = "Network Send"
            };
            t.Start();
        }
        
        //GameRoom
        {
            var t = new Thread(MatchThread)
            {
                Name = "Match Thread"
            };
            t.Start();
        }
        
        Console.WriteLine("MatchingServer is running...");
    }
}