using System.Numerics;
using Google.FlatBuffers;

namespace LCH_RTS.Contents.Units;

public abstract class UnitUtil
{
    private const int MinSpawnX = -21;
    private const int MaxSpawnX = 43;
    private const int MinSpawnY = -9;
    private const int MaxSpawnY = 39;

    public static bool IsValidSpawnSpace(Vec2 pos)
    {
        var isInX = pos.X is > MinSpawnX and < MaxSpawnX;
        var isInY = pos.Y is > MinSpawnY and < MaxSpawnY;
        return isInX && isInY;
    }
    public static int GetUnitTypeFromName(string name)
    {
        return name switch
        {
            "Cube" => 1,
            "SideTower" => 2,
            "KingTower" => 3,
            "Sphere" => 4,
            "Cylinder" => 5,
            _ => 0
        };
    }

    public static string? GetUnitNameFromType(int unitType)
    {
        return unitType switch
        {
            1 => "Cube",
            2 => "SideTower",
            3 => "KingTower",
            4 => "Sphere",
            5 => "Cylinder",
            _ => null
        };
    }

    public struct UnitSimpleInfo(int unitType, string name, int cost)
    {
        public int UnitType = unitType;
        public string Name = name;
        public int Cost = cost;
    }

    public static UnitSimpleInfo GetUnitDisplayInfoConfig(int unitType)
    {
        UnitSimpleInfo displayInfo = default;
        switch (unitType)
        {
            case 1: //Cube
                displayInfo.UnitType = 1;
                displayInfo.Name = "Cube";
                displayInfo.Cost = 1;
                break;
            case 2: //SideTower
                displayInfo.UnitType = 2;
                displayInfo.Name = "SideTower";
                displayInfo.Cost = 0;
                break;
            case 3: //KingTower
                displayInfo.UnitType = 3;
                displayInfo.Name = "KingTower";
                displayInfo.Cost = 0;
                break;
            case 4: //Sphere
                displayInfo.UnitType = 4;
                displayInfo.Name = "Sphere";
                displayInfo.Cost = 2;
                break;
            case 5: //Cylinder
                displayInfo.UnitType = 5;
                displayInfo.Name = "Cylinder";
                displayInfo.Cost = 3;
                break;
            default:
                Environment.Exit(1);
                break;
        }

        return displayInfo;
    }
    
    public static UnitStat GetUnitStatConfig(int unitType)
    {
        int attack = 0, maxHp = 0, currHp = 0, cost = 0;
        float speed = 0f, attackRange = 0f, sight = 0f;
        
        switch (unitType)
        {
            case 1: //Cube
                attack = 10;
                maxHp = currHp = 100;
                speed = 2.0f;
                cost = 1;
                attackRange = 10.0f;
                sight = 8.0f;
                break;
            case 2: //SideTower
                attack = 10;
                maxHp = currHp = 100;
                speed = 0f;
                cost = 0;
                attackRange = 15.0f;
                break;
            case 3: //KingTower
                attack = 20;
                maxHp = currHp = 300;
                speed = 0f;
                cost = 0;   
                attackRange = 15.0f;
                break;
            case 4: //Sphere
                attack = 20;
                maxHp = currHp = 150;
                speed = 3.0f;
                cost = 2;
                attackRange = 3.0f;
                sight = 8.0f;
                break;
            case 5: //Cylinder
                attack = 15;
                maxHp = currHp = 50;
                speed = 4.0f;
                cost = 3;
                attackRange = 10.0f;
                sight = 8.0f;
                break;
            default:
                Environment.Exit(1);
                break;
        }
        
        var builder = new FlatBufferBuilder(1024);
        var unitStatOffset = UnitStat.CreateUnitStat(builder, attack, maxHp, currHp, speed, cost, attackRange, sight);
        builder.Finish(unitStatOffset.Value);
                
        var buffer = builder.DataBuffer;
        var unitStat = UnitStat.GetRootAsUnitStat(buffer);
        return unitStat;
    }

    public static Vec2 CreateVec2(float x, float y)
    {
        var builder = new FlatBufferBuilder(1024);
        var vecOffset = Vec2.CreateVec2(builder, x, y);
        builder.Finish(vecOffset.Value);
        return Vec2.GetRootAsVec2(builder.DataBuffer);
    }
    
    public static UnitStat CreateUnitStat(int attack, int maxHp, int currHp, float speed, int cost, float attackRange, float sight)
    {
        var builder = new FlatBufferBuilder(1024);
        var statOffset = UnitStat.CreateUnitStat(builder, attack,  maxHp, currHp, speed, cost, attackRange, sight);
        builder.Finish(statOffset.Value);
        return UnitStat.GetRootAsUnitStat(builder.DataBuffer);
    }

    public static float GetDistanceSquare(Vec2 from, Vec2 to)
    {
        var xsquare = (from.X- to.X) * (from.X- to.X);
        var ysquare = (from.Y - to.Y) * (from.Y - to.Y);
        return xsquare  + ysquare;
    }

    public static Vector2 GetNotSerializedVector(Vec2 vec)
    {
        return new Vector2(vec.X, vec.Y);
    }
}