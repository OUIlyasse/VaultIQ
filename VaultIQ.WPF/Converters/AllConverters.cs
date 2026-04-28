using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using VaultIQ.Core.Entities;

namespace VaultIQ.WPF.Converters;

/// <summary>int (0-100) → SolidColorBrush selon la force du mot de passe.</summary>
public sealed class StrengthToColorConverter : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c)
        => new SolidColorBrush((Color)ColorConverter.ConvertFromString(
            PasswordStrengthHelper.Color(v is int i ? i : 0)));

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>int → label "Très fort" / "Faible"…</summary>
public sealed class StrengthToLabelConverter : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c)
        => PasswordStrengthHelper.Label(v is int i ? i : 0);

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>bool → Visibility.</summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object v, Type t, object p, CultureInfo c)
    {
        bool b = v is bool bv && bv;
        return (Invert ? !b : b) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>null → Visibility (non-null = Visible).</summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c)
        => v is null ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>string password → "••••••••••••".</summary>
public sealed class MaskPasswordConverter : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c)
        => v is string s && s.Length > 0 ? new string('•', Math.Min(s.Length, 12)) : string.Empty;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>DateTime → "dd/MM/yyyy".</summary>
public sealed class DateConverter : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c)
        => v is DateTime dt ? dt.ToLocalTime().ToString("dd/MM/yyyy") : string.Empty;

    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotSupportedException();
}