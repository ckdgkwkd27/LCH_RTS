using System.Net;
using System.Net.Sockets;
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
        
        // VPN 주소를 제외하고 실제 로컬 네트워크 IPv4 주소 찾기
        var ipAddr = ipHost.AddressList
            .Where(addr => addr.AddressFamily == AddressFamily.InterNetwork)
            .Where(addr => !IPAddress.IsLoopback(addr))
            .FirstOrDefault(addr => !IsPrivate(addr));

        if (ipAddr == null)
        {
            Console.WriteLine("Failed to find local IPv4 address. Using loopback.");
            ipAddr = IPAddress.Loopback;
        }

        Console.WriteLine($"GameServer binding to: {ipAddr}");
        
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
            t.Start();
        }
        
        Console.WriteLine("GameServer is running...");

        try
        {
            var matchingHost = Dns.GetHostName();
            var matchingIpHost = Dns.GetHostEntry(matchingHost);
            
            // 매칭 서버도 동일한 로직 적용
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