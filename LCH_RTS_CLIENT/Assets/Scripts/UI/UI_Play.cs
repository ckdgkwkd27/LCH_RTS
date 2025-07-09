using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UI_Play : MonoBehaviour
{
    private TextMeshProUGUI costText;
    private GameObject field;
    private UnityEngine.UI.Image playerColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        costText = GameObject.Find("CostText").GetComponent<TextMeshProUGUI>();
        field = GameObject.Find("Field");
        playerColor = GameObject.Find("PlayerColor").GetComponent<UnityEngine.UI.Image>();
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
}
