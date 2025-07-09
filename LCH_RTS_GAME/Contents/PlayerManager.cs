namespace LCH_RTS.Contents;

public sealed class PlayerManager
{
    private static readonly PlayerManager _instance = new();
    private PlayerManager() { }
    public static readonly PlayerManager Instance = _instance;
    
    private readonly List<Player> _players = [];
    private readonly Lock _lock = new();
    
    // ReSharper disable once InconsistentNaming
    private static long LAST_PLAYER_ID = 1;
    
    public Player AddPlayer(ClientSession session)
    {
        Player player;
        using (_lock.EnterScope())
        {
            player = new Player(LAST_PLAYER_ID);
            _players.Add(player);
        }

        player.Session = session;
        ++LAST_PLAYER_ID;
        return _players.Last();
    }
    
    public Player? GetPlayer(long playerId)
    {
        return _players.FirstOrDefault(p => p.PlayerId == playerId) ?? null;
    }

    public Player? GetPlayer(ClientSession session)
    {
        return _players.FirstOrDefault(p => p.Session == session) ?? null;
    }

    public ClientSession? GetClientSession(long playerId)
    {
        return _players.FirstOrDefault(p => p.PlayerId == playerId)?.Session;
    }
}