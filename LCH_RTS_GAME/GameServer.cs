using System.Net;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS;
using LCH_RTS.Contents;

internal class Program
{
    private static void NetworkThread()
    {
        while (true)
        {
            SessionManager.ForEach((session) => session.FlushSend());
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

        const int port = 8888;  
        var endPoint = new IPEndPoint(ipAddr, port);

        SessionManager.PrepareSessions(100, () => new ClientSession());
        var clientSession = SessionManager.AcquireFromPool() as ClientSession ?? throw new Exception();

        var acceptor = new Acceptor();
        acceptor.Init(endPoint, 100, 100, clientSession);
        
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
        
        Console.WriteLine("Server is running...");
    }
}