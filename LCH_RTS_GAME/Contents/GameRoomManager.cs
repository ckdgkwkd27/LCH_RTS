using LCH_RTS.Job;
using System.Collections.Generic;
using System.Linq;

namespace LCH_RTS.Contents;
//#TODO: 별개의 매칭서버로 옮겨야 한다!

public sealed class GameRoomManager
{
    private static readonly GameRoomManager _instance = new();
    private GameRoomManager() { }
    public static readonly GameRoomManager Instance = _instance;
    
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
    
    public void Update()
    {
        foreach (var gameRoom in _rooms)
        {
            gameRoom.Flush();
        }
    }
}