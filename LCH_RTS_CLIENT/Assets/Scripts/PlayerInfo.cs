
using UnityEngine;

public class PlayerInfo : ScriptableObject
{
    [SerializeField]
    private long _playerId;
    public long PlayerId => _playerId;


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
}
