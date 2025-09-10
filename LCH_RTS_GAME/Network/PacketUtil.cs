using Google.FlatBuffers;
using LCH_RTS.Contents;

namespace LCH_RTS;

public static class PacketUtil
{
    public static byte[] SC_LOGIN_PACKET(long roomId)
    {
        var builder = new FlatBufferBuilder(1024);
        
        SC_LOGIN.StartSC_LOGIN(builder);
        SC_LOGIN.AddRoomId(builder, roomId);
        var offset = SC_LOGIN.EndSC_LOGIN(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();
        
        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.SC_LOGIN), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }
    
    public static byte[] SC_ENTER_GAME_PACKET(long roomId, long playerId, byte playerSide, int currCost, CardInfo[] cardInfos)
    {
        var builder = new FlatBufferBuilder(1024);

        var cardInfoOffsets = new Offset<CardInfo>[cardInfos.Length];
        for(var idx = 0; idx < cardInfos.Length; idx++)
        {
            var cardInfo = cardInfos[idx];
            var cardNameOffset = builder.CreateString(cardInfo.Name);
            cardInfoOffsets[idx] = CardInfo.CreateCardInfo(builder, cardInfo.UnitType, cardInfo.Cost, cardNameOffset);
        }

        var playerHandOffset = SC_ENTER_GAME.CreatePlayerHandsVector(builder, cardInfoOffsets);
        
        SC_ENTER_GAME.StartSC_ENTER_GAME(builder);
        SC_ENTER_GAME.AddRoomId(builder, roomId);
        SC_ENTER_GAME.AddPlayerId(builder, playerId);
        SC_ENTER_GAME.AddPlayerSide(builder, playerSide);
        SC_ENTER_GAME.AddCurrCost(builder, currCost);
        SC_ENTER_GAME.AddPlayerHands(builder, playerHandOffset);
        var offset = SC_ENTER_GAME.EndSC_ENTER_GAME(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();
        
        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.SC_ENTER_GAME), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }
    
