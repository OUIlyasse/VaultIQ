using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VaultIQ.WPF.ViewModels;

namespace VaultIQ.WPF.Views;

public partial class GeneratorDialog : Window
{
    // ── VM créé AVANT InitializeComponent() ──────────────────────
    // InitializeComponent() déclenche :
    //   • LengthSlider.ValueChanged  → LengthSlider_Changed  → _vm.Length   → NRE
    //   • CheckBox.Checked           → Option_Changed         → _vm.UseXxx   → NRE
    // _vm doit donc exister avant le premier appel de handler.
    private readonly GeneratorViewModel _vm;
    private DispatcherTimer? _feedbackTimer;

    public GeneratorDialog(bool showUseButton = false)
    {
        // ── 1. VM en premier ──────────────────────────────────────
        _vm = new GeneratorViewModel();

        // ── 2. Composants ─────────────────────────────────────────
        InitializeComponent();
        DataContext = _vm;

        // ── 3. Abonnement TARDIF des events slider / checkboxes ───
        // Retirer ValueChanged / Checked / Unchecked du XAML et
        // les abonner ici garantit que tous les contrôles existent.
        LengthSlider.ValueChanged += LengthSlider_Changed;
        ChkUppercase.Checked += Option_Changed;
        ChkUppercase.Unchecked += Option_Changed;
        ChkLowercase.Checked += Option_Changed;
        ChkLowercase.Unchecked += Option_Changed;
        ChkDigits.Checked += Option_Changed;
        ChkDigits.Unchecked += Option_Changed;
        ChkSymbols.Checked += Option_Changed;
        ChkSymbols.Unchecked += Option_Changed;
        ChkNoAmbiguous.Checked += Option_Changed;
        ChkNoAmbiguous.Unchecked += Option_Changed;

        // ── 4. Bouton "Utiliser" ──────────────────────────────────
        UseBtn.Visibility = showUseButton ? Visibility.Visible : Visibility.Collapsed;

        // ── 5. Notifications VM → UI ──────────────────────────────
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

        // ── 6. Génération initiale après le premier rendu ─────────
        // Loaded garantit que ActualWidth est non-nul pour la barre de force.
        Loaded += (_, _) =>
        {
            LengthValue.Text = ((int)LengthSlider.Value).ToString();
            HistoryList.ItemsSource = _vm.History;
            _vm.Regenerate();
        };
    }

    /// <summary>Mot de passe sélectionné — lu par l'appelant quand DialogResult = true.</summary>
    public string GeneratedPassword => _vm.GeneratedPassword;

    // ── Header drag ───────────────────────────────────────────────
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => DragMove();

    // ── Espace = régénérer, Échap = fermer ───────────────────────
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Space) { _vm.Regenerate(); e.Handled = true; }
        if (e.Key == Key.Escape) { DialogResult = false; e.Handled = true; }
    }

    // ── Mise à jour de l'aperçu ───────────────────────────────────
    private void UpdatePreview()
    {
        PreviewText.Text = _vm.GeneratedPassword;
        EntropyBadge.Text = _vm.EntropyLabel;
        MinEntropyInfo.Text = _vm.EntropyLabel;

        int score = VaultIQ.Core.Entities.PasswordStrengthHelper.Evaluate(_vm.GeneratedPassword);
        double ratio = Math.Clamp(score / 100.0, 0, 1);

        StrengthContainer.UpdateLayout();   // force ActualWidth disponible
        StrengthFill.Width = StrengthContainer.ActualWidth * ratio;

        string hex = VaultIQ.Core.Entities.PasswordStrengthHelper.Color(score);
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        StrengthFill.Background = brush;
        StrengthLabel.Foreground = brush;
        StrengthLabel.Text = $"{VaultIQ.Core.Entities.PasswordStrengthHelper.Label(score)} — {score}/100";
    }

    // ── Slider longueur ───────────────────────────────────────────
    private void LengthSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_vm is null || LengthValue is null) return;   // garde pendant init
        int len = (int)LengthSlider.Value;
        LengthValue.Text = len.ToString();
        _vm.Length = len;
        _vm.Regenerate();
    }

    // ── Options caractères ────────────────────────────────────────
    private void Option_Changed(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;   // garde pendant init
        _vm.UseUppercase = ChkUppercase.IsChecked == true;
        _vm.UseLowercase = ChkLowercase.IsChecked == true;
        _vm.UseDigits = ChkDigits.IsChecked == true;
        _vm.UseSymbols = ChkSymbols.IsChecked == true;
        _vm.NoAmbiguous = ChkNoAmbiguous.IsChecked == true;
        _vm.Regenerate();
    }

    // ── Bouton régénérer ──────────────────────────────────────────
    private void RegenerateBtn_Click(object sender, RoutedEventArgs e)
        => _vm.Regenerate();

    // ── Copier ────────────────────────────────────────────────────
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
            _feedbackTimer!.Stop();
        };
        _feedbackTimer.Start();
    }

    // ── Utiliser / Fermer ─────────────────────────────────────────
    private void UseBtn_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void CloseBtn_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    // ── Resize → mettre à jour la barre de force ──────────────────
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        if (_vm is null) return;
        UpdatePreview();
    }
}