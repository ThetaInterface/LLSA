using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using Avalonia.VisualTree;
using System.Linq;

using LLSA.Log;
using LLSA.Locale;
using LLSA.Config;
using LLSA.Utilities;
using LLSA.TagManager;

namespace LLSA.Views;

public partial class MainWindow : Window
{
    private static readonly string[] UniqueTags =
    {
        "language"
    };

    public MainWindow()
    {
        InitializeComponent();

        AppDomain.CurrentDomain.UnhandledException += UnhandledException;

        if (!CheckFiles(AppContext.BaseDirectory, out string error))
        {
            LogManager.Log(error);

            Environment.Exit(0);
        }

        var language = ConfigManager.ReadConfig(ConfigManager.ConfigPath)["language"];

        UpdateLocale(language);

        MainMenuSetup(language);
    }

    private void SettingsSetup(string language)
    {
        var config = ConfigManager.ReadConfig(ConfigManager.ConfigPath);

        stackPanel_Settings.Children.Clear();

        foreach (var keyValue in config)
        {
            if (!LocaleManager.Locales[language].ContainsKey("settings_" + keyValue.Key))
                continue;

            var textBlock = new TextBlock()
            {
                Name = "settings_" + keyValue.Key,
                Text = LocaleManager.Locales[language]["settings_" + keyValue.Key],
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                Tag = new Tag(true)
            };

            var stackPanel = new StackPanel()
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 0),
            };

            if (UniqueTags.Contains(keyValue.Key))
            {
                switch (keyValue.Key)
                {
                    case "language":
                        var comboBox = new ComboBox()
                        {
                            Tag = new Tag(false, keyValue.Key, "settings")
                        };

                        foreach (var key in LocaleManager.Locales.Keys)
                            comboBox.Items.Add(key);

                        comboBox.SelectedValue = ConfigManager.ReadConfig(ConfigManager.ConfigPath)["language"];

                        stackPanel.Children.Add(comboBox);
                        stackPanel.Children.Add(textBlock);
                        break;
                }

                stackPanel_Settings.Children.Add(stackPanel);

                continue;
            }

            
            stackPanel.Children.Add(new TextBox()
            {
                Text = keyValue.Value,
                Tag = new Tag(false, keyValue.Key, "settings"),
                MaxLength = 5
            });

            stackPanel.Children.Add(textBlock);

