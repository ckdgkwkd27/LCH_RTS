using System.Net;
using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS.Contents;
using LCH_RTS.Network;

namespace LCH_RTS;

internal abstract class Program
{
    private static void NetworkThread()
    {
        while (true)
        {
            GameServerSessionManager.ForEach((session) => session.FlushSend());
            Thread.Sleep(0);
        }
    }

    private static void GameRoomThread()
    {
        while (true)
        {
            GameRoomManager.Instance.Update();
            Thread.Sleep(0);
        }
    }
    
    public static void Main(string[] args)
    {
        var host = Dns.GetHostName();
        var ipHost = Dns.GetHostEntry(host);
        var ipAddr = ipHost.AddressList[0];

        var endPoint = new IPEndPoint(ipAddr, NetConfig.GetPort(EPortInfo.GAMESERVER_CLIENT_PORT));

        var acceptor = new Acceptor();
        acceptor.Init(endPoint, () => new ClientSession(), 100, 100);
        
        GameRoomManager.Instance.Init(10);

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
            var t = new Thread(GameRoomThread)
            {
                Name = "Game Room"
            };
            t. Start();
        }
        
        Console.WriteLine("GameServer is running...");

        try
        {
            var matchingHost = Dns.GetHostName();
            var matchingIpHost = Dns.GetHostEntry(matchingHost);
            var matchingIpAddr = matchingIpHost.AddressList[0];
            const int matchingPort = 8002;
            var matchingEndPoint = new IPEndPoint(matchingIpAddr, matchingPort);

            var connector = new Connector();
            connector.Connect(matchingEndPoint, () => new MatchingSession(), 1);
            Console.WriteLine($"Connecting to Matching Server at {matchingEndPoint}...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to connect to Matching Server: {ex.Message}");
        }
    }
}