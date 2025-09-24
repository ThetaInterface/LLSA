using System;
using System.Collections.Generic;
using System.IO;

using LLSA.Log;

namespace LLSA.Locale;

public static class LocaleManager
{
    public static Dictionary<string, Dictionary<string, string>> Locales = GetLocaleContents(Path.Combine(AppContext.BaseDirectory, "Data", "Locale"));

    public static string FormatLine(string line, params string[] variables)
    {
        foreach (string variable in variables)
            if (line.Contains("*"))
                line = ReplaceOnce(line, "*", variable);
            else
            {
                LogManager.Log("Not enough places to replace! TR_1");

                break;
            }

        return line;
    }

    private static string ReplaceOnce(string input, string search, string replacement)
    {
        int pos = input.IndexOf(search);

        if (pos < 1)
        {
            LogManager.Log("Not enough places to replace! TR_2");

            return input;
        }

        return input.Substring(0, pos) + replacement + input.Substring(pos + search.Length);
    }

    private static Dictionary<string, Dictionary<string, string>> GetLocaleContents(string localeFolderpath)
    {
        Dictionary<string, Dictionary<string, string>> localeFilePaths = [];

        string[] paths = Directory.GetFiles(localeFolderpath);

        foreach (string path in paths)
        {
            var localeContent = GetLocaleContent(path);

            localeFilePaths.Add(localeContent["locale_name"], localeContent);
        }

        return localeFilePaths;
    }

    private static Dictionary<string, string> GetLocaleContent(string localePath)
    {
        Dictionary<string, string> localeContent = [];

        string fileContent = "";
        using (StreamReader sr = new StreamReader(localePath))
            fileContent = sr.ReadToEnd();

        string[] lines = fileContent.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (string line in lines)
        {
            string[] pair = line.Split("=", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (pair.Length != 2)
            {
                LogManager.Log($"Locale {localePath} is corrupted!");

                continue;
            }

            localeContent.Add(pair[0], pair[1]);
        }

        return localeContent;
    }
}