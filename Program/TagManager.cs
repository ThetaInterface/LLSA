namespace LLSA.TagManager;

public sealed class Tag
{
    public Tag(bool isHotLocaleReload, params string[] otherTags)
    {
        IsHotLocaleReload = isHotLocaleReload;
        OtherTag = otherTags;
    }

    public bool IsHotLocaleReload { get; }
    public string[] OtherTag { get; }
}