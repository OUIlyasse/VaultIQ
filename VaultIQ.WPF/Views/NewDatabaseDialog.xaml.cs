using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VaultIQ.WPF.ViewModels;

namespace VaultIQ.WPF.Views;

public partial class NewDatabaseDialog : Window
{
    private readonly NewDatabaseViewModel _vm;

    public NewDatabaseDialog()
    {
        InitializeComponent();
        _vm = new NewDatabaseViewModel();
        DataContext = _vm;

        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(NewDatabaseViewModel.PasswordStrength))
                UpdateStrengthBar();
            if (e.PropertyName == nameof(NewDatabaseViewModel.PasswordsMatch))
                UpdateConfirmIndicator();
        };
    }

    // ── Public results ────────────────────────────────────────────
    public string DatabaseName => _vm.DatabaseName;

    public string MasterPassword => _vm.MasterPassword;
    public string FilePath => _vm.FilePath;
    public bool EnablePin => _vm.EnablePin;
    public bool EnableRecovery => _vm.EnableRecovery;

    // ── Header drag ───────────────────────────────────────────────
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    // ── Password change handlers ──────────────────────────────────
    private void MasterPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _vm.MasterPassword = MasterPasswordBox.Password;
        _vm.UpdateStrength();
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _vm.ConfirmPassword = ConfirmPasswordBox.Password;
    }

    // ── Eye toggle (master password) ─────────────────────────────
    private void EyeBtn1_Click(object sender, RoutedEventArgs e)
    {
        // PasswordBox doesn't support plain-text switch natively;
        // swap with a TextBox overlay (same approach as LoginDialog)
    }

    // ── Strength bar ─────────────────────────────────────────────
    private void UpdateStrengthBar()
    {
        double ratio = Math.Clamp(_vm.PasswordStrength / 100.0, 0, 1);
        StrengthBarFill.Width = StrengthBarContainer.ActualWidth * ratio;

        var (color, label) = _vm.PasswordStrength switch
        {
            >= 80 => ("#22C55E", $"Très fort — {_vm.PasswordStrength}/100"),
            >= 60 => ("#F59E0B", $"Moyen — {_vm.PasswordStrength}/100"),
            _ => ("#EF4444", $"Faible — {_vm.PasswordStrength}/100")
        };

        StrengthBarFill.Background = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(color));
        StrengthLabel.Text = label;
        StrengthLabel.Foreground = StrengthBarFill.Background;
        EntropyLabel.Text = $"Entropie ≈ {_vm.EntropyBits} bits";
    }

    // ── Confirm match indicator ───────────────────────────────────
    private void UpdateConfirmIndicator()
    {
        if (_vm.PasswordsMatch)
        {
            ConfirmMatchIcon.Visibility = Visibility.Visible;
            ConfirmErrorText.Visibility = Visibility.Collapsed;
        }
        else if (!string.IsNullOrEmpty(_vm.ConfirmPassword))
        {
            ConfirmMatchIcon.Visibility = Visibility.Collapsed;
            ConfirmErrorText.Text = "Les mots de passe ne correspondent pas.";
            ConfirmErrorText.Visibility = Visibility.Visible;
        }
        else
        {
            ConfirmMatchIcon.Visibility = Visibility.Collapsed;
            ConfirmErrorText.Visibility = Visibility.Collapsed;
        }
    }

    // ── Browse ────────────────────────────────────────────────────
    private void BrowseBtn_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new SaveFileDialog
        {
            Title = "Choisir l'emplacement de la base",
            Filter = "Base VaultIQ (*.viq)|*.viq",
            FileName = _vm.DatabaseName,
            DefaultExt = ".viq"
        };
        if (dlg.ShowDialog() == true)
            _vm.FilePath = dlg.FileName;
    }

    // ── Buttons ───────────────────────────────────────────────────
    private void CreateBtn_Click(object sender, RoutedEventArgs e)
    {
        if (!_vm.Validate(out string error))
        {
            MessageBox.Show(error, "VaultIQ", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        DialogResult = true;
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}