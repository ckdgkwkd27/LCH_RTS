using System.Reflection.Metadata.Ecma335;
using LCH_RTS.Contents.Units;
using LCH_RTS.Job;

namespace LCH_RTS.Contents;
//#TODO: 별개의 매칭서버로 옮겨야 한다!

public static class PlayerSideHelper
{
    public static EPlayerSide GetOppositeSide(EPlayerSide side)
    {
        return side switch
        {
            EPlayerSide.Blue => EPlayerSide.Red,
            EPlayerSide.Red => EPlayerSide.Blue,
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }
}

public enum ERoomState
{
    Waiting,
    Start,
    End,
    
    Max
}

public class GameRoom: JobSerializer
{
    public long RoomId { get;}
    private Dictionary<EPlayerSide, List<Unit>> _playerUnit = new();
    private Dictionary<EPlayerSide, List<Tower>> _playerTower = new();
    private Dictionary<EPlayerSide, List<UnitBase>> _allUnits = new();
    private readonly Dictionary<EPlayerSide, int> _playerCost = new();
    private readonly Dictionary<Player, EPlayerSide> _players = new();
    private ERoomState _roomState = ERoomState.Waiting;
    public GameRoom(long roomId)
    {
        RoomId = roomId;
    }

    public void GameStart()
    {
        var sideTowerType = UnitUtil.GetUnitTypeFromName("SideTower");
        var kingTowerType = UnitUtil.GetUnitTypeFromName("KingTower");
        
        _playerTower = new Dictionary<EPlayerSide, List<Tower>> {
            [EPlayerSide.Blue] = [
                new Tower(IssueUnitId(), EPlayerSide.Blue, UnitUtil.CreateVec2(33.9f, 28.4f), UnitUtil.GetUnitStatConfig(sideTowerType), sideTowerType, RoomId, Tower.ETowerType.Side),
                new Tower(IssueUnitId(), EPlayerSide.Blue, UnitUtil.CreateVec2(-12.3f, 30.3f), UnitUtil.GetUnitStatConfig(sideTowerType), sideTowerType, RoomId, Tower.ETowerType.Side),
                new Tower(IssueUnitId(), EPlayerSide.Blue, UnitUtil.CreateVec2(12.6f, 34.0f), UnitUtil.GetUnitStatConfig(kingTowerType), kingTowerType, RoomId, Tower.ETowerType.King)
            ],
            [EPlayerSide.Red] = [
                new Tower(IssueUnitId(), EPlayerSide.Red, UnitUtil.CreateVec2(33.3f, -1.3f), UnitUtil.GetUnitStatConfig(sideTowerType), sideTowerType, RoomId, Tower.ETowerType.Side),
                new Tower(IssueUnitId(), EPlayerSide.Red, UnitUtil.CreateVec2(-14.0f, 0.9f), UnitUtil.GetUnitStatConfig(sideTowerType), sideTowerType, RoomId, Tower.ETowerType.Side),
                new Tower(IssueUnitId(), EPlayerSide.Red, UnitUtil.CreateVec2(11.0f, -5.0f), UnitUtil.GetUnitStatConfig(kingTowerType), kingTowerType, RoomId, Tower.ETowerType.King),
            ]
        };

        _allUnits = _playerTower.ToDictionary(pair => pair.Key, pair => pair.Value.Cast<UnitBase>().ToList());

        foreach (var (playerSide, towers) in _playerTower)
        {
            foreach (var tower in towers)
            {
                Broadcast(PacketUtil.SC_UNIT_SPAWN_PACKET(tower.UnitId, playerSide, tower.UnitType, tower.Pos, tower.Stat));
            }
        }
        
        _roomState = ERoomState.Start;
        
        PushAfter(1000, Update);
    }
    
    public void AddPlayer(Player player, EPlayerSide playerSide)
    {
        _players[player] = playerSide;

        //#TODO: Player Full일때는 Start로 전환하기
        if (_roomState == ERoomState.Waiting)
        {
            GameStart();
        }
    }

    public EPlayerSide GetPlayerSide(Player player)
    {
        return _players.GetValueOrDefault(player, EPlayerSide.Max);
    }

    public void AddUnit(long unitId, EPlayerSide playerSide, Vec2 pos, UnitStat unitStat, int unitType)
    {
        var unit = new Unit(unitId, playerSide, pos, unitStat, unitType, RoomId);
        
        _playerUnit = new Dictionary<EPlayerSide, List<Unit>> { [playerSide] = [unit] };
        _allUnits[playerSide].Add(unit);
    }

    public void RemoveUnit(UnitBase ub)
    {
        switch (ub)
        {
            case Tower tower:
                _playerTower[ub.PlayerSide].Remove(tower);
                break;
            case Unit unit:
                _playerUnit[ub.PlayerSide].Remove(unit);
                break;
        }
        
        _allUnits.Remove(ub.PlayerSide);
        
        Broadcast(PacketUtil.SC_REMOVE_UNIT_PACKET(RoomId, ub.UnitId));
    }
    
    private void Update()
    {
        //Players
        foreach (var key in _playerCost.Keys.ToList())
        {
            _playerCost[key]++;
        }
        
        //Units
        foreach(var (_, units) in _playerUnit)
        {
            foreach (var unit in units)
            {
                unit.Update();

                if(_allUnits.TryGetValue(PlayerSideHelper.GetOppositeSide(unit.PlayerSide), out var unitList))
                    unit.CheckClosestUnit(unitList);
            }
        }
       
        //Towers
		foreach(var (side, towers) in _playerTower)
        {
            var oppositeSide = PlayerSideHelper.GetOppositeSide(side);
            foreach (var tower in towers)
            {
                if(_playerUnit.TryGetValue(oppositeSide, out var units))
                    tower.CheckClosestUnit([..units]);
            }

            towers.ForEach(tower => tower.Update());
        }
        
        
        PushAfter(1000, Update);
    }

    private long _issuedUnitId = 0;
    public long IssueUnitId() => _issuedUnitId++;

    public void Broadcast(byte[] stream)
    {
        foreach (var player in _players.Keys.ToList())
        {
           player.Session?.Send(stream);
        }
    }

    public bool ContainUnit(UnitBase ub)
    {
        return _allUnits.TryGetValue(ub.PlayerSide, out var units) && units.Contains(ub);
    }
}