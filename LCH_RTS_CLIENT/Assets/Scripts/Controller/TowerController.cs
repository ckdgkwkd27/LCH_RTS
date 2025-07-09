using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : UnitBaseController
{
    [SerializeField]
    private float _shootDelay = 1.0f;

    [SerializeField]
    private GameObject _bullet;
    List<GameObject> _bullets = new List<GameObject>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {

    }

    public override void OnAttack(UnitBaseController ub)
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
            bullet.GetComponent<BulletController>()._tower = this;
        }
    }

    public override void OnRemove()
    {
        foreach(var bullet in _bullets)
        {
            Destroy(bullet);
        }
    }
}
