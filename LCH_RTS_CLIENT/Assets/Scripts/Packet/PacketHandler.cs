using Google.FlatBuffers;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

//FlatBuffer 데이터는 주소값을 참조하는 방식이기 때문에 값 타입으로 가져와야 메인쓰레드에서 해제로 인한 문제가 생기지 않는다.

public class PacketHandler
{
    public static void SC_LOGIN_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_LOGIN.GetRootAsSC_LOGIN(new ByteBuffer(buffer.Array, buffer.Offset));
        var playerId = packet.PlayerId;
        PlayerInfo.Instance.SetPlayerId(playerId);
    }

    public static void SC_ENTER_GAME_Handler(PacketSession session, ArraySegment<byte> buffer) 
    {
        var packet = SC_ENTER_GAME.GetRootAsSC_ENTER_GAME(new ByteBuffer(buffer.Array, buffer.Offset));
        Debug.Log($"RoomId={packet.RoomId}, blueId = {packet.BluePlayerId}, redId = {packet.RedPlayerId}, Cost={packet.CurrCost}");

        SceneManager.sceneLoaded += OnSceneLoaded;
        tempPacket = packet;

        SceneManager.LoadScene("PlayScene");
    }

    private static SC_ENTER_GAME tempPacket;
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (scene.name == "PlayScene")
        {
            var playerId = PlayerInfo.Instance.PlayerId; 
            Debug.Log($"[TEST] PlayerId={playerId}");
            var playerSide = playerId == tempPacket.BluePlayerId ? EPlayerSide.Blue : EPlayerSide.Red;

            var go = GameObject.Find("GameRoomScene");
            if (go != null)
            {
                var roomScene = go.GetComponent<GameRoomScene>();
                if (roomScene != null)
                {
                    roomScene.Init(tempPacket.RoomId, playerId, playerSide, tempPacket.CurrCost, 10);
                    PlayerInfo.Instance.SetRoomId(tempPacket.RoomId);
                }
            }
        }
    }

    public static void SC_UNIT_SPAWN_Handler(PacketSession session, ArraySegment<byte> buffer) 
    {
        var packet = SC_UNIT_SPAWN.GetRootAsSC_UNIT_SPAWN(new ByteBuffer(buffer.Array, buffer.Offset));
        if(packet.CreatePos is null)
        {
            Debug.LogError("SC_UNIT_SPAWN CreatePos is NULL");
        }

        if(packet.UnitStat is null)
        {
            Debug.LogError("SC_UNIT_SPAWN UnitStat is NULL");
        }

        var pos = new Vector2(packet.CreatePos.Value.X, packet.CreatePos.Value.Y);
        var stat = packet.UnitStat.Value;
        var playerSide = packet.PlayerSide;

        Debug.Log($"UnitId={packet.UnitId},PlayerSide={playerSide},UnitType={packet.UnitType},Pos=({pos.x},{pos.y}),Range={stat.AttackRange},Sight={stat.Sight}");
        Managers.Object.Add(packet.UnitId, (EPlayerSide)playerSide, packet.UnitType, pos, BaseStat.ConvertFrom(stat));
    }

    public static void SC_UNIT_MOVE_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_UNIT_MOVE.GetRootAsSC_UNIT_MOVE(new ByteBuffer(buffer.Array, buffer.Offset));
        var go = Managers.Object.FindById(packet.UnitId);
        if (go != null)
        {
            go.transform.position = new Vector3(packet.NowPos.Value.X, packet.NowPos.Value.Y, go.transform.position.z);
        }
        else
        {
            Debug.LogError($"UnitId={packet.UnitId} is NULL!");
        }
    }

    public static void SC_UNIT_ATTACK_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_UNIT_ATTACK.GetRootAsSC_UNIT_ATTACK(new ByteBuffer(buffer.Array, buffer.Offset));
        var victimObject = Managers.Object.FindById(packet.VictimId);
        if (victimObject is null)
        {
            Debug.LogError($"Victim={packet.VictimId} is null");
            return;
        }

        var attackerObject = Managers.Object.FindById(packet.AttackerId);
        if(attackerObject is null)
        {
            Debug.LogError($"Attacker={packet.AttackerId} is null");
            return;
        }

        UnitBaseController attackerUc = attackerObject.GetComponent<UnitBaseController>();
        UnitBaseController victimUc = victimObject.GetComponent<UnitBaseController>();

        attackerUc.OnAttack(victimUc);

        victimUc.Stat.CurrHp = packet.RemainHp;
        victimUc.OnTakeDamage(packet.RemainHp);

        //Debug.Log($"{packet.AttackerId} attacked {packet.VictimId} => {(float)victimUc.Stat.CurrHp / victimUc.Stat.MaxHp}");
    }

    public static void SC_REMOVE_UNIT_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_REMOVE_UNIT.GetRootAsSC_REMOVE_UNIT(new ByteBuffer(buffer.Array, buffer.Offset));
        var go = Managers.Object.FindById(packet.UnitId);
        if (go == null)
        {
            Debug.LogError($"Unit={packet.UnitId} does not exist");
            return;
        }

        UnitBaseController ubc = go.GetComponent<UnitBaseController>();
        if (ubc != null)
        {
            ubc.OnRemove();
        }

        Managers.Object.Remove(packet.UnitId);
        Debug.Log($"Unit={packet.UnitId} remove Success");
    }

    public static void SC_PLAYER_COST_UPDATE_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_PLAYER_COST_UPDATE.GetRootAsSC_PLAYER_COST_UPDATE(new ByteBuffer(buffer.Array, buffer.Offset));
        var go = GameObject.Find("player");
        if (go == null)
        {
            Debug.LogError($"CostUpdate Player is null");
            return;
        }

        var pc = go.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError($"CostUpdate PC is null");
            return;
        }

        pc.SetCost(packet.RemainCost);
    }
}