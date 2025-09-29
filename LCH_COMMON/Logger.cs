namespace LCH_COMMON;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

public enum ELogType
{
    Console,
    File
}

public enum ELogLevel
{
    Info,
    Debug,
    Warning,
    Error
}

public static class Logger
{
    private static readonly string LogFilePath = "log.txt";
    private static readonly ConcurrentQueue<(ELogType, ELogLevel, string)> LogQueue = new();
    private static readonly AutoResetEvent Event = new(false);

    public static void Initialize()
    {
        var t = new Thread(LoggerThread)
        {
            Name = "Logger Thread",
            IsBackground = true
        };
        t.Start();
        Log(ELogType.Console, ELogLevel.Info, "Logger Initialized!");
    }

    public static void Log(ELogType logType, ELogLevel logLevel, string message)
    {
        LogQueue.Enqueue((logType, logLevel, message));
        Event.Set();
    }

    private static void LoggerThread()
    {
        while (true)
        {
            Event.WaitOne();
            PrintLogs();
        }
    }

    private static void PrintLogs()
    {
        while (TryDequeue(out var element))
        {
            FlushLogs(element.LogType, element.LogLevel, element.Message);
        }
    }

    private static bool TryDequeue(out (ELogType LogType, ELogLevel LogLevel, string Message) element)
    {
        return LogQueue.TryDequeue(out element);
    }

    private static void FlushLogs(ELogType logType, ELogLevel logLevel, string message)
    {
        switch (logType)
        {
            case ELogType.Console:
                Console.WriteLine($"[{logLevel}] {message}");
                break;
            case ELogType.File:
                try
                {
                    var logMessage = $"{DateTime.Now:yyyy-MM-dd_HH_mm_ss} - [{logLevel}] {message}";
                    File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Log Write Error: {ex.Message}");
                }
                break;
            default: throw new ArgumentOutOfRangeException(nameof(logType), logType, null);
        }
    }
}
