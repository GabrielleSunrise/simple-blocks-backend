namespace SimpleBlocks.Application.Common;

/// <summary>
/// Canonical identifiers of the block types produced by the frontend.
/// Must match the section prefixes used in the UI.
/// </summary>
public static class BlockTypes
{
    public const string Vertical = "vertical";
    public const string Horizontal = "horizontal";
    public const string Overlay = "overlay";
    public const string Faq = "faq";
    public const string Scroll = "scroll";
    public const string Table = "table";
    public const string Gallery = "gallery";
    public const string Banner = "banner";

    public static readonly string[] All =
    {
        Vertical, Horizontal, Overlay, Faq, Scroll, Table, Gallery, Banner
    };

    public static bool IsValid(string? type)
        => type is not null && All.Contains(type, StringComparer.OrdinalIgnoreCase);
}
