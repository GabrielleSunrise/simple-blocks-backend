using System.Text.Json;

namespace SimpleBlocks.Application.Dtos;

public record SaveBlockRequest(
    string BlockType,
    string Name,
    JsonElement Settings,
    string Html = "",
    string Css = "",
    string Js = "");

public record BlockDto(
    Guid Id,
    string BlockType,
    string Name,
    JsonElement Settings,
    string Html,
    string Css,
    string Js,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public record ReorderRequest(IReadOnlyList<Guid> Ids);

