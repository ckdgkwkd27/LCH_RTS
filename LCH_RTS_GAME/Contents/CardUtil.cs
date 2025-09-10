using Google.FlatBuffers;

namespace LCH_RTS.Contents;

public static class CardUtil
{
    public static void DrawCard(PlayerDeck deck, List<Card> currHands)
    {
        while(true)
        {
            var newCard = deck[deck.DeckPtr];
            deck.DeckPtr = (deck.DeckPtr + 1) % PlayerDeck.MAX_CARD_LIST;
            if (currHands.Contains(newCard)) 
                continue;
            
            currHands.Add(newCard);
            break;
        }
    }
    
    public static CardInfo ConvertToCardInfo(Card card)
    {
        var builder = new FlatBufferBuilder(1024);
        var cardNameOffset = builder.CreateString(card.Name);
        var cardInfoOffset = CardInfo.CreateCardInfo(builder, card.UnitType, card.Cost, cardNameOffset);
        builder.Finish(cardInfoOffset.Value);
                
        var buffer = builder.DataBuffer;
        var cardInfo = CardInfo.GetRootAsCardInfo(buffer);
        return cardInfo;
    }

    public static List<CardInfo> ConvertToCardInfos(List<Card> cards)
    {
        List<CardInfo> cardInfos = [];
        var builder = new FlatBufferBuilder(1024);

        foreach (var cardInfoOffset in from card in cards let cardNameOffset = builder.CreateString(card.Name) select CardInfo.CreateCardInfo(builder, card.UnitType, card.Cost, cardNameOffset))
        {
            builder.Finish(cardInfoOffset.Value);
            var buffer = builder.DataBuffer;
            var cardInfo = CardInfo.GetRootAsCardInfo(buffer);
            cardInfos.Add(cardInfo);
        }

        return cardInfos;
    }
}