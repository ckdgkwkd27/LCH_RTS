
public class NetworkManager
{
    ServerSession _matchSession;
    GameSession _gameSession;

    public void Init()
    {
    }

    public void SetSession(PacketSession session)
    {
        switch (session)
        {
            case ServerSession:
                SetMatchSession(session as ServerSession);
                break;
            case GameSession:
                SetGameSession(session as GameSession);
                break;
            default:
                throw new System.Exception();
        }
    }

    public void SetMatchSession(ServerSession session)
    {
        _matchSession = session;
    }

    public void SetGameSession(GameSession session)
    {
        _gameSession = session;
    }

    public void Disconnect()
    {
        if(!_matchSession._disconnected)    _matchSession.Disconnect(); 
        if(!_gameSession._disconnected)     _gameSession.Disconnect();
    }

    public void SendToMatch(byte[] stream)
    {
        _matchSession.Send(stream);
    }

    public void SendToGame(byte[] stream)
    {
        _gameSession.Send(stream);
    }

    public void DisconnectGameSession()
    {
        _gameSession?.Disconnect();
        _gameSession = null;
    }

    public void Update()
    {
        var datas = PacketDispatcher.Instance.PopAll();
        foreach (var data in datas)
        {
            data.Item1.Invoke(data.Item2, data.Item3, data.Item4);
        }
    }
}
