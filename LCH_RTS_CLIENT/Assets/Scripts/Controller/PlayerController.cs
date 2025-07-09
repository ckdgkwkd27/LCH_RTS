using UnityEngine;

public enum EPlayerSide
{
    Blue,
    Red,

    Max
}

public class PlayerController : MonoBehaviour
{
    public static long PlayerId { get; private set; }
    public long RoomId { get; private set; } = 0;
    public EPlayerSide PlayerSide {  get; private set; }
    public int CurrCost { get; private set; } = 0;
    public int MaxCost { get; private set; } = 0;

    public static void SetPlayerId(long id)
    {
        PlayerId = id;
    }

    public void Init(long playerId, long roomId, EPlayerSide playerSide, int currCost, int maxCost)
    {
        PlayerId = playerId;
        RoomId = roomId;
        PlayerSide = playerSide;
        CurrCost = currCost;
        MaxCost = maxCost;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Managers.UI.ShowPlayUI().UpdatePlayerColor(PlayerSide);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetCost(int currCost)
    {
        CurrCost = currCost;
        Managers.UI.ShowPlayUI().UpdateCostText(CurrCost, MaxCost);
    }
}
