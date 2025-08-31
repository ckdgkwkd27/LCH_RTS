using System.Numerics;
using System.Linq;
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
            EPlayerSide.Max => throw new Exception(),
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }
}

public enum ERoomState
{
    Waiting,
    PreStart,
    Start,
    End,
    Reset,
    
    Max
}

public class GameRoom: JobSerializer
{
    public long RoomId { get;}
    private readonly List<long> _tempPlayerIds = [];
    private readonly Dictionary<EPlayerSide, List<Unit>> _playerUnit = new();
    private Dictionary<EPlayerSide, List<Tower>> _playerTower = new();
    private Dictionary<EPlayerSide, List<UnitBase>> _allUnits = new();

    private readonly long[] _playerSideIds = new long[(int)(EPlayerSide.Max)];
    private readonly Dictionary<EPlayerSide, PlayerInGameInfo> _playerInGameInfos = new();
    
    private List<WayPoint> _wayPoints = [];
    private ERoomState _roomState = ERoomState.Waiting;
    private long _gameStartTime = 0;
    
    public GameRoom(long roomId)
    {
        RoomId = roomId;
    }

    private void GameStart()
    {
        _gameStartTime = 0;
        
        var sideTowerType = UnitUtil.GetUnitTypeFromName("SideTower");
        var kingTowerType = UnitUtil.GetUnitTypeFromName("KingTower");
        
        _playerTower = new Dictionary<EPlayerSide, List<Tower>> {
            [EPlayerSide.Blue] = [
                new Tower(IssueUnitId(), EPlayerSide.Blue, UnitUtil.CreateVec2(34f, 29.4f), UnitUtil.GetUnitStatConfig(sideTowerType), sideTowerType, RoomId, Tower.ETowerType.Side),
                new Tower(IssueUnitId(), EPlayerSide.Blue, UnitUtil.CreateVec2(-13.5f, 29.4f), UnitUtil.GetUnitStatConfig(sideTowerType), sideTowerType, RoomId, Tower.ETowerType.Side),
                new Tower(IssueUnitId(), EPlayerSide.Blue, UnitUtil.CreateVec2(9.9f, 34.8f), UnitUtil.GetUnitStatConfig(kingTowerType), kingTowerType, RoomId, Tower.ETowerType.King)
            ],
            [EPlayerSide.Red] = [
                new Tower(IssueUnitId(), EPlayerSide.Red, UnitUtil.CreateVec2(34f, 0f), UnitUtil.GetUnitStatConfig(sideTowerType), sideTowerType, RoomId, Tower.ETowerType.Side),
                new Tower(IssueUnitId(), EPlayerSide.Red, UnitUtil.CreateVec2(-13.5f, 0f), UnitUtil.GetUnitStatConfig(sideTowerType), sideTowerType, RoomId, Tower.ETowerType.Side),
                new Tower(IssueUnitId(), EPlayerSide.Red, UnitUtil.CreateVec2(9.9f, -5.0f), UnitUtil.GetUnitStatConfig(kingTowerType), kingTowerType, RoomId, Tower.ETowerType.King),
            ]
        };

        _allUnits = _playerTower.ToDictionary(pair => pair.Key, pair => pair.Value.Cast<UnitBase>().ToList());

        _wayPoints =
        [
            new WayPoint(new Vector2(31.8f, 10.2f), 0),
            new WayPoint(new Vector2(-12.0f, 12.25f), 0),
            
            new WayPoint(new Vector2(34f, 29.4f), 1),
            new WayPoint(new Vector2(-13.5f, 29.4f), 1),
            new WayPoint(new Vector2(34f, 0f), 1),
            new WayPoint(new Vector2(-13.5f, 0f), 1),
            
            new WayPoint(new Vector2(9.9f, 34.8f), 2),
            new WayPoint(new Vector2(9.9f, -5.0f), 2)
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
        
        SetRoomState(ERoomState.Start);
        // PushAfter(1000, Update); // 중복 제거: Update는 GameReady()에서 이미 스케줄링됨
    }

    // ReSharper disable once InconsistentNaming
    private const int INVALID_PLAYER_ID_VALUE = 0;
    public void AddPlayer(Player player, PlayerDeck deck, List<Card> hand)
    {
        // if (!_tempPlayerIds.Contains(player.PlayerId))
        // {
        //     return EPlayerSide.Max;
        // }
        
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
            //return (EPlayerSide)i;
        }

        Console.WriteLine($"[WARNING] Player {player.PlayerId} could not be added to the game");
        //return EPlayerSide.Max;
    }

    public void AddPlayers(long playerId1, long playerId2)
    {
        _tempPlayerIds.Add(playerId1);
        _tempPlayerIds.Add(playerId2);
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

        if (_roomState != ERoomState.Waiting) 
            return;
        
        SetRoomState(ERoomState.PreStart);
        _gameStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3;
        PushAfter(1000, Update); // Update 스케줄링은 여기서만 한 번만 수행
        
        Console.WriteLine($"Game will start at {_gameStartTime}");
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
    private void Update()
    {
        if (_roomState == ERoomState.End)
        {
            return;
        }
        
        //Players
        foreach (var (_, p) in _playerInGameInfos)
        {
            p.RemainCost = Math.Clamp(p.RemainCost + 1, 0, MaxPlayerCost);
            p.Player.Session?.Send(PacketUtil.SC_PLAYER_COST_UPDATE_PACKET(RoomId, p.RemainCost));
        }
        
        //Units
        foreach(var (_, units) in _playerUnit)
        {
            var unitsCopy = units.ToList();
            foreach (var unit in unitsCopy)
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
            var towersCopy = towers.ToList();
            foreach (var tower in towersCopy)
            {
                if(_playerUnit.TryGetValue(oppositeSide, out var units))
                    tower.CheckClosestUnit([..units]);
            }

            towersCopy.ForEach(tower => tower.Update());
        }

        if (_roomState == ERoomState.PreStart)
        {
            var utcNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (utcNow > _gameStartTime)
            {
                GameStart();
                Console.WriteLine($"Game Start roomId={RoomId}");
            }
        }

        PushAfter(1000, Update);
    }

    private long _issuedUnitId;

    public long IssueUnitId() => _issuedUnitId++;

    public void Broadcast(byte[] stream)
    {
        _playerInGameInfos.Values.ToList().ForEach(p => p.Player.Session?.Send(stream));
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
        _roomState = state;
    }

    public ERoomState GetRoomState()
    {
        return _roomState;
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

    public List<Card> GetNextHands(EPlayerSide playerSide)
    {
        var playerInfo = _playerInGameInfos[playerSide];
        var deck = playerInfo.Deck;
        var hands = playerInfo.Hand;
        
        var nextHands = new List<Card>(hands);
        CardUtil.DrawCard(deck, nextHands);
        return nextHands;
    }
    
    public bool HasCardInHand(EPlayerSide playerSide, int unitType)
    {
        var playerInfo = _playerInGameInfos[playerSide];
        var hands = playerInfo.Hand;
        return hands.Any(card => card.UnitType == unitType);
    }
    
    private void Reset()
    {
        _tempPlayerIds.Clear();
        _playerUnit.Clear();
        _playerTower.Clear();
        _allUnits.Clear();
        _playerInGameInfos.Clear();
        _wayPoints.Clear();
        Array.Fill(_playerSideIds, INVALID_PLAYER_ID_VALUE);
        _roomState = ERoomState.Waiting;
        _gameStartTime = 0;
        _issuedUnitId = 0;
        
        Console.WriteLine($"GameRoom {RoomId} has been reset");
    }
}