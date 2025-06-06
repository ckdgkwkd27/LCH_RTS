using UnityEngine.EventSystems;
using UnityEngine;

public class GameRoomScene : MonoBehaviour
{
    public void Init(long roomId, long playerId, EPlayerSide playerSide, int currCost, int maxCost)
    {
        Managers.Object.Clear();

        GameObject go = new GameObject();
        go.name = "player";
        go.AddComponent<PlayerController>();
        Instantiate(go);

        var playerController = go.GetComponent<PlayerController>();
        playerController.Init(playerId, roomId, playerSide, currCost, maxCost);
    }
}