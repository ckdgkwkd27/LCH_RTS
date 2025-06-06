using System.Net;
using System.Net.Sockets;

namespace LCH_RTS_CORE_LIB.Network;

public class Connector
{
    private Session _session;
    
    public void Connect(IPEndPoint endPoint, Session session, int count = 1)
    {
        _session = session;
        for (var i = 0; i < count; i++)
        {
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;

            RegisterConnect(args);

            Thread.Sleep(10);
        }
    }

    void RegisterConnect(SocketAsyncEventArgs args)
    {
        Socket socket = args.UserToken as Socket;
        if (socket == null)
            return;

        try
        {
            bool pending = socket.ConnectAsync(args);
            if (pending == false)
                OnConnectCompleted(null, args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void OnConnectCompleted(object? sender, SocketAsyncEventArgs args)
    {
        try
        {
            if (args.SocketError == SocketError.Success)
            {
                var session = SessionManager.AcquireFromPool();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}