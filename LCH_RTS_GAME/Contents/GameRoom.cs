using System.Diagnostics;
using System.Numerics;
using System.Linq;
using LCH_RTS.Contents.Units;
using LCH_RTS.Job;
namespace LCH_RTS.Contents;

public static class PlayerSideHelper
{
    public static EPlayerSide GetOppositeSide(EPlayerSide side)
    {
        return side switch
        {
            EPlayerSide.Blue => EPlayerSide.Red,
            EPlayerSide.Red => EPlayerSide.Blue,
            EPlayerSide.Max => throw new Exception(),
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }
}

public class GameRoom: JobSerializer
{
    public long RoomId { get;}
    private readonly Dictionary<EPlayerSide, List<Unit>> _playerUnit = new();
    private Dictionary<EPlayerSide, List<Tower>> _playerTower = new();
    private Dictionary<EPlayerSide, List<UnitBase>> _allUnits = new();

    private readonly long[] _playerSideIds = new long[(int)(EPlayerSide.Max)];
    private readonly Dictionary<EPlayerSide, PlayerInGameInfo> _playerInGameInfos = new();
    
    private List<WayPoint> _wayPoints = [];
    private readonly GameRoomStatus _roomStatus;
    
    public GameRoom(long roomId)
    {
        RoomId = roomId;
        _roomStatus = new GameRoomStatus(this);
    }

    public void GameStart()
    {
        var sideTowerType = UnitUtil.GetUnitTypeFromName("SideTower");
        var kingTowerType = UnitUtil.GetUnitTypeFromName("KingTower");
        
        _playerTower = new Dictionary<EPlayerSide, List<Tower>> {
            [EPlayerSide.Blue] = [
                new Tower(IssueUnitId(), EPlayerSide.Blue, UnitUtil.CreateVec2(37f, 29.4f), UnitUtil.GetUnitStatConfig(sideTowerType), sideTowerType, RoomId, Tower.ETowerType.Side),
                new Tower(IssueUnitId(), EPlayerSide.Blue, UnitUtil.CreateVec2(-16.5f, 29.4f), UnitUtil.GetUnitStatConfig(sideTowerType), sideTowerType, RoomId, Tower.ETowerType.Side),
                new Tower(IssueUnitId(), EPlayerSide.Blue, UnitUtil.CreateVec2(9.9f, 34.8f), UnitUtil.GetUnitStatConfig(kingTowerType), kingTowerType, RoomId, Tower.ETowerType.King)
            ],
            [EPlayerSide.Red] = [
                new Tower(IssueUnitId(), EPlayerSide.Red, UnitUtil.CreateVec2(37f, 0f), UnitUtil.GetUnitStatConfig(sideTowerType), sideTowerType, RoomId, Tower.ETowerType.Side),
                new Tower(IssueUnitId(), EPlayerSide.Red, UnitUtil.CreateVec2(-16.5f, 0f), UnitUtil.GetUnitStatConfig(sideTowerType), sideTowerType, RoomId, Tower.ETowerType.Side),
                new Tower(IssueUnitId(), EPlayerSide.Red, UnitUtil.CreateVec2(9.9f, -5.0f), UnitUtil.GetUnitStatConfig(kingTowerType), kingTowerType, RoomId, Tower.ETowerType.King),
            ]
        };

        _allUnits = _playerTower.ToDictionary(pair => pair.Key, pair => pair.Value.Cast<UnitBase>().ToList());

        if (!_playerUnit.ContainsKey(EPlayerSide.Blue)) _playerUnit[EPlayerSide.Blue] = [];
        if (!_playerUnit.ContainsKey(EPlayerSide.Red)) _playerUnit[EPlayerSide.Red] = [];

        _wayPoints =
        [
            new WayPoint(new Vector2(31.8f, 10.2f), 0),
            new WayPoint(new Vector2(-12.0f, 12.25f), 0),
            
            new WayPoint(new Vector2(37f, 29.4f), 1),
            new WayPoint(new Vector2(-16.5f, 29.4f), 1),
            new WayPoint(new Vector2(37f, 0f), 1),
            new WayPoint(new Vector2(-16.5f, 0f), 1),
            
            new WayPoint(new Vector2(9.9f, 34.8f - 5.0f), 2),
            new WayPoint(new Vector2(9.9f, -5.0f + 5.0f), 2)
        ];

        foreach (var (playerSide, towers) in _playerTower)
        {
            foreach (var tower in towers)
            {
                Broadcast(PacketUtil.SC_UNIT_SPAWN_PACKET(tower.UnitId, playerSide, tower.UnitType, tower.Pos, tower.Stat));
                Console.WriteLine("[DEBUG] SC_UNIT_SPAWN_PACKET sent!");
            }
        }
        
        _playerInGameInfos.Values.ToList().ForEach(p =>
        {
            const int firstCost = 0;
            p.RemainCost = firstCost;
            PacketUtil.SC_PLAYER_COST_UPDATE_PACKET(RoomId, firstCost);
        });
        
        _roomStatus.ChangeState(ERoomState.Start);
    }

