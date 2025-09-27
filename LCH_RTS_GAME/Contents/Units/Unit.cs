namespace LCH_RTS.Contents.Units;

public class Unit : UnitBase
{
    private Dictionary<EUnitStatus, Action> StatusActions { get;}
    private int _currentLevel = 0;

    public Unit(long unitId, EPlayerSide playerSide, Vec2 pos, UnitStat stat, int unitType, long roomId) : base(unitId, playerSide, pos, stat, unitType, roomId)
    {
        Status = EUnitStatus.Moving;
        StatusActions = new Dictionary<EUnitStatus, Action>
        {
            { EUnitStatus.Moving, UpdateMoving },
            { EUnitStatus.Attack, UpdateAttack },
            { EUnitStatus.Chase, UpdateChase }
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
        var room = GameRoomManager.Instance.GetRoom(RoomId);
        if (room is null)
            return;
        
        var wayPoints = room.GetWayPoints();
        if (wayPoints.Count == 0)
            return;
        
        var candidates = wayPoints.Where(wp => wp.Level == _currentLevel).ToList();
        if (candidates.Count == 0)
        {
            _currentLevel++;
            candidates = wayPoints.Where(wp => wp.Level == _currentLevel).ToList();
            if (candidates.Count == 0)
            {
                return;
            }
        }

        if (PlayerSide == EPlayerSide.Blue)
        {
            candidates = candidates.Where(wp => wp.Pos.Y < 10f).ToList();
        }
        else
        {
            candidates = candidates.Where(wp => wp.Pos.Y > 20f).ToList();
        }

        var filtered = PlayerSide == EPlayerSide.Blue ? candidates.Where(wp => wp.Pos.Y < Pos.Y).ToList() : candidates.Where(wp => wp.Pos.Y > Pos.Y).ToList();
        if (filtered.Count == 0)
        {
            _currentLevel++;
            return;
        }

        var targetWp = filtered.MinBy(wp => (wp.Pos.X - Pos.X) * (wp.Pos.X - Pos.X) + (wp.Pos.Y - Pos.Y) * (wp.Pos.Y - Pos.Y));
        var dirX = targetWp.Pos.X - Pos.X;
        var dirY = targetWp.Pos.Y - Pos.Y;
        var distSqr = dirX * dirX + dirY * dirY; 
        if (distSqr < 0.1f)
        {
            _currentLevel++;
            return;
        }
        
        var mag = MathF.Sqrt(dirX * dirX + dirY * dirY);
        if (mag > 0)
        {
            dirX /= mag;
            dirY /= mag;
        }
        IncPos(dirX * Stat.Speed, dirY * Stat.Speed);
    }

    protected override void UpdateChase()
    {
        if (Target is null)
        {
            Status = EUnitStatus.Moving;
            return;
        }
            
        var gameRoom = GameRoomManager.Instance.GetRoom(RoomId);
        if (gameRoom is null) return;
            
        var dirX = Target.Pos.X - Pos.X;
        var dirY = Target.Pos.Y - Pos.Y;
        var distSqr = dirX * dirX + dirY * dirY; 
        if (distSqr < Stat.AttackRange)
        {
            Status = EUnitStatus.Attack;
            return;
        }
            
        var mag = MathF.Sqrt(dirX * dirX + dirY * dirY);
        if (mag > 0)
        {
            dirX /= mag;
            dirY /= mag;
        }
        IncPos(dirX * Stat.Speed, dirY * Stat.Speed);

        {
            var waypoints = gameRoom.GetWayPoints();
            var candidates = waypoints.Where(wp => _currentLevel == wp.Level).ToList();
            if (candidates.Count == 0)
            {
                return;
            }

            var filtered = candidates.Where(wp => wp.Pos.Y < Pos.Y).ToList();
            if (filtered.Count == 0)
            {
                _currentLevel++;
                return;
            }

            var targetWp = filtered.MinBy(wp => (wp.Pos.X - Pos.X) * (wp.Pos.X - Pos.X) + (wp.Pos.Y - Pos.Y) * (wp.Pos.Y - Pos.Y));
            var wpDirX = targetWp.Pos.X - Pos.X;
            var wpDirY = targetWp.Pos.Y - Pos.Y;
            var wpDistSqr = wpDirX * wpDirX + wpDirY * wpDirY;
            if (wpDistSqr < 0.1f)
            {
                _currentLevel++;
            }
        }
    }
}