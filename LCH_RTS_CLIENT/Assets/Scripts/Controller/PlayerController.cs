using System.Collections.Generic;
using UnityEngine;

public enum EPlayerSide
{
    Blue,
    Red,

    Max
}

public class PlayerController : MonoBehaviour
{
    public long PlayerId { get; private set; }
    public long RoomId { get; private set; } = 0;
    public EPlayerSide PlayerSide {  get; private set; }
    public int CurrCost { get; private set; } = 0;
    public int MaxCost { get; private set; } = 0;
    public List<Card> PlayerHands {  get; private set; } = new List<Card>();

    public void Init(long playerId, long roomId, EPlayerSide playerSide, int currCost, int maxCost, List<Card> playerHands)
    {
        PlayerId = playerId;
        RoomId = roomId;
        PlayerSide = playerSide;
        CurrCost = currCost;
        MaxCost = maxCost;
        PlayerHands = playerHands;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Managers.UI.ShowPlayUI().UpdatePlayerColor(PlayerSide);
        Managers.UI.ShowCardUI().SetCardItems(PlayerHands);
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

    public void SetHands(List<Card> hands)
    {
        Managers.UI.ShowCardUI().SetCardItems(hands);
    }

    public void SetResultImage(bool isWinner)
    {
        var playUI = Managers.UI.ShowPlayUI();
        if (isWinner)
        {
            playUI.SetWinnerImage();
        }
        else
        {
            playUI.SetLoserImage();
        }
    }
}
