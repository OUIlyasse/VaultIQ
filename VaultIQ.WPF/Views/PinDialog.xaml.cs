using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VaultIQ.WPF.ViewModels;

namespace VaultIQ.WPF.Views;

public partial class PinDialog : Window
{
    private readonly PinViewModel _vm;
    private readonly PasswordBox[] _boxes;

    public PinDialog()
    {
        InitializeComponent();
        _vm = new PinViewModel();
        DataContext = _vm;

        _boxes = [Pin1, Pin2, Pin3, Pin4, Pin5, Pin6];

        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(PinViewModel.PinLength))
                UpdateUI();
        };
    }

    /// <summary>The collected PIN string (read by caller after DialogResult = true).</summary>
    public string Pin => _vm.Pin;

    // ── Header drag ───────────────────────────────────────────────
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    // ── PIN box handlers ──────────────────────────────────────────
    private void PinBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb)
            pb.SelectAll();
    }

    private void PinBox_Changed(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox current) return;

        // Collect full PIN from all boxes
        _vm.Pin = string.Concat(_boxes.Select(b => b.Password.Length > 0
            ? b.Password[^1].ToString()
            : ""));

        // Auto-advance focus
        int idx = Array.IndexOf(_boxes, current);
        if (current.Password.Length > 0 && idx < _boxes.Length - 1)
            _boxes[idx + 1].Focus();
    }

    // ── UI update ────────────────────────────────────────────────
    private void UpdateUI()
    {
        int len = _vm.PinLength;

        PinCounterText.Text = $"{len} / 6 chiffres · 6+ chiffres = excellent";
        NextBtn.IsEnabled = len >= 4;

        double ratio = Math.Clamp(len / 8.0, 0, 1);
        PinStrengthFill.Width = PinStrengthContainer.ActualWidth * ratio;

        var (color, label) = len switch
        {
            >= 6 => ("#22C55E", $"Bon PIN ({len} chiffres)"),
            >= 4 => ("#F59E0B", $"PIN acceptable ({len} chiffres)"),
            _ => ("#EF4444", "PIN trop court (min. 4)")
        };

        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        PinStrengthFill.Background = brush;
        PinStrengthLabel.Foreground = brush;
        PinStrengthLabel.Text = label;
    }

    // ── Button handlers ───────────────────────────────────────────
    private void NextBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.PinLength < 4)
        {
            MessageBox.Show("Le PIN doit contenir au moins 4 chiffres.", "VaultIQ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DialogResult = true;
    }

    private void BackBtn_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    private void SkipBtn_Click(object sender, RoutedEventArgs e)
    {
        _vm.Pin = string.Empty;
        DialogResult = true;   // caller checks Pin == "" to know it was skipped
    }
}