using System.Threading;

namespace LCH_RTS_MATCHING;

public sealed class PlayerIdGenerator
{
    private static readonly Lazy<PlayerIdGenerator> _instance = new(() => new PlayerIdGenerator());
    public static PlayerIdGenerator Instance => _instance.Value;

    private long _currentId;

    private PlayerIdGenerator()
    {
        _currentId = 0;
    }

    public long NextId()
    {
        return Interlocked.Increment(ref _currentId);
    }
}