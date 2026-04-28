using VaultIQ.Core.Enums;

namespace VaultIQ.Core.Entities;

/// <summary>Entrée de mot de passe dans la base VaultIQ.</summary>
public class PasswordEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GroupId { get; set; } = Guid.Empty; // Peut être null ou Guid.Empty pour les entrées sans groupe
    public string Title { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    // Calculé à la volée — pas stocké
    public PasswordStrength Strength { get; set; }

    public bool IsExpiringSoon => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.Now.AddDays(30);

    public int StrengthScore => PasswordStrengthHelper.Evaluate(Password);

    public PasswordEntry Clone() => new()
    {
        Id = Guid.NewGuid(),
        GroupId = GroupId,
        Title = Title + " (copie)",
        Username = Username,
        Password = Password,
        Url = Url,
        Notes = Notes,
        IsFavorite = false,
        CreatedAt = DateTime.UtcNow,
        ModifiedAt = DateTime.UtcNow,
    };

    public override string ToString()
    {
        return $"GroupId: {GroupId},\nTitle: {Title},\nUsername: {Username},\nStrength: {StrengthScore} ({PasswordStrengthHelper.Label(StrengthScore)})";
    }

    public void Touch() => ModifiedAt = DateTime.UtcNow;
}

/// <summary>Calcule un score de force 0–100 pour un mot de passe.</summary>
public static class PasswordStrengthHelper
{
    public static int Evaluate(string password)
    {
        if (string.IsNullOrEmpty(password)) return 0;
        int score = 0;
        if (password.Length >= 8) score += 15;
        if (password.Length >= 12) score += 10;
        if (password.Length >= 16) score += 10;
        if (System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]")) score += 15;
        if (System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]")) score += 15;
        if (System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]")) score += 15;
        if (System.Text.RegularExpressions.Regex.IsMatch(password, @"[^A-Za-z0-9]")) score += 20;
        return Math.Clamp(score, 0, 100);
    }

    public static string Label(int score) => score switch
    {
        0 => "Vide",
        <= 20 => "Très faible",
        <= 40 => "Faible",
        <= 60 => "Moyen",
        <= 80 => "Fort",
        _ => "Très fort"
    };

    public static string Color(int score) => score switch
    {
        0 => "#606080",
        <= 20 => "#EF4444",
        <= 40 => "#F97316",
        <= 60 => "#F59E0B",
        <= 80 => "#84CC16",
        _ => "#22C55E"
    };
}