            stackPanel_Settings.Children.Add(stackPanel);
        }

        Button saveButton = new Button()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 50, 0, 0)
        };

        saveButton.Click += SettingsSaveButton_Click;

        saveButton.Content = new TextBlock()
        {
            Name = "settings_SaveButton",
            Text = LocaleManager.Locales[language]["settings_SaveButton"],
            Tag = new Tag(true),
            Margin = new Thickness(50, 0, 50, 0)
        };

        stackPanel_Settings.Children.Add(saveButton);
    }

    private void MainMenuSetup(string language)
    {
        var config = ConfigManager.ReadConfig(ConfigManager.ConfigPath);

        stackPanel_MainMenu.Children.Clear();

        foreach (var keyValue in config)
        {
            if (!LocaleManager.Locales[language].ContainsKey("mainMenu_" + keyValue.Key))
                continue;

            var stackPanel = new StackPanel()
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 0),
            };

            stackPanel.Children.Add(new TextBox()
            {
                Tag = new Tag(false, keyValue.Key, "mainMenu"),
                MaxLength = 40
            });

            var textBlock = new TextBlock()
            {
                Name = "mainMenu_" + keyValue.Key,
                Text = LocaleManager.Locales[language]["mainMenu_" + keyValue.Key],
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                Tag = new Tag(true)
            };

            stackPanel.Children.Add(textBlock);
            stackPanel_MainMenu.Children.Add(stackPanel);
        }

        StackPanel invertTimeStackPanel = BuildStackPanelWithCheckBoxAndTextBlock("invertTime", "mainMenu_InvertedSecondCountCheckBox");
        StackPanel invertVideoCountStackPanel = BuildStackPanelWithCheckBoxAndTextBlock("invertVideoCount", "mainMenu_InvertedVideoCountCheckBox");
        StackPanel videoCountMultiplyStackPanel = BuildStackPanelWithCheckBoxAndTextBlock("multiplyVideoCount", "mainMenu_MultiplyVideoCountCheckBox");

        stackPanel_MainMenu.Children.Add(invertTimeStackPanel);
        stackPanel_MainMenu.Children.Add(invertVideoCountStackPanel);
        stackPanel_MainMenu.Children.Add(videoCountMultiplyStackPanel);


        Button statsUpdateButton = BuildButton("mainMenu_StatsUpdateButton", StatsUpdateButton_Click);
        Button quitButton = BuildButton("mainMenu_ExitProgramButton", ExitButton_Click);
        Button sendButton = BuildButton("mainMenu_SendButton", SendButton_Click);

        stackPanel_MainMenu.Children.Add(sendButton);
        stackPanel_MainMenu.Children.Add(statsUpdateButton);
        stackPanel_MainMenu.Children.Add(quitButton);


        Button BuildButton(string tag, EventHandler<RoutedEventArgs> clickEvent)
        {
            Button button = new Button()
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Thickness(0, 50, 0, 0)
            };

            button.Click += clickEvent;

            button.Content = new TextBlock()
            {
                Name = tag,
                Text = LocaleManager.Locales[language][tag],
                Tag = new Tag(true),
                Margin = new Thickness(50, 0, 50, 0)
            };

            return button;
        }

        StackPanel BuildStackPanelWithCheckBoxAndTextBlock(string checkBoxTag, string textBlockTag)
        {
            CheckBox checkBox = new CheckBox()
            {
                Tag = new Tag(false, checkBoxTag, "mainMenu"),
            };

            TextBlock textBlock = new TextBlock()
            {
                Name = textBlockTag,
                Text = LocaleManager.Locales[language][textBlockTag],
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                Tag = new Tag(true)
            };

            StackPanel stackPanel = new StackPanel()
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stackPanel.Children.Add(checkBox);
            stackPanel.Children.Add(textBlock);

            return stackPanel;
        }
    }

    private void SendButton_Click(object? sender, RoutedEventArgs e)
    {
        if (stackPanel_MainMenu.IsVisible)
        {
            var config = ConfigManager.ReadConfig(ConfigManager.ConfigPath);

            var checkBoxes = this.GetVisualDescendants().OfType<CheckBox>().Where(c => c.Tag is Tag && (c.Tag as Tag ?? new Tag(false)).OtherTag.Contains("mainMenu")).ToArray();
            
            int watchTime = 0;
            int speakTime = 0;
            int videoCount = 0;

            foreach (var children in this.GetVisualDescendants().OfType<Control>().Where(x => x.Tag is Tag && (x.Tag as Tag ?? new Tag(false)).OtherTag.Contains("mainMenu")).ToArray())
            {
                string tag = (children.Tag as Tag)?.OtherTag[0] ?? "";

                int input = 0;

                switch (tag)
                {
                    case "TodaySecondCount":
                    case "TodayLanguageUseSecondCount":
                        string[] requests = (children as TextBox)?.Text?.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

                        if (requests.Length < 1)
                            break;

                        foreach (string request in requests)
                        {
                            string key = "";
                            int value = 0;

                            for (int i = 0; i < request.Length; i++)
                                if (!int.TryParse(request[i].ToString(), out _))
                                    key += request[i];

                            key.ToLower();

                            if (!int.TryParse(request.Replace(key, ""), out value))
                                continue;

                            switch (key)
                            {
                                case "h":
                                    input += value * 3600;
                                    break;
                                case "m":
                                    input += value * 60;
                                    break;
                                case "s":
                                    input += value;
                                    break;
                            }
                        }

                        switch (tag)
                        {
                            case "TodaySecondCount":
                                watchTime = input;
                                break;
                            case "TodayLanguageUseSecondCount":
                                speakTime = input;
                                break;

                            default:
                                continue;
                        }

                        break;

                    case "WatchedVideoCount":
                        int.TryParse((children as TextBox)?.Text, out videoCount);
                        break;

                    default:
                        continue;
                }

                if (children is TextBox tb)
                    tb.Text = string.Empty;
            }

            if (checkBoxes.Where(x => (x.Tag as Tag ?? new Tag(false)).OtherTag.Contains("multiplyVideoCount")).ToArray()[0].IsChecked ?? false)
                watchTime *= videoCount;

            if (checkBoxes.Where(x => (x.Tag as Tag ?? new Tag(false)).OtherTag.Contains("invertTime")).ToArray()[0].IsChecked ?? false)
            {
                speakTime *= -1;
                watchTime *= -1;
            }
            
            if (checkBoxes.Where(x => (x.Tag as Tag ?? new Tag(false)).OtherTag.Contains("invertVideoCount")).ToArray()[0].IsChecked ?? false)
                videoCount *= -1;

            config["TodaySecondCount"] = (int.Parse(config["TodaySecondCount"]) + watchTime).ToString();
            config["TodayLanguageUseSecondCount"] = (int.Parse(config["TodayLanguageUseSecondCount"]) + speakTime).ToString();
            config["WatchedVideoCount"] = (int.Parse(config["WatchedVideoCount"]) + videoCount).ToString();

            foreach (var checkBox in checkBoxes)
                checkBox.IsChecked = false;

            ConfigManager.WriteConfig(ConfigManager.ConfigPath, config);

            UpdateLocale(ConfigManager.ReadConfig(ConfigManager.ConfigPath)["language"]);
        }
    }

    private void ExitButton_Click(object? sender, RoutedEventArgs e) => Environment.Exit(0);

    private void StatsUpdateButton_Click(object? sender, RoutedEventArgs e) => UpdateLocale(ConfigManager.ReadConfig(ConfigManager.ConfigPath)["language"]);

    private void SettingsSaveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (stackPanel_Settings.IsVisible)
        {
            var config = ConfigManager.ReadConfig(ConfigManager.ConfigPath);

            foreach (var children in this.GetVisualDescendants().OfType<Control>().Where(x => (x.Tag is Tag) && (x.Tag as Tag ?? new Tag(false)).OtherTag.Contains("settings")).ToArray())
            {
                string tag = (children.Tag as Tag)?.OtherTag[0] ?? "";

                if (config.ContainsKey(tag) && children is TextBox)
                    config[tag] = (children as TextBox)?.Text ?? config[tag];
                else if (config.ContainsKey(tag) && children is ComboBox)
                    config[tag] = (children as ComboBox)?.SelectedItem as string ?? config[tag];
            }

            ConfigManager.WriteConfig(ConfigManager.ConfigPath, config);

            UpdateLocale(ConfigManager.ReadConfig(ConfigManager.ConfigPath)["language"]);
        }
    }

    private void Settings_Click(object? sender, RoutedEventArgs e)
    {
        button_Settings.Click -= Settings_Click;
        button_Settings.Click += MainMenu_Click;

        border_MainMenu.IsVisible = false;
        border_Settings.IsVisible = true;

        UpdateLocale(ConfigManager.ReadConfig(ConfigManager.ConfigPath)["language"]);
        textBlock_stats.Text = MakeStats(ConfigManager.ReadConfig(ConfigManager.ConfigPath)["language"]);

        UtilitieManager.ClearAllTextBoxes(this);

        SettingsSetup(ConfigManager.ReadConfig(ConfigManager.ConfigPath)["language"]);
    }

    private void MainMenu_Click(object? sender, RoutedEventArgs e)
    {
        button_Settings.Click -= MainMenu_Click;
        button_Settings.Click += Settings_Click;

        border_Settings.IsVisible = false;
        border_MainMenu.IsVisible = true;

        UpdateLocale(ConfigManager.ReadConfig(ConfigManager.ConfigPath)["language"]);
        textBlock_stats.Text = MakeStats(ConfigManager.ReadConfig(ConfigManager.ConfigPath)["language"]);

        UtilitieManager.ClearAllTextBoxes(this);

        MainMenuSetup(ConfigManager.ReadConfig(ConfigManager.ConfigPath)["language"]);
    }

    private void UpdateLocale(string language)
    {
        var hotReload = this.GetVisualDescendants().OfType<TextBlock>().Where(x => (x.Tag as Tag ?? new Tag(false)).IsHotLocaleReload).ToArray();
        foreach (var textBlock in hotReload)
            textBlock.Text = LocaleManager.Locales[language][textBlock.Name ?? "error_1"];

        UpdateStats(language);
    }

    private void UpdateStats(string language)
    {
        var config = ConfigManager.ReadConfig(ConfigManager.ConfigPath);

        DateTime lastLoginDate = DateTime.Parse(config["LastLoginDate"]);
        int overallSecondCount = int.Parse(config["OverallSecondCount"]);
        int todaySecondCount = int.Parse(config["TodaySecondCount"]);
        int penaltySecondCount = int.Parse(config["PenaltySecondCount"]);
        int planQuotaAmount = int.Parse(config["PlanQuotaAmount"]);
        int quotaBonusProcent = int.Parse(config["QuotaBonusProcent"]);
        int overallLanguageUseDays = int.Parse(config["OverallLanguageUseDays"]);
        int overallLanguageUseSecondCount = int.Parse(config["OverallLanguageUseSecondCount"]);
        int todayLanguageUseSecondCount = int.Parse(config["TodayLanguageUseSecondCount"]);

        int daysPassedSinceLastLogin = (DateTime.Now.Date - lastLoginDate.Date).Days;

        if (daysPassedSinceLastLogin > 0)
        {
            lastLoginDate = DateTime.Now;

            int tommorowPlanLefover = planQuotaAmount * 3600 * daysPassedSinceLastLogin + penaltySecondCount - todaySecondCount;

            if (todayLanguageUseSecondCount > 0)
            {
                tommorowPlanLefover -= (int)Math.Round((double)todayLanguageUseSecondCount / 100.0 * quotaBonusProcent, 0);
                overallLanguageUseDays += 1;
            }

            penaltySecondCount = tommorowPlanLefover >= 0 ? tommorowPlanLefover : 0;
            overallSecondCount += todaySecondCount;
            overallLanguageUseSecondCount += todayLanguageUseSecondCount;

            todaySecondCount = 0;
            todayLanguageUseSecondCount = 0;
        }

        config["LastLoginDate"] = lastLoginDate.ToString("O");
        config["OverallSecondCount"] = overallSecondCount.ToString();
        config["TodaySecondCount"] = todaySecondCount.ToString();
        config["PenaltySecondCount"] = penaltySecondCount.ToString();
        config["PlanQuotaAmount"] = planQuotaAmount.ToString();
        config["QuotaBonusProcent"] = quotaBonusProcent.ToString();
        config["OverallLanguageUseDays"] = overallLanguageUseDays.ToString();
        config["OverallLanguageUseSecondCount"] = overallLanguageUseSecondCount.ToString();
        config["TodayLanguageUseSecondCount"] = todayLanguageUseSecondCount.ToString();

        ConfigManager.WriteConfig(ConfigManager.ConfigPath, config);

        textBlock_stats.Text = MakeStats(language);
    }

    private string MakeStats(string language)
    {
        string stats = "";
        Dictionary<string, string> config = ConfigManager.ReadConfig(ConfigManager.ConfigPath);

        DateTime startDate = DateTime.Parse(config["StartDate"]);
        int overallSecondCount = int.Parse(config["OverallSecondCount"]);
        int todaySecondCount = int.Parse(config["TodaySecondCount"]);
        int penaltySecondCount = int.Parse(config["PenaltySecondCount"]);
        int watchedVideoCount = int.Parse(config["WatchedVideoCount"]);
        int planWatchHourAmount = int.Parse(config["PlanWatchHourAmount"]);
        int planSpeakHourAmount = int.Parse(config["PlanSpeakHourAmount"]);
        int planQuotaAmount = int.Parse(config["PlanQuotaAmount"]);
        int quotaBonusProcent = int.Parse(config["QuotaBonusProcent"]);
        int overallLanguageUseDays = int.Parse(config["OverallLanguageUseDays"]);
        int overallLanguageUseSecondCount = int.Parse(config["OverallLanguageUseSecondCount"]);
        int todayLanguageUseSecondCount = int.Parse(config["TodayLanguageUseSecondCount"]);

        int todayQuotaBonus = todayLanguageUseSecondCount / 100 * quotaBonusProcent;

        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_WatchPlan"],
            Math.Round(overallSecondCount / ((double)planWatchHourAmount * 3600 / 100), 3).ToString(),
            planWatchHourAmount.ToString()) + "\n";
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_WatchEstimatedTime"],
            Math.Round(((double)planWatchHourAmount * 3600 - overallSecondCount) / 3600 / planQuotaAmount, 0, MidpointRounding.ToPositiveInfinity).ToString()) + "\n";

        string time = FormatSeconds(overallSecondCount, language);
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_WatchedHours"],
            time) + "\n";
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_WatchedVideos"],
            watchedVideoCount.ToString()) + "\n";
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_ElapsedDays"],
            (DateTime.Now - startDate).Days.ToString()) + "\n";

        time = FormatSeconds(todaySecondCount, language);
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_WatchedToday"],
            time) + "\n";

        int temp_time = planQuotaAmount * 3600 + penaltySecondCount - todaySecondCount;
        temp_time = todayLanguageUseSecondCount > 0 ? temp_time - todayQuotaBonus : temp_time;

        time = FormatSeconds(temp_time, language, LocaleManager.Locales[language]["stats_TodayPlanOverflow"]);
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_TodayPlan"],
            time) + "\n";

        time = FormatSeconds(penaltySecondCount, language, LocaleManager.Locales[language]["stats_PenaltyOverflow"]);
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_Penalty"],
            time) + "\n\n";

        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_SpeakPlan"],
            Math.Round(overallLanguageUseSecondCount / ((double)planSpeakHourAmount * 3600 / 100), 3).ToString(),
            planSpeakHourAmount.ToString()) + "\n";
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_SpeakEstimatedTime"],
            Math.Round(((double)planSpeakHourAmount * 3600 - overallLanguageUseSecondCount) / ((double)overallLanguageUseSecondCount / overallLanguageUseDays), 0, MidpointRounding.ToPositiveInfinity).ToString()) + "\n";

        time = FormatSeconds(overallLanguageUseSecondCount, language);
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_SpokenHours"],
            time) + "\n";
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_SpokenDays"],
            overallLanguageUseDays.ToString()) + "\n";

        time = FormatSeconds(todayLanguageUseSecondCount, language);
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_SpokenToday"],
            time) + "\n";

        time = FormatSeconds(todayQuotaBonus, language);
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_TodayQuotaBonus"],
            time) + "\n\n";
        stats += LocaleManager.FormatLine(LocaleManager.Locales[language]["stats_StartDate"],
            startDate.ToString());

        return stats;
    }

    private string FormatSeconds(int unformated, string language, string overflow = "-")
    {
        string time = string.Empty;

        int hours = unformated / 3600;
        int minutes = unformated % 3600 / 60;
        int seconds = unformated % 60;

        time += hours > 0 ? $"{hours}{LocaleManager.Locales[language]["stats_Hour"]} " : "";
        time += minutes > 0 ? $"{minutes}{LocaleManager.Locales[language]["stats_Minute"]} " : "";
        time += seconds > 0 ? $"{seconds}{LocaleManager.Locales[language]["stats_Second"]} " : "";

        time = time.Equals("") ? overflow : time;

        return time;
    }

    private bool CheckFiles(string path, out string error)
    {
        path = Path.Combine(path, "Data");

        if (!Directory.Exists(path) || !Directory.Exists(Path.Combine(path, "Locale")))
        {
            error = "Important directories do not exist! IFDE_1";
            return false;
        }

        if (!File.Exists(Path.Combine(path, "error.log")))
            File.Create(Path.Combine(path, "error.log"));

        if (!File.Exists(Path.Combine(path, "config.ini")))
        {
            File.Create(Path.Combine(path, "config.ini")).Close();

            ConfigManager.WriteDefaultConfig(ConfigManager.ConfigPath);
        }

        if (Directory.GetFiles(Path.Combine(path, "Locale")).Length < 1)
        {
            error = "Important directories do not exist (locales)! IFDE_2";
            return false;
        }

        error = "OK";
        return true;
    }

    private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogManager.Log($"{e.ExceptionObject}");
    }
}