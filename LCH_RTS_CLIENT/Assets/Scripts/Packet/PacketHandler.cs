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
        Debug.Log($"RoomId={packet.RoomId}, blueId = {packet.BluePlayerId}, redId = {packet.RedPlayerId}");
        SceneManager.LoadScene("PlayScene");
        var playerId = PlayerInfo.Instance.PlayerId;
        var playerSide = playerId == packet.BluePlayerId ? EPlayerSide.Blue : EPlayerSide.Red;

        var go = GameObject.Find("GameRoomScene");
        if (go != null)
        {
            var roomScene = go.GetComponent<GameRoomScene>();
            if (roomScene != null)
            {
                roomScene.Init(packet.RoomId, playerId, playerSide, 0, 10);
            }
        }
        Managers.Network.Send(PacketUtil.CS_UNIT_SPAWN_PACKET(packet.RoomId, 1, new Vector2(34.0f, 18f)));
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

        Debug.Log($"UnitId={packet.UnitId}, PlayerSide={playerSide}, UnitType={packet.UnitType}, (X,Y)=({pos.x},{pos.y}), Range={stat.Range}");
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
        var go = Managers.Object.FindById(packet.VictimId);
        if (go == null)
        {
            Debug.LogError($"Victim={packet.VictimId} is null");
            return;
        }

        UnitBaseController uc = go.GetComponent<UnitBaseController>();
        uc.Stat.CurrHp = packet.RemainHp;
        uc.OnTakeDamage(packet.RemainHp);

        Debug.Log($"{packet.AttackerId} attacked {packet.VictimId} => {(float)uc.Stat.CurrHp / uc.Stat.MaxHp}");
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

        Managers.Object.Remove(packet.UnitId);
        Debug.Log($"Unit={packet.UnitId} remove Success");
    }
}