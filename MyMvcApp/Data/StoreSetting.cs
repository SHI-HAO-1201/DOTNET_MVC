namespace MyMvcApp.Data;

public sealed class StoreSetting
{
    public required string Key { get; set; }
    public string Value { get; set; } = string.Empty;
}
