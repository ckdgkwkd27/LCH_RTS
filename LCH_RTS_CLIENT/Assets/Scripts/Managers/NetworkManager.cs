using System.Collections.Generic;
using System.Net;
using System;
using UnityEditor;

public class NetworkManager
{
    ServerSession _session;

    public void Init()
    {
    }

    public void SetSession(ServerSession session)
    {
        _session = session;

        EditorApplication.playModeStateChanged += (state) =>
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                _session.Disconnect();
            }
        };
    }

    public void Disconnect()
    {
        if(!_session._disconnected)
            _session.Disconnect(); 
    }

    public void Send(byte[] stream)
    {
        _session.Send(stream);
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
