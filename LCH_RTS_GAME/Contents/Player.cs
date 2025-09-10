namespace LCH_RTS.Contents;

public class PlayerInGameInfo(Player player, EPlayerSide playerSide, long roomId, int remainCost, PlayerDeck deck, List<Card> hand)
{
    public Player Player { get; set; } = player;
    public readonly EPlayerSide PlayerSide = playerSide;
    public long RoomId = roomId;
    public int RemainCost = remainCost;
    public readonly PlayerDeck Deck = deck;
    public List<Card> Hand = hand;
}

public class Player(long playerId)
{
    public long PlayerId { get; } = playerId;
    public ClientSession? Session { get; set; }
}