using System.Numerics;
using Google.FlatBuffers;

namespace LCH_RTS.Contents.Units;

public abstract class UnitUtil
{
    public static int GetUnitTypeFromName(string name)
    {
        return name switch
        {
            "Cube" => 1,
            "SideTower" => 2,
            "KingTower" => 3,
            _ => 0
        };
    }
    public static UnitStat GetUnitStatConfig(int unitType)
    {
        int attack = 0, maxHp = 0, currHp = 0, cost = 0;
        float speed = 0f, attackRange = 0f, sight = 0f;
        
        switch (unitType)
        {
            case 1: //Cube
                attack = 10;
                maxHp = currHp = 300;
                speed = 2.0f;
                cost = 1;
                attackRange = 10.0f;
                sight = 15.0f;
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