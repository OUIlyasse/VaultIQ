using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace VaultIQ.WPF.Converters;

// ══════════════════════════════════════════════════════════════════════════
//  BoolToVisibilityConverter
//  ConverterParameter="Invert" → True = Collapsed, False = Visible
// ══════════════════════════════════════════════════════════════════════════
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool flag = value is bool b && b;

        // Invert si ConverterParameter == "Invert" (insensible à la casse)
        bool invert = parameter is string s &&
                      s.Equals("Invert", StringComparison.OrdinalIgnoreCase);

        if (invert) flag = !flag;

        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = parameter is string s &&
                      s.Equals("Invert", StringComparison.OrdinalIgnoreCase);

        bool visible = value is Visibility v && v == Visibility.Visible;
        return invert ? !visible : visible;
    }
}

// ══════════════════════════════════════════════════════════════════════════
//  NullToVisibilityConverter
//  null → Collapsed   /   non-null → Visible
//  ConverterParameter="Invert" → inverse
// ══════════════════════════════════════════════════════════════════════════
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isNull = value is null;
        bool invert = parameter is string s &&
                      s.Equals("Invert", StringComparison.OrdinalIgnoreCase);

        bool show = invert ? isNull : !isNull;
        return show ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// ══════════════════════════════════════════════════════════════════════════
//  MaskPasswordConverter
//  Remplace chaque caractère par "•"  (longueur fixe 12 pour ne pas révéler la longueur)
// ══════════════════════════════════════════════════════════════════════════
public class MaskPasswordConverter : IValueConverter
{
    private const string Mask = "••••••••••••";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string pwd && pwd.Length > 0 ? Mask : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// ══════════════════════════════════════════════════════════════════════════
//  DateConverter
//  DateTime / DateTime? → "dd/MM/yyyy"
//  null / DateTime.MinValue → "—"
// ══════════════════════════════════════════════════════════════════════════
public class DateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        DateTime? dt = value switch
        {
            DateTime d => d,
            DateTimeOffset o => o.LocalDateTime,
            _ => null
        };

        if (dt is null || dt.Value == DateTime.MinValue)
            return "—";

        // Convertir UTC → heure locale pour l'affichage
        var local = dt.Value.Kind == DateTimeKind.Utc
            ? dt.Value.ToLocalTime()
            : dt.Value;

        return local.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// ══════════════════════════════════════════════════════════════════════════
//  StrengthToColorConverter
//  int score (0-100) → SolidColorBrush (couleur du texte du badge)
//  Utilisé : Foreground="{Binding StrengthScore, Converter={StaticResource StrColor}}"
// ══════════════════════════════════════════════════════════════════════════
public class StrengthToColorConverter : IValueConverter
{
    // Couleurs du texte — correspondance avec PasswordStrengthHelper.Color()
    private static readonly SolidColorBrush Gray = new(Color.FromRgb(0x3D, 0x60, 0x80));
    private static readonly SolidColorBrush Red = new(Color.FromRgb(0xEF, 0x44, 0x44));
    private static readonly SolidColorBrush Orange = new(Color.FromRgb(0xF9, 0x73, 0x16));
    private static readonly SolidColorBrush Amber = new(Color.FromRgb(0xF5, 0x9E, 0x0B));
    private static readonly SolidColorBrush Lime = new(Color.FromRgb(0x84, 0xCC, 0x16));
    private static readonly SolidColorBrush Green = new(Color.FromRgb(0x22, 0xC5, 0x5E));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is int score ? score switch
        {
            0 => Gray,
            <= 20 => Red,
            <= 40 => Orange,
            <= 60 => Amber,
            <= 80 => Lime,
            _ => Green
        } : Gray;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// ══════════════════════════════════════════════════════════════════════════
//  StrengthToBackgroundConverter
//  int score (0-100) → SolidColorBrush semi-transparent (fond du badge)
//  Utilisé : Background="{Binding StrengthScore, Converter={StaticResource StrBgColor}}"
//  Enregistrer dans Window.Resources : <conv:StrengthToBackgroundConverter x:Key="StrBgColor"/>
// ══════════════════════════════════════════════════════════════════════════
public class StrengthToBackgroundConverter : IValueConverter
{
    // Fonds semi-transparents — alpha 0x1A-0x1F (~10-12 %)
    private static readonly SolidColorBrush GrayBg = new(Color.FromArgb(0x1A, 0x3D, 0x60, 0x80));
    private static readonly SolidColorBrush RedBg = new(Color.FromArgb(0x1A, 0xEF, 0x44, 0x44));
    private static readonly SolidColorBrush OrangeBg = new(Color.FromArgb(0x1A, 0xF9, 0x73, 0x16));
    private static readonly SolidColorBrush AmberBg = new(Color.FromArgb(0x1F, 0xF5, 0x9E, 0x0B));
    private static readonly SolidColorBrush LimeBg = new(Color.FromArgb(0x1F, 0x84, 0xCC, 0x16));
    private static readonly SolidColorBrush GreenBg = new(Color.FromArgb(0x1F, 0x22, 0xC5, 0x5E));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is int score ? score switch
        {
            0 => GrayBg,
            <= 20 => RedBg,
            <= 40 => OrangeBg,
            <= 60 => AmberBg,
            <= 80 => LimeBg,
            _ => GreenBg
        } : GrayBg;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}