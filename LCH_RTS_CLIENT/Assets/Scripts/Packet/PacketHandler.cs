using Google.FlatBuffers;
using System;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PacketHandler
{
    public static void SC_LOGIN_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_LOGIN.GetRootAsSC_LOGIN(new ByteBuffer(buffer.Array, buffer.Offset));
        var roomId = packet.RoomId;
        session.Send(PacketUtil.CS_ENTER_GAME_Packet(Util.PlayerId, roomId));
    }

    public static void SC_ENTER_GAME_Handler(PacketSession session, ArraySegment<byte> buffer) 
    {
        var packet = SC_ENTER_GAME.GetRootAsSC_ENTER_GAME(new ByteBuffer(buffer.Array, buffer.Offset));
        SceneManager.sceneLoaded += OnSceneLoaded;
        tempPacket = packet;

        gs = session as GameSession;
        SceneManager.LoadScene("PlayScene");
    }

    private static SC_ENTER_GAME tempPacket;
    private static GameSession gs;
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (scene.name == "PlayScene")
        {
            var playerId = tempPacket.PlayerId;
            var playerSide = (EPlayerSide)tempPacket.PlayerSide;

            var go = GameObject.Find("GameRoomScene");
            if (go != null)
            {
                var roomScene = go.GetComponent<GameRoomScene>();
                if (roomScene != null)
                {
                    roomScene.Init(tempPacket.RoomId, playerId, playerSide, tempPacket.CurrCost, 10, 
                        Util.ConvertCardInfosToCards(tempPacket.PlayerHands, tempPacket.PlayerHandsLength), gs);
                }
            }
        }
        gs = null;
    }

    public static void SC_START_GAME_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_START_GAME.GetRootAsSC_START_GAME(new ByteBuffer(buffer.Array, buffer.Offset));
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

    public static void SC_END_GAME_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_END_GAME.GetRootAsSC_END_GAME(new ByteBuffer(buffer.Array, buffer.Offset));

        var ss = session as GameSession;
        var pc = ss.PlayerController;
        if (pc == null)
            return;

        var playerId = pc.PlayerId;
        var playerSide = pc.PlayerSide;

        if (packet.WinnerPlayerSide == (sbyte)playerSide)
        {
            pc.SetResultImage(true);
            Debug.Log($"VICTORY!");
        }
        else
        {
            pc.SetResultImage(false);
            Debug.Log($"Lose!");
        }

        Managers.Network.DisconnectGameSession();
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

    public static void SC_PLAYER_HAND_UPDATE_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = SC_PLAYER_HAND_UPDATE.GetRootAsSC_PLAYER_HAND_UPDATE(new ByteBuffer(buffer.Array, buffer.Offset));
        var go = GameObject.Find("player");
        if (go == null)
        {
            Debug.LogError($"HandUpdate Player is null");
            return;
        }

        var pc = go.GetComponent <PlayerController>();
        if(pc == null)
        {
            Debug.LogError($"HandUpdate PC is null");
            return;
        }

        pc.SetHands(Util.ConvertCardInfosToCards(packet.PlayerHands, packet.PlayerHandsLength));
    }

    public static void MC_MATCH_JOIN_INFO_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = MC_MATCH_JOIN_INFO.GetRootAsMC_MATCH_JOIN_INFO(new ByteBuffer(buffer.Array, buffer.Offset));
        Debug.Log($"IP={packet.Ip}, Port={packet.Port}");
        Util.MatchId = packet.MatchId;
        var endPoint = new IPEndPoint(IPAddress.Parse(packet.Ip), packet.Port);
        Connector connector = new Connector();
        connector.Connect(endPoint, () => new GameSession());
    }

    public static void MC_PLAYER_REGISTERED_Handler(PacketSession session, ArraySegment<byte> buffer)
    {
        var packet = MC_PLAYER_REGISTERED.GetRootAsMC_PLAYER_REGISTERED(new ByteBuffer(buffer.Array, buffer.Offset));
        var playerId = packet.PlayerId;
        var ss = session as ServerSession;
        Util.PlayerId = ss.PlayerId = playerId;
        Debug.Log($"PlayerRegistered PlayerId={ss.PlayerId}");
    }
}