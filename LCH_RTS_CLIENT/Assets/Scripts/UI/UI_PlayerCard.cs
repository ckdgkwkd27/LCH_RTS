using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UI_PlayerCard : MonoBehaviour
{
    private List<Card> cards = new List<Card>();
    private const int MAX_CARD_LIST = 4;

    void Start()
    {
        AddCardItem(new Card(1, 1, "Cube"));
    }

    void Update()
    {
       
    }

    void UpdateUI() 
    {
        transform.Cast<Transform>().ToList().ForEach(child => Destroy(child.gameObject));


        var deck = cards.Take(MAX_CARD_LIST).ToList();
        foreach (var card in deck)
        {
            var go = Managers.Resource.Instantiate($"UI/PlayerCardSlot");
            UI_PlayerCardItem item = go.GetComponent<UI_PlayerCardItem>();
            if (item != null)
            {
                item.transform.SetParent(transform, false);
                item.cardInfo = card;
                item.SetCostAndName(card.cost, card.name);
            }
        }
    }

    void AddCardItem(Card card)
    {
        cards.Add(card);
        UpdateUI();
    }
}
