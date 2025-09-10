using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class ObjectManager
{
    Dictionary<long, GameObject> _objects = new Dictionary<long, GameObject>();

    public void Remove(long id)
    {
        GameObject go = FindById(id);
        if (go == null)
            return;

        _objects.Remove(id);
        Managers.Resource.Destroy(go);
    }

    public void Add(long unitId, EPlayerSide playerSide, int unitType, Vector2 pos, BaseStat stat)
    {
        var unitName = Util.GetNameFromUnitType(unitType);
        var sideName = playerSide == EPlayerSide.Blue ? "Blue" : "Red";

        GameObject go = Managers.Resource.Instantiate($"Unit/{sideName}/{unitName}");
        go.name = unitName + unitId.ToString();
        go.transform.position = new Vector3(pos.x, pos.y, -6.7f);
        go.transform.Rotate(new Vector3(90f, 50f, 50f));

        UnitBaseController uc = go.GetComponent<UnitBaseController>();
        if (uc is null)
        {
            Debug.LogError("GetComponent <UnitBaseController> is null!");
            return;
        }

        uc.UnitId = unitId;
        uc.Pos = pos;
        uc.Stat = stat;

        _objects.Add(unitId, go);
    }

    public GameObject FindById(long id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public GameObject Find(Func<GameObject, bool> condition)
    {
        foreach (GameObject _go in _objects.Values)
        {
            if (condition.Invoke(_go))
                return _go;
        }

        return null;
    }

    public List<GameObject> FindAll(Func<GameObject, bool> condition)
    {
        List<GameObject> gameObjects = new List<GameObject>();
        foreach(GameObject _go in _objects.Values)
        {
            if (condition.Invoke(_go))
                gameObjects.Add(_go);
        }
        return gameObjects;
    }

    public void Clear()
    {
        foreach (GameObject _go in _objects.Values.ToList())
        {
            Managers.Resource.Destroy(_go);
            _objects.Clear();
        }
    }
}