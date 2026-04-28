namespace VaultIQ.Shared.DTOs;

/// <summary>
/// DTO sérialisable représentant un groupe d'entrées.
/// </summary>
public sealed record GroupDto
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Icon { get; init; } = "🔐";
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public int EntryCount { get; init; } = 0;
}