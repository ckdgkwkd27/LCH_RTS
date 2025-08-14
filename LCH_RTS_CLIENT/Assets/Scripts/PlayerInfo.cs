
using UnityEngine;

//#TODO: Remove This;

public class PlayerInfo : ScriptableObject
{
    private long _playerId;
    public long PlayerId => _playerId;

    private long _roomId;
    public long RoomId => _roomId;


    private static PlayerInfo _instance;
    public static PlayerInfo Instance
    {
        get 
        {
            if (_instance == null)
            {
                _instance = CreateInstance<PlayerInfo>();
            }

            return _instance;
        }
    }

    public void SetPlayerId(long playerId)
    {
        _playerId = playerId;
    }

    public void SetRoomId(long roomId) 
    {
        _roomId = roomId;
    }
}
