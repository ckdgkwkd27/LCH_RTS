using LCH_COMMON;

namespace LCH_RTS.Contents.Units;

public class Tower : UnitBase
{
    private Dictionary<EUnitStatus, Action> StatusActions { get;}
    public enum ETowerType
    {
        Side,
        King
    }

    public ETowerType TowerType { get; set; }
    
    public Tower(long unitId, EPlayerSide playerSide, Vec2 pos, UnitStat stat, int unitType, long roomId, ETowerType towerType)  : base(unitId, playerSide, pos, stat, unitType, roomId)
    {
        Status = EUnitStatus.Attack;
        StatusActions = new Dictionary<EUnitStatus, Action>
        {
            { EUnitStatus.Attack, UpdateAttack },
        };
        
        TowerType = towerType;
    }

    public override void Update()
    {
        if (Status is EUnitStatus.Idle or EUnitStatus.Chase) 
            return;

        if (StatusActions.TryGetValue(Status, out var action))
        {
            action();
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    //타워는 유닛만 공격
    protected override void UpdateAttack()
    {
        if (Target is not Unit) 
            return;

        var gameRoom = GameRoomManager.Instance.GetRoom(RoomId);
        if (gameRoom is null)
            return;

        if (!gameRoom.ContainUnit(Target))
            return;
        
        var remainHp = Target.Stat.CurrHp - Stat.Attack;
        gameRoom.Broadcast(PacketUtil.SC_UNIT_ATTACK_PACKET(RoomId, UnitId, Target.UnitId, remainHp));
        if (remainHp >= 0)
        {
            var stat = UnitUtil.CreateUnitStat(Target.Stat.Attack, Target.Stat.MaxHp, remainHp, Target.Stat.Speed, Target.Stat.Cost, Target.Stat.AttackRange, Target.Stat.Sight);
            Target.Stat =  stat;
            return;
        }

        gameRoom.RemoveUnit(Target);
        Target = null;
    }

    public override void CheckClosestUnit(List<UnitBase>? enemyUnits)
    {
        if (enemyUnits is null || enemyUnits.Count == 0)
        {
            return;
        }
        
        foreach (var unit in enemyUnits.Where(unit => UnitUtil.GetDistanceSquare(Pos, unit.Pos) < Stat.AttackRange * Stat.AttackRange))
        {
            if(unit is not Unit) 
                continue;
            
            Status = EUnitStatus.Attack;
            Target = unit;     
            return;
        }

        Target = null;
    }

    protected override void OnDead(long roomId, EPlayerSide winnerSide, EPlayerSide loserSide)
    {
        if (TowerType != ETowerType.King) 
            return;
        
        var gameRoom = GameRoomManager.Instance.GetRoom(roomId);
        if (gameRoom == null)
        {
            Logger.Log(ELogType.Console, ELogLevel.Error, $"Error: Cannot Find Room (Tower::OnDead)");
            return;
        }

        gameRoom.SetRoomState(ERoomState.End);
        gameRoom.Broadcast(PacketUtil.SC_END_GAME_PACKET(gameRoom.RoomId, (sbyte)winnerSide, (sbyte)loserSide));
        Logger.Log(ELogType.Console, ELogLevel.Info, $"Game Finish.Winner={winnerSide},Loser={loserSide}");
    }
}