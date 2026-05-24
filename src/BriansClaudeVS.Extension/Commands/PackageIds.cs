namespace BriansClaudeVS;

internal static class PackageIds
{
    public const string PackageGuidString = "DE73F1AB-E288-45B5-956D-0F9CC77A3B2A";
    public const string CommandSetGuidString = "7B431BC8-DDFC-485F-BFC5-772FF0EFC7C8";

    public static readonly Guid PackageGuid = new(PackageGuidString);
    public static readonly Guid CommandSetGuid = new(CommandSetGuidString);

    public const int OpenChatWindowCommandId = 0x0100;
}
