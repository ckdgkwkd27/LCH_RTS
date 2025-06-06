using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Acceptor
{
    private Socket _listenSocket;
    private Session _session;

    public void Init(IPEndPoint endPoint, int maxListenCnt, int maxRegisterCnt, Session session)
    {
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _listenSocket.NoDelay = true;
        
        _session = session;
        _session.SetSocket(_listenSocket);
        
        try
        {
            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(maxListenCnt);

            Debug.Log($"[Acceptor] Listening on {endPoint}");

            for (var i = 0; i < maxRegisterCnt; i++)
            {
                var args = new SocketAsyncEventArgs();
                args.Completed += OnAcceptCompleted!;
                RegisterAccept(args);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"[Acceptor] Init Error: {ex}");
        }
    }

    public void RegisterAccept(SocketAsyncEventArgs args)
    {
        args.AcceptSocket = null;

        try
        {
            var pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
                OnAcceptCompleted(null, args);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
    
    private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
    {
        try
        {
            if (args.SocketError == SocketError.Success)
            {
                var session = SessionManager.AcquireFromPool();
                if (session is null)
                {
                    Debug.Log("[Acceptor] Session is null");
                    return;
                }
                
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
                Debug.Log(args.SocketError.ToString());
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        RegisterAccept(args);
    }
}