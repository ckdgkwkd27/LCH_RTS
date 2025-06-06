using Google.FlatBuffers;
using UnityEngine;

public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    public static string GetNameFromUnitType(int type)
    {
        string name = null; 
        switch (type)
        {
            case 1:
                name = "Cube";
                break;
            case 2:
                name = "SideTower";
                break;
             case 3:
                name = "KingTower";
                break;
            default:
                break;
        }

        return name;
    }

    public static Vec2 CreateVec2(float x, float y)
    {
        var builder = new FlatBufferBuilder(1024);
        var vecOffset = Vec2.CreateVec2(builder, x, y);
        builder.Finish(vecOffset.Value);
        return Vec2.GetRootAsVec2(builder.DataBuffer);
    }

    public static UnitStat CreateUnitStat(int attack, int maxHp, int currHp, float speed, int cost)
    {
        var builder = new FlatBufferBuilder(1024);
        var statOffset = UnitStat.CreateUnitStat(builder, attack, maxHp, currHp, speed, cost);
        builder.Finish(statOffset.Value);
        return UnitStat.GetRootAsUnitStat(builder.DataBuffer);
    }
}

