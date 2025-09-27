using System.Net;
using System.Net.Sockets;
using LCH_COMMON;

namespace LCH_RTS_CORE_LIB.Network;

public class Connector
{
    private Func<PacketSession> _sessionFactory;
    
    public void Connect(IPEndPoint endPoint, Func<PacketSession> sessionFactory, int count = 1)
    {
        _sessionFactory = sessionFactory;
        for (var i = 0; i < count; i++)
        {
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            var args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;

            RegisterConnect(args);

            Thread.Sleep(10);
        }
    }

    private void RegisterConnect(SocketAsyncEventArgs args)
    {
        if (args.UserToken is not Socket socket)
            return;

        try
        {
            var pending = socket.ConnectAsync(args);
            if (!pending)
                OnConnectCompleted(null, args);
        }
        catch (Exception e)
        {
            Logger.Log(ELogType.Console, ELogLevel.Info, e.ToString());
        }
    }

    private void OnConnectCompleted(object? sender, SocketAsyncEventArgs args)
    {
        try
        {
            if (args.ConnectSocket is null || args.RemoteEndPoint is null)
            {
                throw new Exception();
            }
            
            if (args.SocketError == SocketError.Success)
            {
                var session = _sessionFactory.Invoke();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Logger.Log(ELogType.Console, ELogLevel.Error, $"OnConnectCompleted Fail: {args.SocketError}");
            }
        }
        catch (Exception e)
        {
            Logger.Log(ELogType.Console, ELogLevel.Info, e.ToString());
        }
    }
}