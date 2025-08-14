

using System;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public UnitBaseController _attacker;
    public UnitBaseController _target;
    private float Speed = 10.0f;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (_target)
        {
            transform.LookAt(_target.transform.position);
            transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, Time.deltaTime * Speed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other is not null)
        {
            Destroy(gameObject);
            return;
        }
    }
}