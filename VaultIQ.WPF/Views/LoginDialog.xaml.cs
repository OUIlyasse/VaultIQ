using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VaultIQ.Views;
using VaultIQ.WPF.ViewModels;

namespace VaultIQ.WPF.Views;

public partial class LoginDialog : Window
{
    private readonly LoginViewModel _vm;

    /// <summary>Pass the public database name shown before password entry.</summary>
    public LoginDialog(string databaseName)
    {
        InitializeComponent();
        _vm = new LoginViewModel(databaseName);
        DataContext = _vm;

        // Populate static metadata displayed before unlock
        SubtitleText.Text = $"{databaseName} — Saisir le mot de passe";
        DbNameText.Text = databaseName;
        DbSubText.Text = $"Base personnelle · Dernière ouverture : {_vm.LastOpened}";
        StatEntries.Text = _vm.EntryCount.ToString();
        StatGroups.Text = _vm.GroupCount.ToString();
        StatModified.Text = _vm.LastModifiedShort;
        StatRecovery.Text = _vm.HasRecovery ? "✓" : "—";

        // Forward ViewModel result to DialogResult
        _vm.LoginSucceeded += () => { DialogResult = true; };
        _vm.LoginFailed += msg => { ErrorText.Text = msg; ErrorText.Visibility = Visibility.Visible; };
    }

    /// <summary>Password entered by the user (read by MainViewModel).</summary>
    public string Password => _vm.Password;

    // ── Header drag ───────────────────────────────────────────────
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    // ── Eye toggle ────────────────────────────────────────────────
    private void EyeBtn_Click(object sender, RoutedEventArgs e)
    {
        bool show = PasswordClearBox.Visibility == Visibility.Collapsed;
        if (show)
        {
            PasswordClearBox.Text = PasswordBox.Password;
            PasswordClearBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Collapsed;
        }
        else
        {
            PasswordBox.Password = PasswordClearBox.Text;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordClearBox.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowPasswordChk_Changed(object sender, RoutedEventArgs e)
        => EyeBtn_Click(sender, e);

    // ── Keyboard ──────────────────────────────────────────────────
    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) OpenBtn_Click(sender, e);
    }

    // ── Button handlers ───────────────────────────────────────────
    private void OpenBtn_Click(object sender, RoutedEventArgs e)
    {
        // Sync whichever field is visible
        _vm.Password = PasswordBox.Visibility == Visibility.Visible
            ? PasswordBox.Password
            : PasswordClearBox.Text;

        ErrorText.Visibility = Visibility.Collapsed;
        _vm.ConfirmCommand.Execute(null);
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void ForgotBtn_Click(object sender, RoutedEventArgs e)
    {
        // Open recovery wizard at step 1
        var recovery = new RecoverySetupView();
        recovery.Owner = this;
        recovery.ShowDialog();
    }
}