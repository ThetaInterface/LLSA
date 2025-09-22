using LLSA.Config;
using LLSA.Locale;
using LLSA.TagManager;

namespace LLSA.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public string Version { get; } = $"v {ConfigManager.ReadConfig(ConfigManager.ConfigPath)["version"]}";
    public string Title { get; } = LocaleManager.Locales[$"{ConfigManager.ReadConfig(ConfigManager.ConfigPath)["language"]}"]["application_title"];

    public Tag HotReloadLocale { get; } = new Tag(true);

    public string Color1 { get; } = "#171717";
    public string Color2 { get; } = "#080808";
    public string Color3 { get; } = "#7d7d7d";
    public string Color4 { get; } = "#b3b3b3";
    public string Color5 { get; } = "#ffffff";
}
