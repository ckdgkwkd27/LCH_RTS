using LCH_RTS.Job;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LCH_RTS.Contents;

public sealed class GameRoomManager
{
    private static readonly Lazy<GameRoomManager> _instance = new(() => new GameRoomManager());
    public static GameRoomManager Instance => _instance.Value;
    
    private readonly List<GameRoom> _rooms = [];
    private readonly Lock _lock = new();
    
    public void Init(int roomCount)
    {
        for (var i = 1; i <= roomCount; i++)
        {
            _rooms.Add(new GameRoom(i));
        }
    }

    public void Add(GameRoom room)
    {
        using (_lock.EnterScope())
        {
            _rooms.Add(room);
        }
    }

    public void Remove(GameRoom room)
    {
        using (_lock.EnterScope())
        {
            _rooms.Remove(room);
        }
    }

    public GameRoom? GetRoom(long roomId)
    {
        return _rooms.FirstOrDefault(r => r.RoomId == roomId) ?? null;
    }

    public void Update(long roomId)
    {
        using (_lock.EnterScope())
        {
            var gameRoom = GetRoom(roomId);
            gameRoom?.Flush();
        }
    }
}