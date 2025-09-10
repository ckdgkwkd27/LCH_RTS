using System.Numerics;
using System.Runtime.Intrinsics.X86;
using Google.FlatBuffers;

public enum EPlayerSide
{
    Blue,
    Red,
    
    Max
}

namespace LCH_RTS.Contents.Units
{
    public enum EUnitStatus
    {
        Idle,
        Moving,
        Attack,
        Chase
    }
    
    public class UnitBase
    {
        public EPlayerSide PlayerSide { get; set; }
        public Vec2 Pos { get; set; }
        public UnitStat Stat {get; set;}
        public long UnitId { get; }
        public int UnitType { get; private set; }
        protected EUnitStatus Status { get; set; } = EUnitStatus.Idle;
        protected long RoomId { get; set; }
        private GameRoom? Room { get; set; }
        protected UnitBase? Target;

        protected UnitBase(long unitId, EPlayerSide playerSide, Vec2 pos, UnitStat stat, int unitType, long roomId)
        {
            UnitId = unitId;
            PlayerSide = playerSide;
            Pos = pos;
            Stat = stat;
            UnitType = unitType;
            RoomId = roomId;
            Room = GameRoomManager.Instance.GetRoom(roomId);
        }

        public void SetStatus(EUnitStatus status)
        {
            Status = status;
        }

        protected void IncPos(float x, float y)
        {
            var builder = new FlatBufferBuilder(1024);
            var newPosOffset = Vec2.CreateVec2(builder, Pos.X + x, Pos.Y + y);
            builder.Finish(newPosOffset.Value);
            var newPos = Vec2.GetRootAsVec2(builder.DataBuffer);
            Pos = newPos;
            Room?.Broadcast(PacketUtil.SC_UNIT_MOVE_PACKET(RoomId, UnitId, UnitType, Pos));
        }
        
        public virtual void CheckClosestUnit(List<UnitBase>? enemyUnits)
        {
            if (enemyUnits is null || enemyUnits.Count == 0)
            {
                return;
            }
        
            foreach (var unit in enemyUnits.Where(unit => UnitUtil.GetDistanceSquare(Pos, unit.Pos) < Stat.AttackRange * Stat.AttackRange))
            {
                Status = EUnitStatus.Attack;
                Target = unit;     
                return;
            }

            foreach (var unit in enemyUnits.Where(unit => UnitUtil.GetDistanceSquare(Pos, unit.Pos) < Stat.Sight * Stat.Sight))
            {
                Status = EUnitStatus.Chase;
                Target = unit;
                return;
            }

            Target = null;
        }
        
        public virtual void Update()
        {
        }

        protected virtual void UpdateIdle()
        {
            
        }

        protected virtual void UpdateMoving()
        {
            
        }
        
        protected virtual void UpdateChase()
        {
            
        }

        protected virtual void UpdateAttack()
        {
            if (Target is null)
            {
                Status = EUnitStatus.Moving;
                return;
            }

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
                Target.Stat = stat;
                return;
            }

            Target.OnDead(RoomId, PlayerSide, Target.PlayerSide);
            gameRoom.RemoveUnit(Target);
            Target = null;
            Status = EUnitStatus.Moving;
        }

        protected virtual void OnDead(long roomId, EPlayerSide winnerSide, EPlayerSide loserSide)
        {
            
        }
    }
}