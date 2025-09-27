using System.Net;
using System.Net.Sockets;
using LCH_COMMON;

namespace LCH_RTS_CORE_LIB.Network;

public class Acceptor
{
    private Socket _listenSocket;
    private Func<Session> _sessionFactory;

    public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int maxListenCnt = 10, int maxRegisterCnt = 10)
    {
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _listenSocket.NoDelay = true;

        _sessionFactory += sessionFactory;
        try
        {
            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(maxListenCnt);

            Logger.Log(ELogType.Console, ELogLevel.Info, $"[Acceptor] Listening on {endPoint}");

            for (var i = 0; i < maxRegisterCnt; i++)
            {
                var args = new SocketAsyncEventArgs();
                args.Completed += OnAcceptCompleted;
                RegisterAccept(args);
            }
        }
        catch (Exception ex)
        {
            Logger.Log(ELogType.Console, ELogLevel.Error, $"[Acceptor] Init Error: {ex}");
        }
    }

    private void RegisterAccept(SocketAsyncEventArgs args)
    {
        args.AcceptSocket = null;

        try
        {
            var pending = _listenSocket.AcceptAsync(args);
            if (!pending)
                OnAcceptCompleted(null, args);
        }
        catch (Exception e)
        {
            Logger.Log(ELogType.Console, ELogLevel.Error, e.ToString());
        }
    }
    
    private void OnAcceptCompleted(object? sender, SocketAsyncEventArgs args)
    {
        try
        {
            if (args is { SocketError: SocketError.Success, AcceptSocket.RemoteEndPoint: not null })
            {
                var session = _sessionFactory();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
                Logger.Log(ELogType.Console, ELogLevel.Error, args.SocketError.ToString());
        }
        catch (Exception e)
        {
            Logger.Log(ELogType.Console, ELogLevel.Error, e.ToString());
        }

        RegisterAccept(args);
    }
}