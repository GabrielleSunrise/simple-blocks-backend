namespace SimpleBlocks.Domain.Entities;

/// <summary>
/// A user-generated block configuration persisted in the anonymous account's
/// private "vault". Settings are stored as a JSON document and are only
/// accessible to the owning account.
/// </summary>
public class SavedBlock
{
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    public SeedAccount? Owner { get; set; }

    /// <summary>One of the known block type identifiers (vertical, horizontal, overlay, faq, scroll, table, gallery, banner).</summary>
    public string BlockType { get; set; } = string.Empty;

    /// <summary>User-defined label for the saved block.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Flat JSON settings object produced by the frontend (getSettings).</summary>
    public string SettingsJson { get; set; } = string.Empty;

    /// <summary>Generated HTML snapshot of the block at save time.</summary>
    public string Html { get; set; } = string.Empty;

    /// <summary>Generated CSS snapshot of the block at save time.</summary>
    public string Css { get; set; } = string.Empty;

    /// <summary>Generated JS snapshot of the block at save time (empty when the block type has no JS).</summary>
    public string Js { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>User-defined order of the block within the account's collection.</summary>
    public int SortOrder { get; set; }
}
