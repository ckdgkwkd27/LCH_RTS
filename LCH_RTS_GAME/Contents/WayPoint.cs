namespace LCH_RTS.Contents;

using System.Numerics;
public readonly struct WayPoint(Vector2 pos, int level)
{
    public Vector2 Pos { get; } = pos;
    public int Level { get; } = level;
}
