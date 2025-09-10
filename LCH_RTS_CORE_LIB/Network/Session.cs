using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using Google.FlatBuffers;

namespace LCH_RTS_CORE_LIB.Network;

public enum EPacketSessionCategory
{
    None,
    ClientSession,
    LobbySession,
    MatchingSesion,
    GameSession,
        
    Max
}
public abstract class PacketSession : Session
{
    protected EPacketSessionCategory sessionCategory = EPacketSessionCategory.None;
    public override int OnRecv(ArraySegment<byte> buffer)
    {
        const int headerSize = 2;
        var processLen = 0;

        while (true)
        {
            if(buffer.Count < headerSize || buffer == null || buffer.Array == null)
                break;
            
            var dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            if(buffer.Count < dataSize)
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
    private Socket? _socket = null;
    private bool _disconnected = false;

    private readonly Lock _lock = new Lock();
    private readonly PacketBuffer _recvBuffer = new PacketBuffer(65535);
    private readonly Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
    private readonly List<ArraySegment<byte>> _pendingForSend = new List<ArraySegment<byte>>();
    private readonly SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
    private readonly SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        
    private readonly SocketAsyncEventArgs _acceptArgs = new SocketAsyncEventArgs();

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
        using (_lock.EnterScope())  
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

        using (_lock.EnterScope())  
        {
            foreach (ArraySegment<byte> sendBuff in sendBuffList)
                _sendQueue.Enqueue(sendBuff);

            if (_pendingForSend.Count == 0)
                RegisterSend();
        }
    }
        
    public void Send(ArraySegment<byte> sendBuff)
    {
        using (_lock.EnterScope())  
        {
            _sendQueue.Enqueue(sendBuff);
            if (_pendingForSend.Count == 0)
                RegisterSend();
        }
    }
        
    public void Disconnect()
    {
        if (Interlocked.Exchange(ref _disconnected, true) == true)
            return;

        if (_socket?.RemoteEndPoint is null)
            return;

        OnDisconnected(_socket.RemoteEndPoint);
        _socket.Shutdown(SocketShutdown.Both);
        _socket.Close();
        Clear();
    }

    public void RegisterSend()
    {
        if(_disconnected)
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
            Console.WriteLine(e);
        }
    }

    void OnSendCompleted(object? sender, SocketAsyncEventArgs args)
    {
        using (_lock.EnterScope())  
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
                    Console.WriteLine($"OnSendCompleted Failed {e}");
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
            Console.WriteLine("RegisterRecv Disconnected");
            return;
        }

        _recvBuffer.Clean();
        var segment = _recvBuffer.WriteSegment;
        _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

        try
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if(!pending)
                OnRecvCompleted(null, _recvArgs);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void OnRecvCompleted(object? sender, SocketAsyncEventArgs args)
    {
        if (args is { BytesTransferred: > 0, SocketError: SocketError.Success })
        {
            try
            {
                // Write 커서 이동
                if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                {
                    Disconnect();
                    return;
                }

                // 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다
                var processLen = OnRecv(_recvBuffer.ReadSegment);
                if (processLen < 0 || _recvBuffer.DataSize < processLen)
                {
                    Disconnect();
                    return;
                }

                // Read 커서 이동
                if (_recvBuffer.OnRead(processLen) == false)
                {
                    Disconnect();
                    return;
                }

                RegisterRecv();
            }
            catch (Exception e)
            {
                Console.WriteLine($"OnRecvCompleted Failed {e}");
            }
        }
        else
        {
            Disconnect();
        }
    }
}