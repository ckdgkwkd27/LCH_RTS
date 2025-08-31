using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public abstract class PacketSession : Session
{
    public override int OnRecv(ArraySegment<byte> buffer)
    {
        const int headerSize = 2;
        var processLen = 0;

        while (true)
        {
            if (buffer.Count < headerSize || buffer == null || buffer.Array == null)
                break;

            var dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            if (buffer.Count < dataSize)
                break;

            OnRecvPacket(buffer);

            processLen += dataSize;
            buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
        }

        return processLen;
    }

    public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    public abstract void FlushSend();
}

public abstract class Session
{
    private Socket _socket = null;
    public bool _disconnected = false;

    private readonly object _lock = new();
    private readonly PacketBuffer _recvBuffer = new PacketBuffer(65535);
    private readonly Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
    private readonly List<ArraySegment<byte>> _pendingForSend = new List<ArraySegment<byte>>();
    private readonly SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    private readonly SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

    public abstract void OnConnected(EndPoint endPoint);
    public abstract void OnDisconnected(EndPoint endPoint);
    public abstract int OnRecv(ArraySegment<byte> buffer);
    public abstract void OnSend(int sendBytes);

    public void SetSocket(Socket socket)
    {
        _socket = socket;
    }

    private void Clear()
    {
        lock (_lock)
        {
            _sendQueue.Clear();
            _pendingForSend.Clear();
        }
    }

    public void Start(Socket socket)
    {
        _socket = socket;
        _recvArgs.Completed += OnRecvCompleted;
        _sendArgs.Completed += OnSendCompleted;
        RegisterRecv();
    }

    public void Send(List<ArraySegment<byte>> sendBuffList)
    {
        if (sendBuffList.Count == 0)
            return;

        lock (_lock)
        {
            foreach (ArraySegment<byte> sendBuff in sendBuffList)
                _sendQueue.Enqueue(sendBuff);

            if (_pendingForSend.Count == 0)
                RegisterSend();
        }
    }

    public void Send(ArraySegment<byte> sendBuff)
    {
        lock (_lock)
        {
            _sendQueue.Enqueue(sendBuff);
            if (_pendingForSend.Count == 0)
                RegisterSend();
        }
    }

    public void Disconnect()
    {
        if (!_disconnected)
            return;

        _disconnected = true;
        OnDisconnected(_socket.RemoteEndPoint);
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        Clear();
    }

    public void RegisterSend()
    {
        if (_disconnected)
            return;

        while (_sendQueue.Count > 0)
        {
            ArraySegment<byte> sendBuff = _sendQueue.Dequeue();
            _pendingForSend.Add(sendBuff);
        }
        _sendArgs.BufferList = _pendingForSend;

        try
        {
            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    void OnSendCompleted(object sender, SocketAsyncEventArgs args)
    {
        lock (_lock)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    _sendArgs.BufferList = null;
                    _pendingForSend.Clear();

                    OnSend(_sendArgs.BytesTransferred);

                    if (_sendQueue.Count > 0)
                        RegisterSend();
                }
                catch (Exception e)
                {
                    Debug.LogError($"OnSendCompleted Failed {e}");
                }
            }
            else
            {
                Disconnect();
            }
        }
    }

    public void RegisterRecv()
    {
        if (_disconnected)
        {
            Debug.Log("RegisterRecv Disconnected");
            return;
        }

        _recvBuffer.Clean();
        var segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

        try
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (!pending)
                OnRecvCompleted(null, _recvArgs);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            try
            {
                if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                {
                    Disconnect();
                    return;
                }

                var processLen = OnRecv(_recvBuffer.ReadSegment);
                if (processLen < 0 || _recvBuffer.DataSize < processLen)
                {
                    Disconnect();
                    return;
                }

                if (_recvBuffer.OnRead(processLen) == false)
                {
                    Disconnect();
                    return;
                }

                RegisterRecv();
            }
            catch (Exception e)
            {
                Debug.Log($"OnRecvCompleted Failed {e}");
            }
        }
        else
        {
            Disconnect();
        }
    }
}
