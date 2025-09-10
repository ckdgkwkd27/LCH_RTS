using System.Collections.Generic;
using UnityEngine;

public class GameRoomScene : MonoBehaviour
{
    public void Init(long roomId, long playerId, EPlayerSide playerSide, int currCost, int maxCost, List<Card> playerHands, GameSession session)
    {
        Managers.Object.Clear();

        GameObject go = new GameObject();
        go.name = "player";
        go.AddComponent<PlayerController>();

        var playerController = go.GetComponent<PlayerController>();
        playerController.Init(playerId, roomId, playerSide, currCost, maxCost, playerHands);
        session.PlayerController = playerController;
    }
}