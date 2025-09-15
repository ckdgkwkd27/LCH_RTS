namespace LCH_RTS_BOT_TEST;
using System;
using System.IO;

public static class Logger
{
    private static readonly Lock Lock = new();
    private static readonly string LogFilePath = $"log.txt";

    public static void Log(string message)
    {
        using (Lock.EnterScope())
        {
            try
            {
                var logMessage = $"{DateTime.Now:yyyy-MM-dd_HH_mm_ss} - {message}";
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log Write Error: {ex.Message}");
            }
        }
    }
}
