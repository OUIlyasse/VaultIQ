using VaultIQ.Shared.Enums;

namespace VaultIQ.Shared.Extensions;

/// <summary>
/// Extensions sur DateTime pour l'affichage convivial dans VaultIQ.
/// </summary>
public static class DateTimeExtensions
{
    // ── Relative time ─────────────────────────────────────────────────────
    /// <summary>
    /// Retourne une chaîne relative lisible :
    /// "il y a 3 jours", "dans 2 heures", "aujourd'hui"…
    /// </summary>
    public static string ToRelativeString(
        this DateTime dateTime,
        Language lang = Language.French,
        DateTime? relativeTo = null)
    {
        var now = (relativeTo ?? DateTime.UtcNow).ToLocalTime();
        var local = dateTime.ToLocalTime();
        var diff = now - local;

        if (lang == Language.French) return ToRelativeFR(diff, local, now);
        if (lang == Language.Arabic) return ToRelativeAR(diff, local, now);
        return ToRelativeEN(diff, local, now);
    }

    private static string ToRelativeFR(TimeSpan diff, DateTime local, DateTime now)
    {
        bool future = diff.TotalSeconds < 0;
        diff = diff.Duration();

        if (diff.TotalSeconds < 60) return future ? "dans quelques secondes" : "à l'instant";
        if (diff.TotalMinutes < 60) return future ? $"dans {(int)diff.TotalMinutes} min" : $"il y a {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24) return future ? $"dans {(int)diff.TotalHours} h" : $"il y a {(int)diff.TotalHours} h";
        if (diff.TotalDays < 2) return future ? "demain" : "hier";
        if (diff.TotalDays < 7) return future ? $"dans {(int)diff.TotalDays} jours" : $"il y a {(int)diff.TotalDays} jours";
        if (diff.TotalDays < 30) return future ? $"dans {(int)(diff.TotalDays / 7)} sem." : $"il y a {(int)(diff.TotalDays / 7)} sem.";
        if (diff.TotalDays < 365) return future ? $"dans {(int)(diff.TotalDays / 30)} mois" : $"il y a {(int)(diff.TotalDays / 30)} mois";
        return local.ToString("dd/MM/yyyy");
    }

    private static string ToRelativeEN(TimeSpan diff, DateTime local, DateTime now)
    {
        bool future = diff.TotalSeconds < 0;
        diff = diff.Duration();

        if (diff.TotalSeconds < 60) return future ? "in a few seconds" : "just now";
        if (diff.TotalMinutes < 60) return future ? $"in {(int)diff.TotalMinutes}m" : $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24) return future ? $"in {(int)diff.TotalHours}h" : $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays < 2) return future ? "tomorrow" : "yesterday";
        if (diff.TotalDays < 7) return future ? $"in {(int)diff.TotalDays}d" : $"{(int)diff.TotalDays}d ago";
        if (diff.TotalDays < 30) return future ? $"in {(int)(diff.TotalDays / 7)}w" : $"{(int)(diff.TotalDays / 7)}w ago";
        if (diff.TotalDays < 365) return future ? $"in {(int)(diff.TotalDays / 30)} mo" : $"{(int)(diff.TotalDays / 30)}mo ago";
        return local.ToString("MM/dd/yyyy");
    }

    private static string ToRelativeAR(TimeSpan diff, DateTime local, DateTime now)
    {
        bool future = diff.TotalSeconds < 0;
        diff = diff.Duration();

        if (diff.TotalSeconds < 60) return future ? "خلال ثوانٍ" : "الآن";
        if (diff.TotalMinutes < 60) return future ? $"خلال {(int)diff.TotalMinutes} دقيقة" : $"منذ {(int)diff.TotalMinutes} دقيقة";
        if (diff.TotalHours < 24) return future ? $"خلال {(int)diff.TotalHours} ساعة" : $"منذ {(int)diff.TotalHours} ساعة";
        if (diff.TotalDays < 2) return future ? "غداً" : "أمس";
        if (diff.TotalDays < 30) return future ? $"خلال {(int)diff.TotalDays} يوم" : $"منذ {(int)diff.TotalDays} يوم";
        return local.ToString("yyyy/MM/dd");
    }

    // ── Expiry helpers ────────────────────────────────────────────────────
    /// <summary>La date est-elle dépassée (expirée) ?</summary>
    public static bool IsExpired(this DateTime? date) =>
        date.HasValue && date.Value.Date < DateTime.UtcNow.Date;

    /// <summary>La date expire-t-elle dans les N prochains jours ?</summary>
    public static bool IsExpiringSoon(this DateTime? date, int days = 7) =>
        date.HasValue &&
        !date.IsExpired() &&
        date.Value.Date <= DateTime.UtcNow.Date.AddDays(days);

    /// <summary>Nombre de jours restants avant expiration (négatif = déjà expiré).</summary>
    public static int DaysUntilExpiry(this DateTime? date) =>
        date.HasValue
            ? (int)(date.Value.Date - DateTime.UtcNow.Date).TotalDays
            : int.MaxValue;

    // ── Formatting ────────────────────────────────────────────────────────
    /// <summary>Format court pour l'affichage dans le DataGrid : "12/04/2025 14:32".</summary>
    public static string ToShortDisplay(this DateTime dt) =>
        dt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

    /// <summary>Format long pour le panneau de détail : "12 avril 2025 — 14:32:05".</summary>
    public static string ToLongDisplay(this DateTime dt) =>
        dt.ToLocalTime().ToString("dd MMMM yyyy — HH:mm:ss",
            new System.Globalization.CultureInfo("fr-FR"));
}