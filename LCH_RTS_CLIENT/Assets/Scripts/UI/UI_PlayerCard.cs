using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UI_PlayerCard : MonoBehaviour
{
    private List<Card> _cards = new List<Card>();
    private const int MAX_CARD_LIST = 4;

    void Start()
    {

    }

    void Update()
    {
       
    }

    void UpdateUI() 
    {
        transform.Cast<Transform>().ToList().ForEach(child => Destroy(child.gameObject));


        var deck = _cards.Take(MAX_CARD_LIST).ToList();
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

    public void AddCardItem(Card card)
    {
        _cards.Add(card);
        UpdateUI();
    }

    public void ClearCardItems()
    {
        _cards.Clear();
        UpdateUI();
    }

    public void AddCardItems(List<Card> cards)
    {
        foreach (var card in cards)
        {
            _cards.Add(card);
        }
        UpdateUI();
    }

    public void SetCardItems(List<Card> cards)
    {
        _cards.Clear();
        foreach (var card in cards)
        {
            _cards.Add(card);
        }
        UpdateUI();
    }
}
