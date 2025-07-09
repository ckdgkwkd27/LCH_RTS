namespace LCH_RTS.Contents;

public class PlayerInGameInfo(Player player, EPlayerSide playerSide, long roomId, int remainCost)
{
    public Player Player { get; set; } = player;
    public EPlayerSide PlayerSide = playerSide;
    public long RoomId = roomId;
    public int RemainCost = remainCost;
}

public class Player(long playerId)
{
    public long PlayerId { get; } = playerId;
    public ClientSession? Session { get; set; }
}