    public static byte[] SC_START_GAME_PACKET()
    {
        var builder = new FlatBufferBuilder(1024);
        
        SC_START_GAME.StartSC_START_GAME(builder);
        var offset = SC_START_GAME.EndSC_START_GAME(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();
        
        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.SC_START_GAME), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] SC_UNIT_SPAWN_PACKET(long unitId, EPlayerSide playerSide, int unitType, Vec2 pos, UnitStat stat)
    {
        var builder = new FlatBufferBuilder(1024);

        var vecOffset = Vec2.CreateVec2(builder, pos.X, pos.Y);
        var statOffset = UnitStat.CreateUnitStat(builder, stat.Attack, stat.MaxHp, stat.CurrHp, stat.Speed, stat.Cost, stat.AttackRange, stat.Sight);

        SC_UNIT_SPAWN.StartSC_UNIT_SPAWN(builder);
        SC_UNIT_SPAWN.AddUnitId(builder, unitId);
        SC_UNIT_SPAWN.AddPlayerSide(builder, (sbyte)playerSide);
        SC_UNIT_SPAWN.AddUnitType(builder, unitType);
        SC_UNIT_SPAWN.AddCreatePos(builder, vecOffset);
        SC_UNIT_SPAWN.AddUnitStat(builder, statOffset);
        
        var offset = SC_UNIT_SPAWN.EndSC_UNIT_SPAWN(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();

        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.SC_UNIT_SPAWN), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] SC_UNIT_MOVE_PACKET(long roomId, long unitId, int unitType, Vec2 pos)
    {
        var builder = new FlatBufferBuilder(1024);
        
        var posOffset = Vec2.CreateVec2(builder, pos.X, pos.Y);

        SC_UNIT_MOVE.StartSC_UNIT_MOVE(builder);
        SC_UNIT_MOVE.AddRoomId(builder, roomId);
        SC_UNIT_MOVE.AddUnitId(builder, unitId);
        SC_UNIT_MOVE.AddUnitType(builder, unitType);

        SC_UNIT_MOVE.AddNowPos(builder, posOffset);
        
        var offset = SC_UNIT_MOVE.EndSC_UNIT_MOVE(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();
        
        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.SC_UNIT_MOVE), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] SC_UNIT_ATTACK_PACKET(long roomId, long attackerId, long victimId, int remainHp)
    {
        var builder = new FlatBufferBuilder(1024);
        
        SC_UNIT_ATTACK.StartSC_UNIT_ATTACK(builder);
        SC_UNIT_ATTACK.AddRoomId(builder, roomId);
        SC_UNIT_ATTACK.AddAttackerId(builder, attackerId);
        SC_UNIT_ATTACK.AddVictimId(builder, victimId);
        SC_UNIT_ATTACK.AddRemainHp(builder, remainHp);
        
        var offset = SC_UNIT_ATTACK.EndSC_UNIT_ATTACK(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();
        
        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.SC_UNIT_ATTACK), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] SC_REMOVE_UNIT_PACKET(long roomId, long unitId)
    {
        var builder = new FlatBufferBuilder(1024);

        SC_REMOVE_UNIT.StartSC_REMOVE_UNIT(builder);
        SC_REMOVE_UNIT.AddRoomId(builder, roomId);
        SC_REMOVE_UNIT.AddUnitId(builder, unitId);
        
        var offset = SC_REMOVE_UNIT.EndSC_REMOVE_UNIT(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();
        
        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.SC_REMOVE_UNIT), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] SC_END_GAME_PACKET(long roomId, sbyte winnerPlayerSide, sbyte loserPlayerSide)
    {
        var builder = new FlatBufferBuilder(1024);

        SC_END_GAME.StartSC_END_GAME(builder);
        SC_END_GAME.AddRoomId(builder, roomId);
        SC_END_GAME.AddWinnerPlayerSide(builder, winnerPlayerSide);
        SC_END_GAME.AddLoserPlayerSide(builder, loserPlayerSide);

        var offset = SC_END_GAME.EndSC_END_GAME(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();

        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.SC_END_GAME), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] SC_PLAYER_COST_UPDATE_PACKET(long roomId, int cost)
    {
        var builder = new FlatBufferBuilder(1024);
        
        SC_PLAYER_COST_UPDATE.StartSC_PLAYER_COST_UPDATE(builder);
        SC_PLAYER_COST_UPDATE.AddRoomId(builder, roomId);
        SC_PLAYER_COST_UPDATE.AddRemainCost(builder, cost);
        
        var offset =  SC_PLAYER_COST_UPDATE.EndSC_PLAYER_COST_UPDATE(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();
        
        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.SC_PLAYER_COST_UPDATE), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }

    public static byte[] SC_PLAYER_HAND_UPDATE_PACKET(long roomId, long playerId, List<CardInfo> playerHands)
    {
        var builder = new FlatBufferBuilder(1024);

        List<Offset<CardInfo>> tempHands = [];
        tempHands.AddRange(from cardInfo in playerHands let cardName = builder.CreateString(cardInfo.Name) select CardInfo.CreateCardInfo(builder, cardInfo.UnitType, cardInfo.Cost, cardName));
        var cards = SC_PLAYER_HAND_UPDATE.CreatePlayerHandsVector(builder, tempHands.ToArray());

        SC_PLAYER_HAND_UPDATE.StartSC_PLAYER_HAND_UPDATE(builder);
        SC_PLAYER_HAND_UPDATE.AddRoomId(builder, roomId);
        SC_PLAYER_HAND_UPDATE.AddPlayerId(builder, playerId);
        SC_PLAYER_HAND_UPDATE.AddPlayerHands(builder, cards);

        var offset = SC_PLAYER_HAND_UPDATE.EndSC_PLAYER_HAND_UPDATE(builder);
        builder.Finish(offset.Value);
        var bodyArr = builder.SizedByteArray();

        var stream = new byte[bodyArr.Length + 4];
        Array.Copy(BitConverter.GetBytes((ushort)stream.Length), 0, stream, 0, 2);
        Array.Copy(BitConverter.GetBytes((ushort)PACKET_ID.SC_PLAYER_HAND_UPDATE), 0, stream, 2, 2);
        Array.Copy(bodyArr, 0, stream, 4, bodyArr.Length);
        return stream;
    }
}