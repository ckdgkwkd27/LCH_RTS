using UnityEngine;
using System;

public class PacketBuffer
{
    private int _capacity;
    private int _bufferSize = 0;
    private int _readPos = 0;
    private int _writePos = 0;
    private ArraySegment<byte> _buffer;
    public PacketBuffer(int bufferSize)
    {
        _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
    }

    public int DataSize => _writePos - _readPos;
    public ArraySegment<byte> ReadSegment => new(_buffer.Array, _buffer.Offset + _readPos, DataSize);
    public ArraySegment<byte> WriteSegment => new(_buffer.Array, _buffer.Offset + _writePos, FreeSize);
    private int FreeSize => _buffer.Count - _writePos;
    private byte? ReadPos => _buffer.Array?[_readPos];
    private byte? WritePos => _buffer.Array?[_writePos];

    public void Clean()
    {
        var dataSize = DataSize;
        if (dataSize == 0)
        {
            _readPos = 0;
            _writePos = 0;
        }
        else
        {
            if (FreeSize >= _bufferSize) return;
            if (_buffer.Array != null)
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
            _readPos = 0;
            _writePos = dataSize;
        }
    }

    public bool OnRead(int bytes)
    {
        if (bytes > DataSize)
            return false;
        
        _readPos += bytes;
        return true;
    }

    public bool OnWrite(int bytes)
    {
        if(bytes > FreeSize)
            return false;
        
        _writePos += bytes;
        return true;
    }
}