    // ReSharper disable once InconsistentNaming
    private const int INVALID_PLAYER_ID_VALUE = 0;
    public void AddPlayer(Player player, PlayerDeck deck, List<Card> hand)
    {
        for (var i = 0; i < _playerSideIds.Length; i++)
        {
            if (_playerSideIds[i] != INVALID_PLAYER_ID_VALUE) 
                continue;
            
            _playerSideIds[i] = player.PlayerId;

            var playerSide = (EPlayerSide)i;
            _playerInGameInfos[playerSide] = new PlayerInGameInfo(player, playerSide, RoomId, 0, deck, hand);
            player.Session?.Send(PacketUtil.SC_ENTER_GAME_PACKET(RoomId, GetPlayerId(playerSide), (byte)playerSide, 0,
                CardUtil.ConvertToCardInfos(deck.ShuffleAndTake(PlayerDeck.MAX_CARD_LIST)).ToArray()));
            Console.WriteLine($"Player {player.PlayerId}({playerSide}) added to the game");
            return;
        }

        Console.WriteLine($"[WARNING] Player {player.PlayerId} could not be added to the game");
    }

    public long GetPlayerId(EPlayerSide playerSide)
    {
        return _playerSideIds[(int)playerSide];
    }

    public void GameReady()
    {
        if (_playerSideIds.Any(id => id == INVALID_PLAYER_ID_VALUE))
        {
            Console.WriteLine($"[WARNING] Game is not ready for room {RoomId}");
            return;
        }

        if (!_roomStatus.IsInState(ERoomState.Waiting)) 
            return;
        
        _roomStatus.ChangeState(ERoomState.PreStart);
        PushAfter(1000, Update);
        
        Console.WriteLine($"Game will enter PreStart state for room {RoomId}");
    }

    public EPlayerSide GetPlayerSide(Player player)
    {
        return _playerInGameInfos.Values .FirstOrDefault(info => info.Player == player)?.PlayerSide ?? EPlayerSide.Max;
    }

    public void AddUnit(long unitId, EPlayerSide playerSide, Vec2 pos, UnitStat unitStat, int unitType)
    {
        var unit = new Unit(unitId, playerSide, pos, unitStat, unitType, RoomId);
        
        if (!_playerUnit.TryGetValue(playerSide, out var units)) 
        {
            units = [];
            _playerUnit[playerSide] = units;
        }

        units.Add(unit);
        _allUnits[playerSide].Add(unit);
    }

    public void RemoveUnit(UnitBase ub)
    {
        switch (ub)
        {
            case Tower tower:
                _wayPoints.RemoveAll(wp => wp.Pos == UnitUtil.GetNotSerializedVector(tower.Pos));
                _playerTower[ub.PlayerSide].Remove(tower);
                break;
            case Unit unit:
                _playerUnit[ub.PlayerSide].Remove(unit);
                break;
        }
        
        _allUnits[ub.PlayerSide].Remove(ub);
        Broadcast(PacketUtil.SC_REMOVE_UNIT_PACKET(RoomId, ub.UnitId));
    }

