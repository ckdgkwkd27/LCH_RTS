namespace LCH_RTS.Contents;

public class Player(long playerId)
{
    public long PlayerId { get; } = playerId;
    public ClientSession? Session { get; set; }
}