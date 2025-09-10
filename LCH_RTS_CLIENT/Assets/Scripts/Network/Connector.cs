using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Connector
{
    public void Connect(IPEndPoint endPoint, Func<PacketSession> sessionFactory)
    {
        try
        {
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                
            socket.Connect(endPoint);
                
            var session = sessionFactory.Invoke();
            Managers.Network.SetSession(session);
            session.Start(socket);
            session.OnConnected(endPoint);
                
            Debug.Log($"Connected to {endPoint}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection failed: {e.Message}");
        }
    }
}