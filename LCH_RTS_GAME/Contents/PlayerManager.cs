namespace LCH_RTS.Contents;

public sealed class PlayerManager
{
    private static readonly Lazy<PlayerManager> _instance = new(() => new PlayerManager());
    public static PlayerManager Instance => _instance.Value;
    
    private readonly List<Player> _players = [];
    private readonly Lock _lock = new();
    
    public Player AddPlayer(ClientSession session, long playerId)
    {
        Player player;
        using (_lock.EnterScope())
        {
            player = new Player(playerId);
            _players.Add(player);
        }

        player.Session = session;
        return player;
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