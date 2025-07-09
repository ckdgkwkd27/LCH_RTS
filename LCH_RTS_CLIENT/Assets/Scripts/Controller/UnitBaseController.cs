using UnityEngine;
using UnityEngine.UI;

public class BaseStat
{
    public int attack;
    public int MaxHp;
    public int CurrHp;
    public float speed;
    public int cost;
    public float attackRange;
    public float sight;

    public static BaseStat ConvertFrom(UnitStat stat)
    {
        BaseStat bs = new BaseStat();
        bs.attack = stat.Attack;
        bs.MaxHp = stat.MaxHp;
        bs.CurrHp = stat.CurrHp;
        bs.speed = stat.Speed;
        bs.cost = stat.Cost;
        bs.attackRange = stat.AttackRange;
        bs.sight = stat.Sight;
        return bs;
    }
}

public class UnitBaseController : MonoBehaviour
{
    public long UnitId { get; set; }
    public string Name { get; set; }
    public Vector2 Pos { get; set; }
    public BaseStat Stat { get; set; }

    private Slider HealthBar;

    [SerializeField]
    private Vector3 canvasPos;

    [SerializeField]
    private Vector3 canvasRot;

    [SerializeField]
    private Vector3 canvasScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        GameObject hpBarCanvas = Managers.Resource.Instantiate($"UI/HpBarCanvas");
        hpBarCanvas.transform.SetParent(transform);

        RectTransform rectTransform = hpBarCanvas.GetComponent<RectTransform>();
        rectTransform.anchoredPosition3D = canvasPos;
        rectTransform.localRotation = Quaternion.Euler(canvasRot);
        rectTransform.transform.localScale = canvasScale;

        HealthBar = hpBarCanvas.transform.Find("HpBar").GetComponent<Slider>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    public virtual void OnAttack(UnitBaseController ub)
    {

    }

    public void OnTakeDamage(int remainHp)
    {
        Stat.CurrHp = remainHp;
        HealthBar.value = (float)Stat.CurrHp / Stat.MaxHp;
    }

    public virtual void OnRemove() { }
}
