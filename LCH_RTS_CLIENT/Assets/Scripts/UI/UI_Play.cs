using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Play : MonoBehaviour
{
    private Canvas canvas;
    private TextMeshProUGUI costText;
    private GameObject field;
    private UnityEngine.UI.Image playerColor;
    private UnityEngine.UI.Image matchEndImage;
    private TextMeshProUGUI matchEndText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        costText = GameObject.Find("CostText").GetComponent<TextMeshProUGUI>();
        field = GameObject.Find("Field");
        playerColor = GameObject.Find("PlayerColor").GetComponent<UnityEngine.UI.Image>();
        
        var matchEndImageGo = canvas.transform.Find("MatchEndImage");
        if (matchEndImageGo != null)
        {
            matchEndImage = matchEndImageGo.GetComponent<Image>();
            matchEndText = matchEndImageGo.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateCostText(int currCost, int maxCost)
    {
        costText.text = $"{currCost} / {maxCost}";
    }

    public void UpdateRedUI()
    {
        field.transform.position = new Vector3(5, 17, -11);
        field.transform.rotation = new Quaternion(180, 90, -90, field.transform.rotation.z);
    }

    public void UpdatePlayerColor(EPlayerSide playerSide)
    {
        if(playerSide == EPlayerSide.Blue)
        {
            playerColor.color = Color.blue;
        }
        else if(playerSide == EPlayerSide.Red)
        {
            playerColor.color = Color.red;
        }
        else
        {
            Debug.LogError("No TeamSide Error");
        }
    }

    public void SetWinnerImage()
    {
        if (matchEndImage == null || matchEndText == null)
        {
            Debug.LogWarning("MatchEndImage or MatchEndText is not initialized");
            return;
        }

        matchEndText.text = "Victory";
        matchEndImage.gameObject.SetActive(true);
    }

    public void SetLoserImage()
    {
        if (matchEndImage == null || matchEndText == null)
        {
            Debug.LogWarning("MatchEndImage or MatchEndText is not initialized");
            return;
        }

        matchEndText.text = "Defeat";
        matchEndImage.gameObject.SetActive(true);
    }
}
