
public struct Card
{
    public int cost;
    public int unitType;
    public string name;
    public Card(int cost, int unitType, string name)
    {
        this.cost = cost;
        this.unitType = unitType;
        this.name = name;
    }
}