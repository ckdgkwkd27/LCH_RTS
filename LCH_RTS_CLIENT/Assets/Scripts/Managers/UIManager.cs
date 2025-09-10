using System.Xml.Linq;
using UnityEngine;

public class UIManager
{
    int _order = 10;
    public UI_Login LoginUI { get; private set; }
    public UI_Play PlayUI { get; private set; }
    public UI_PlayerCard CardUI { get; private set; }

    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        if (sort)
        {
            canvas.sortingOrder = _order;
        }
        else
        {
            canvas.sortingOrder = 0;
        }
    }

    public UI_Play ShowPlayUI()
    {
        var uiPlay = GameObject.Find("UI_Play");
        if (uiPlay != null)
        {
            PlayUI = uiPlay.GetComponent<UI_Play>();
            return PlayUI;
        }

        GameObject go = Managers.Resource.Instantiate($"UI_Play");
        UI_Play sceneUI = Util.GetOrAddComponent<UI_Play>(go);
        PlayUI = sceneUI;
        return sceneUI;
    }

    public UI_PlayerCard ShowCardUI()
    {
        var uiCard = GameObject.Find("UI_PlayerCard");
        if(uiCard != null)
        {
            CardUI = uiCard.GetComponent<UI_PlayerCard>();
            return CardUI;
        }

        GameObject go = Managers.Resource.Instantiate($"UI_PlayerCard");
        if (go == null)
        {
            Debug.LogError("Failed to instantiate UI_PlayerCard prefab");
            return null;
        }
        
        UI_PlayerCard sceneUI = Util.GetOrAddComponent<UI_PlayerCard>(go);
        CardUI = sceneUI;
        return sceneUI;
    }
}