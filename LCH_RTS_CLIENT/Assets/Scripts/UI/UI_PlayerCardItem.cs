using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_PlayerCardItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IEndDragHandler
{
    public Card cardInfo;
    private TextMeshProUGUI costText;
    private TextMeshProUGUI cardNameText;
    private Image background;

    private void Awake()
    {
        costText = transform.Find("Cost").GetComponent<TextMeshProUGUI>();
        cardNameText = transform.Find("CardName").GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        background = GetComponent<Image>();
    }

    void Update()
    {
        
    }

    public void SetCostAndName(int cost,  string name)
    {
        costText.text = cost.ToString();
        cardNameText.text = name;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        background.color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.color = new Color32(147, 71, 71, 100);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            var playerGo = GameObject.Find("player");
            if (playerGo == null)
            {
                Debug.LogError("OnDrag Owner Player Is NULL");
                return;
            }

            var pc = playerGo.GetComponent<PlayerController>();
            if (pc == null)
            {
                Debug.LogError("OnDrag Owner PC Is NULL");
                return;
            }

            Vector3 worldPos = hit.point;
            Managers.Network.SendToGame(PacketUtil.CS_UNIT_SPAWN_Packet(pc.RoomId, cardInfo.unitType, worldPos));
            Debug.Log($"EndDrag=>UnitType={cardInfo.unitType}");
        }
    }
}
