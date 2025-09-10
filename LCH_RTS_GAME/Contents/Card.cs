using Google.FlatBuffers;

namespace LCH_RTS.Contents;
public class Card(int unitType, int cost, string name)
{
    public readonly int UnitType = unitType;
    public readonly int Cost = cost;
    public readonly string Name = name;
    
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            null => false,
            Card other => UnitType == other.UnitType && Cost == other.Cost && Name == other.Name,
            _ => false
        };
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(UnitType, Cost, Name);
    }
}