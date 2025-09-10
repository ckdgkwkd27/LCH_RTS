using System.Net;
using System.Net.Sockets;
using LCH_COMMON;
using LCH_RTS_CORE_LIB.Network;
using LCH_RTS.Contents;
using LCH_RTS.Network;

namespace LCH_RTS;

internal abstract class Program
{
    private const int MaxRoomCount = 1001;
    private static readonly int MaxThreadCount = Environment.ProcessorCount / 2;

    private static void NetworkThread()
    {
        while (true)
        {
            GameServerSessionManager.ForEach(session => session.FlushSend());
            Thread.Sleep(0);
        }
    }

    private static void GameRoomThread(int threadIdx)
    {
        while (true)
        {
            var i = threadIdx;
            if(i >= MaxRoomCount) return;
            for(; i < MaxRoomCount; i += MaxThreadCount)
            {
                GameRoomManager.Instance.Update(i);
                Thread.Sleep(0);
            }
        }
    }
    
    public static void Main(string[] args)
    {
        var host = Dns.GetHostName();
        var ipHost = Dns.GetHostEntry(host);
        
        var ipAddr = ipHost.AddressList
            .Where(addr => addr.AddressFamily == AddressFamily.InterNetwork)
            .Where(addr => !IPAddress.IsLoopback(addr))
            .FirstOrDefault(addr => !IsPrivate(addr)) ?? IPAddress.Loopback;
        Console.WriteLine($"GameServer binding to: {ipAddr}");
        
        var endPoint = new IPEndPoint(ipAddr, NetConfig.GetPort(EPortInfo.GAMESERVER_CLIENT_PORT));

        var acceptor = new Acceptor();
        acceptor.Init(endPoint, () => new ClientSession(), 100, 100);
        
        GameRoomManager.Instance.Init(MaxRoomCount);

        //Networking
        {
            var t = new Thread(NetworkThread)
            {
                Name = "Network Thread"
            };
            t.Start();
        }
        
        //GameRoom
        for(var i = 1; i <= MaxThreadCount; i++)
        {
            var threadIndex = i; 
            var t = new Thread(() => GameRoomThread(threadIndex))
            {
                Name = $"GameRoom Thread{threadIndex}"
            };
            t.Start();
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
        
        while (true)
        {
            Thread.Sleep(1000);
        }
    }

    private static bool IsPrivate(IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        return bytes[0] switch
        {
            10 => true,
            172 => bytes[1] >= 16 && bytes[1] <= 31,
            192 => bytes[1] == 168,
            _ => false
        };
    }
}