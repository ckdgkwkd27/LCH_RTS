namespace LCH_RTS.Contents.Units;

public class Unit : UnitBase
{
    private Dictionary<EUnitStatus, Action> StatusActions { get;}

    public Unit(long unitId, EPlayerSide playerSide, Vec2 pos, UnitStat stat, int unitType, long roomId) : base(unitId, playerSide, pos, stat, unitType, roomId)
    {
        Status = EUnitStatus.Moving;
        StatusActions = new Dictionary<EUnitStatus, Action>
        {
            { EUnitStatus.Moving, UpdateMoving },
            { EUnitStatus.Attack, UpdateAttack },
            { EUnitStatus.Chase, UpdateChase },
            { EUnitStatus.Dead, UpdateDead }
        };
    }
    public override void Update()
    {
        if (Status == EUnitStatus.Idle) 
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
    
    protected override void UpdateMoving()
    {
        var (x, y) = PlayerSide switch
        {
            EPlayerSide.Blue => (0, -1),
            EPlayerSide.Red  => (0, 1),
            _ => throw new ArgumentOutOfRangeException()
        };
        IncPos(x, y);
    }
    
    protected override void UpdateChase()
    {
        
    }

    
}
    