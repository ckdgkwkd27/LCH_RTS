using System.Collections;
using System.Collections.Generic;
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

    [SerializeField]
    private float _shootDelay = 1.0f;

    [SerializeField]
    private GameObject _bullet;
    List<GameObject> _bullets = new List<GameObject>();

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

    public void OnTakeDamage(int remainHp)
    {
        Stat.CurrHp = remainHp;
        HealthBar.value = (float)Stat.CurrHp / Stat.MaxHp;
    }

    public void OnAttack(UnitBaseController ub)
    {
        StartCoroutine(shoot(ub));
    }

    IEnumerator shoot(UnitBaseController target)
    {
        yield return new WaitForSeconds(_shootDelay);

        {
            GameObject bullet = Instantiate(_bullet, transform.position, Quaternion.identity);
            _bullets.Add(bullet);
            bullet.GetComponent<BulletController>()._target = target;
            bullet.GetComponent<BulletController>()._attacker = this;
        }
    }

    public void OnRemove()
    {
        foreach (var bullet in _bullets)
        {
            Destroy(bullet);
        }
    }
}
