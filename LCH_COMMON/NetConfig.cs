using LCH_COMMON;

namespace LCH_COMMON;
public enum EPortInfo
{
    MATCHING_CLIENT_PORT = 8001,
    MATCHING_GAMESERVER_PORT = 8002,
    GAMESERVER_CLIENT_PORT = 8003,
}

public static class NetConfig
{
    public static string Ip { get; } = "127.0.0.1";

    public static int GetPort(EPortInfo port)
    {
        return Convert.ToInt32(port);
    }
}