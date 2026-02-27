using System;
using System.Collections.Generic;
using System.IO;

using LLSA.Log;

namespace LLSA.Config;

public static class ConfigManager
{
    public const string LAST_VERSION = "1.0.5";

    public static readonly Dictionary<string, string> DEFAULT = new Dictionary<string, string>() {
        { "version", "1.0.5" },
        { "language", "English" },
        { "StartDate", $"{DateTime.Now}" },
        { "LastLoginDate", $"{DateTime.Now}" },
        { "OverallSecondCount", "0" },
        { "TodaySecondCount", "0" },
        { "PenaltySecondCount", "0" },
        { "WatchedVideoCount", "0" },
        { "PlanWatchHourAmount", "100" },
        { "PlanSpeakHourAmount", "100" },
        { "PlanQuotaAmount", "1" },
        { "QuotaBonusProcent", "25" },
        { "OverallLanguageUseDays", "0" },
        { "OverallLanguageUseSecondCount", "0" },
        { "TodayLanguageUseSecondCount", "0" },
        { "BonusRestriction", "10" }
    };
    public static string ConfigPath { get; } = Path.Combine(AppContext.BaseDirectory, "Data", "config.ini");

    public static Dictionary<string, string> ReadConfig(string configFilePath)
    {
        Dictionary<string, string> configValues = new Dictionary<string, string>();

        string content = "";
        using (StreamReader sr = new StreamReader(configFilePath))
            content = sr.ReadToEnd();

        string[] lines = content.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (string line in lines)
        {
            string[] pair = line.Split("=", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (pair.Length != 2)
            {
                LogManager.Log($"Config is corrupted!");

                break;
            }

            configValues.Add(pair[0], pair[1]);
        }

        return configValues;
    }

    public static void WriteConfig(string configFilePath, Dictionary<string, string> values)
    {
        using (StreamWriter sw = new StreamWriter(configFilePath))
            foreach (KeyValuePair<string, string> pair in values)
                sw.WriteLine($"{pair.Key} = {pair.Value}");
    }

    public static void WriteDefaultConfig(string configFilePath) => WriteConfig(configFilePath, DEFAULT);

    public static void Repair()
    {
        Dictionary<string, string> currentConfig = ReadConfig(ConfigPath);

        foreach (var pair in DEFAULT)
        {
            if (!currentConfig.ContainsKey(pair.Key))
                currentConfig.Add(pair.Key, pair.Value);
        }

        if (!currentConfig["version"].Equals(DEFAULT["version"]))
            currentConfig["version"] = DEFAULT["version"];

        WriteConfig(ConfigPath, currentConfig);
    }
}