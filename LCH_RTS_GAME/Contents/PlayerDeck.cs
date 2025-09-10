using LCH_RTS.Contents.Units;

namespace LCH_RTS.Contents;

public class PlayerDeck
{
    // ReSharper disable once InconsistentNaming
    public const int MAX_CARD_LIST = 3;
    public int DeckPtr { get; set; } = 0;

    private readonly Card[] _deck = new Card[MAX_CARD_LIST];

    public void MakeTestDeck()
    {
        int[] unitTypes = [1, 4, 5];
        for (var i = 0; i < unitTypes.Length; i++)
        {
            var displayInfo = UnitUtil.GetUnitDisplayInfoConfig(unitTypes[i]);
            _deck[i] = new Card(displayInfo.UnitType, displayInfo.Cost, displayInfo.Name);
        }
    }

    public List<Card> ShuffleAndTake(int handSize)
    {
        var rand = new Random();
        return _deck.OrderBy(_ => rand.Next()).Take(handSize).ToList();
    }

    public Card this[int deckPtr] => _deck[deckPtr];
}