    private const int MaxPlayerCost = 10;
    
    public void UpdatePlayersCost()
    {
        foreach (var (_, p) in _playerInGameInfos)
        {
            p.RemainCost = Math.Clamp(p.RemainCost + 1, 0, MaxPlayerCost);
            p.Player.Session?.Send(PacketUtil.SC_PLAYER_COST_UPDATE_PACKET(RoomId, p.RemainCost));
        }
    }
    
    public void UpdateUnits()
    {
        if (_playerUnit.Count == 0)
            return;
        
        foreach(var (_, units) in _playerUnit)
        {
            var unitsCopy = units?.ToList();
            if (unitsCopy == null) 
                continue;
            
            foreach (var unit in unitsCopy)
            {
                unit.Update();

                if (_allUnits.TryGetValue(PlayerSideHelper.GetOppositeSide(unit.PlayerSide), out var unitList))
                    unit.CheckClosestUnit(unitList);
            }
        }
    }
    
    public void UpdateTowers()
    {
        foreach(var (side, towers) in _playerTower)
        {
            var oppositeSide = PlayerSideHelper.GetOppositeSide(side);
            var towersCopy = towers.ToList();
            foreach (var tower in towersCopy)
            {
                if(_playerUnit.TryGetValue(oppositeSide, out var units))
                    tower.CheckClosestUnit([..units]);
            }

            towersCopy.ForEach(tower => tower.Update());
        }
    }
    
    private void Update()
    {
        if (_roomStatus.IsInState(ERoomState.End))
        {
            return;
        }
        _roomStatus.Update();
        PushAfter(1000, Update);
    }

    private long _issuedUnitId;

    public long IssueUnitId() => _issuedUnitId++;

    public void Broadcast(byte[] stream)
    {
        foreach (var session in _playerInGameInfos.Values.Select(infos => infos.Player.Session))
        {
            Debug.Assert(session is not null);
            session.Send(stream);
        }
    }

    public bool ContainUnit(UnitBase ub)
    {
        return _allUnits.TryGetValue(ub.PlayerSide, out var units) && units.Contains(ub);
    }

    public List<WayPoint> GetWayPoints()
    {
        return _wayPoints;
    }

    public void SetRoomState(ERoomState state)
    {
        _roomStatus.ChangeState(state);
    }

    public ERoomState GetRoomState()
    {
        return _roomStatus.GetRoomStateEnum();
    }
    
    public int DecreaseCost(EPlayerSide playerSide, int unitStatCost)
    {
        _playerInGameInfos[playerSide].RemainCost = Math.Clamp(_playerInGameInfos[playerSide].RemainCost - unitStatCost, 0, MaxPlayerCost);
        return _playerInGameInfos[playerSide].RemainCost;
    }

    public int GetPlayerCost(EPlayerSide playerSide)
    {
        return _playerInGameInfos[playerSide].RemainCost;
    }

    public void HandsUpdate(EPlayerSide playerSide, long playerId, int unitType)
    {
        var playerInfo = _playerInGameInfos[playerSide];
        var deck = playerInfo.Deck;
        var hands = playerInfo.Hand;
        
        var cardToRemove = hands.FirstOrDefault(card => card.UnitType == unitType);
        if (cardToRemove != null)
        {
            hands.Remove(cardToRemove);
            CardUtil.DrawCard(deck, hands);
        }
        else
        {
            Console.WriteLine($"[WARNING] Card with UnitType {unitType} not found in player's hand");
        }

        var handsConverted = CardUtil.ConvertToCardInfos(playerInfo.Hand);
        _playerInGameInfos[playerSide].Player.Session?.Send(PacketUtil.SC_PLAYER_HAND_UPDATE_PACKET(RoomId, playerId, handsConverted));
    }

    public bool HasCardInHand(EPlayerSide playerSide, int unitType)
    {
        var playerInfo = _playerInGameInfos[playerSide];
        var hands = playerInfo.Hand;
        return hands.Any(card => card.UnitType == unitType);
    }
}