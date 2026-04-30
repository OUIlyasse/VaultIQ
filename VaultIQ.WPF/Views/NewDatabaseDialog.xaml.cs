using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VaultIQ.WPF.ViewModels;

namespace VaultIQ.WPF.Views;

public partial class NewDatabaseDialog : Window
{
    private readonly NewDatabaseViewModel _vm;

    // ── État interne du toggle œil ────────────────────────────────
    private bool _masterVisible = false;
    private bool _confirmVisible = false;

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

    // ── Résultats lus par l'appelant (MainViewModel) ──────────────
    public string DatabaseName => _vm.DatabaseName;
    public string MasterPassword => _vm.MasterPassword;
    public string FilePath => _vm.FilePath;
    public bool EnablePin => _vm.EnablePin;
    public bool EnableRecovery => _vm.EnableRecovery;

    // ── Drag header ───────────────────────────────────────────────
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => DragMove();

    // ══════════════════════════════════════════════════════════════
    //  TOGGLE ŒIL — MOT DE PASSE PRINCIPAL
    // ══════════════════════════════════════════════════════════════
    private void EyeBtn1_Click(object sender, RoutedEventArgs e)
    {
        _masterVisible = !_masterVisible;

        if (_masterVisible)
        {
            // Passer de PasswordBox → TextBox clair
            MasterClearBox.Text = MasterPasswordBox.Password;
            MasterClearBox.Visibility = Visibility.Visible;
            MasterPasswordBox.Visibility = Visibility.Collapsed;
            EyeIcon1.Source = new System.Windows.Media.Imaging.BitmapImage(
                new Uri("/Resources/Icons/eye_off_16.png", UriKind.Relative));
        }
        else
        {
            // Repasser à PasswordBox masqué
            MasterPasswordBox.Password = MasterClearBox.Text;
            MasterPasswordBox.Visibility = Visibility.Visible;
            MasterClearBox.Visibility = Visibility.Collapsed;
            EyeIcon1.Source = new System.Windows.Media.Imaging.BitmapImage(
                new Uri("/Resources/Icons/eye_16.png", UriKind.Relative));
        }

        MasterPasswordBox.Focus();
    }

    // ══════════════════════════════════════════════════════════════
    //  TOGGLE ŒIL — CONFIRMATION
    // ══════════════════════════════════════════════════════════════
    private void EyeBtn2_Click(object sender, RoutedEventArgs e)
    {
        _confirmVisible = !_confirmVisible;

        if (_confirmVisible)
        {
            ConfirmClearBox.Text = ConfirmPasswordBox.Password;
            ConfirmClearBox.Visibility = Visibility.Visible;
            ConfirmPasswordBox.Visibility = Visibility.Collapsed;
            EyeIcon2.Source = new System.Windows.Media.Imaging.BitmapImage(
                new Uri("/Resources/Icons/eye_off_16.png", UriKind.Relative));
        }
        else
        {
            ConfirmPasswordBox.Password = ConfirmClearBox.Text;
            ConfirmPasswordBox.Visibility = Visibility.Visible;
            ConfirmClearBox.Visibility = Visibility.Collapsed;
            EyeIcon2.Source = new System.Windows.Media.Imaging.BitmapImage(
                new Uri("/Resources/Icons/eye_16.png", UriKind.Relative));
        }

        ConfirmPasswordBox.Focus();
    }

    // ══════════════════════════════════════════════════════════════
    //  CHANGEMENTS MDP — synchronisation VM
    // ══════════════════════════════════════════════════════════════

    // PasswordBox principal (mode masqué)
    private void MasterPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _vm.MasterPassword = MasterPasswordBox.Password;
        _vm.UpdateStrength();
        SyncConfirmValidation();
    }

    // TextBox principal (mode clair)
    private void MasterClearBox_TextChanged(object sender,
        System.Windows.Controls.TextChangedEventArgs e)
    {
        _vm.MasterPassword = MasterClearBox.Text;
        _vm.UpdateStrength();
        SyncConfirmValidation();
    }

    // PasswordBox confirmation (mode masqué)
    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _vm.ConfirmPassword = ConfirmPasswordBox.Password;
        SyncConfirmValidation();
    }

    // TextBox confirmation (mode clair)
    private void ConfirmClearBox_TextChanged(object sender,
        System.Windows.Controls.TextChangedEventArgs e)
    {
        _vm.ConfirmPassword = ConfirmClearBox.Text;
        SyncConfirmValidation();
    }

    // ══════════════════════════════════════════════════════════════
    //  BARRE DE FORCE
    // ══════════════════════════════════════════════════════════════
    private void UpdateStrengthBar()
    {
        double ratio = Math.Clamp(_vm.PasswordStrength / 100.0, 0, 1);

        // La largeur doit être calculée après que le Grid ait sa taille réelle
        StrengthBarContainer.UpdateLayout();
        double containerWidth = StrengthBarContainer.ActualWidth;
        StrengthBarFill.Width = containerWidth * ratio;

        var (hex, label) = _vm.PasswordStrength switch
        {
            >= 80 => ("#22C55E", $"Très fort — {_vm.PasswordStrength}/100"),
            >= 60 => ("#F59E0B", $"Moyen — {_vm.PasswordStrength}/100"),
            > 0 => ("#EF4444", $"Faible — {_vm.PasswordStrength}/100"),
            _ => ("#3D6080", string.Empty)
        };

        var brush = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(hex));

        StrengthBarFill.Background = brush;
        StrengthLabel.Text = label;
        StrengthLabel.Foreground = brush;
        EntropyLabel.Text = _vm.PasswordStrength > 0
            ? $"Entropie ≈ {_vm.EntropyBits} bits"
            : string.Empty;
    }

    // ══════════════════════════════════════════════════════════════
    //  INDICATEUR CORRESPONDANCE
    // ══════════════════════════════════════════════════════════════
    private void SyncConfirmValidation()
    {
        // Récupère le texte de confirmation selon le mode actif
        string confirm = _confirmVisible
            ? ConfirmClearBox.Text
            : ConfirmPasswordBox.Password;

        string master = _masterVisible
            ? MasterClearBox.Text
            : MasterPasswordBox.Password;

        bool empty = string.IsNullOrEmpty(confirm);
        bool match = !empty && confirm == master;

        ConfirmMatchIcon.Visibility = match ? Visibility.Visible : Visibility.Collapsed;
        ConfirmErrorText.Visibility = (!empty && !match) ? Visibility.Visible : Visibility.Collapsed;
        ConfirmErrorText.Text = "Les mots de passe ne correspondent pas.";
    }

    private void UpdateConfirmIndicator() => SyncConfirmValidation();

    // ══════════════════════════════════════════════════════════════
    //  PARCOURIR
    // ══════════════════════════════════════════════════════════════
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

    // ══════════════════════════════════════════════════════════════
    //  BOUTONS CRÉER / ANNULER
    // ══════════════════════════════════════════════════════════════
    private void CreateBtn_Click(object sender, RoutedEventArgs e)
    {
        // S'assurer que le VM a le mot de passe du champ actif
        _vm.MasterPassword = _masterVisible
            ? MasterClearBox.Text
            : MasterPasswordBox.Password;
        _vm.ConfirmPassword = _confirmVisible
            ? ConfirmClearBox.Text
            : ConfirmPasswordBox.Password;

        if (!_vm.Validate(out string error))
        {
            MessageBox.Show(error, "VaultIQ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}