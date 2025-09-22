using System;
using System.IO;

namespace LLSA.Log;

public static class LogManager
{
    private static string LogFilePath = Path.Combine(AppContext.BaseDirectory, @"Data\error.log");

    public static void Log(string message)
    {
        if (message == null)
            return;

        if (!File.Exists(LogFilePath))
            return;

        using (StreamWriter sw = new StreamWriter(LogFilePath, append: true))
            sw.WriteLine($"[{DateTime.Now}] {message}");
    }
}