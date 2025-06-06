using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Connector
{
    private ServerSession _session;
    bool _isConnected = false;
    
    public void Connect(IPEndPoint endPoint, ServerSession session, int count = 1)
    {
        if(_isConnected) 
            return;

        _session = session;
        Managers.Network.SetSession(_session);
        for (var i = 0; i < count; i++)
        {
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;
            args.UserToken = socket;

            RegisterConnect(args);

            //Thread.Sleep(10);
        }

        _isConnected = true;
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
            Debug.Log(e);
        }
    }

    void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
    {
        try
        {
            if (args.SocketError == SocketError.Success)
            {
                _session.Start(args.ConnectSocket);
                _session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Debug.Log($"OnConnectCompleted Fail: {args.SocketError}");
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}