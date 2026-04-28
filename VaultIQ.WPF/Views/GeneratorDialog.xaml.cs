using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VaultIQ.WPF.ViewModels;

namespace VaultIQ.WPF.Views;

public partial class GeneratorDialog : Window
{
    private readonly GeneratorViewModel _vm;
    private DispatcherTimer? _feedbackTimer;

    /// <summary>
    /// Standalone generator. Set <see cref="ShowUseButton"/> = true when opened
    /// from an EntryDialog so the caller can retrieve <see cref="GeneratedPassword"/>.
    /// </summary>
    public GeneratorDialog(bool showUseButton = false)
    {
        InitializeComponent();
        _vm = new GeneratorViewModel();
        DataContext = _vm;

        // "Use this password" button only shown when opened from EntryDialog
        UseBtn.Visibility = showUseButton ? Visibility.Visible : Visibility.Collapsed;

        _vm.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(GeneratorViewModel.GeneratedPassword):
                    UpdatePreview();
                    break;
                case nameof(GeneratorViewModel.History):
                    HistoryList.ItemsSource = _vm.History;
                    break;
            }
        };

        // Initial generation
        _vm.Regenerate();
        LengthValue.Text = ((int)LengthSlider.Value).ToString();
        HistoryList.ItemsSource = _vm.History;
    }

    /// <summary>Password selected by the user (read by caller when DialogResult = true).</summary>
    public string GeneratedPassword => _vm.GeneratedPassword;

    // ── Header drag ───────────────────────────────────────────────
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    // ── Spacebar regenerates ──────────────────────────────────────
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Space) { _vm.Regenerate(); e.Handled = true; }
        if (e.Key == Key.Escape) { DialogResult = false; e.Handled = true; }
    }

    // ── Preview update ────────────────────────────────────────────
    private void UpdatePreview()
    {
        PreviewText.Text = _vm.GeneratedPassword;
        EntropyBadge.Text = _vm.EntropyLabel;
        MinEntropyInfo.Text = _vm.EntropyLabel;

        // Strength bar
        int score = VaultIQ.Core.Entities.PasswordStrengthHelper.Evaluate(_vm.GeneratedPassword);
        double ratio = Math.Clamp(score / 100.0, 0, 1);
        StrengthFill.Width = StrengthContainer.ActualWidth * ratio;

        string hex = VaultIQ.Core.Entities.PasswordStrengthHelper.Color(score);
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        StrengthFill.Background = brush;
        StrengthLabel.Foreground = brush;
        StrengthLabel.Text = $"{VaultIQ.Core.Entities.PasswordStrengthHelper.Label(score)} — {score}/100";
    }

    // ── Sliders & checkboxes ──────────────────────────────────────
    private void LengthSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (LengthValue is null) return;
        int len = (int)LengthSlider.Value;
        LengthValue.Text = len.ToString();
        _vm.Length = len;
        _vm.Regenerate();
    }

    private void Option_Changed(object sender, RoutedEventArgs e)
    {
        _vm.UseUppercase = ChkUppercase.IsChecked == true;
        _vm.UseLowercase = ChkLowercase.IsChecked == true;
        _vm.UseDigits = ChkDigits.IsChecked == true;
        _vm.UseSymbols = ChkSymbols.IsChecked == true;
        _vm.NoAmbiguous = ChkNoAmbiguous.IsChecked == true;
        _vm.Regenerate();
    }

    // ── Regenerate button ─────────────────────────────────────────
    private void RegenerateBtn_Click(object sender, RoutedEventArgs e) => _vm.Regenerate();

    // ── Copy ──────────────────────────────────────────────────────
    private void CopyBtn_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_vm.GeneratedPassword);
        ShowCopiedFeedback();
    }

    private void HistoryCopy_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is string pwd)
        {
            Clipboard.SetText(pwd);
            ShowCopiedFeedback();
        }
    }

    private void ShowCopiedFeedback()
    {
        CopiedFeedback.Visibility = Visibility.Visible;
        _feedbackTimer?.Stop();
        _feedbackTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _feedbackTimer.Tick += (_, _) =>
        {
            CopiedFeedback.Visibility = Visibility.Collapsed;
            _feedbackTimer.Stop();
        };
        _feedbackTimer.Start();
    }

    // ── Use / Close ───────────────────────────────────────────────
    private void UseBtn_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void CloseBtn_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    // ── Resize → update strength bar ─────────────────────────────
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdatePreview();
    }
}