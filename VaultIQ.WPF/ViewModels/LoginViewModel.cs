using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace VaultIQ.WPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    // ── Public metadata (read from .viq header, no password needed) ──
    public string DatabaseName { get; }

    public int EntryCount { get; }
    public int GroupCount { get; }
    public string LastOpened { get; }
    public string LastModifiedShort { get; }
    public bool HasRecovery { get; }

    // ── Bindable state ────────────────────────────────────────────
    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    // ── Events forwarded to the code-behind ──────────────────────
    public event Action? LoginSucceeded;

    public event Action<string>? LoginFailed;

    public LoginViewModel(string databaseName,
                          int entryCount = 0,
                          int groupCount = 0,
                          string lastOpened = "—",
                          string lastModifiedShort = "—",
                          bool hasRecovery = false)
    {
        DatabaseName = databaseName;
        EntryCount = entryCount;
        GroupCount = groupCount;
        LastOpened = lastOpened;
        LastModifiedShort = lastModifiedShort;
        HasRecovery = hasRecovery;
    }

    // ── Commands ──────────────────────────────────────────────────
    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        if (string.IsNullOrEmpty(Password))
        {
            LoginFailed?.Invoke("Veuillez saisir le mot de passe.");
            return;
        }
        // Actual decryption is attempted in the caller (MainViewModel / LoginDialog)
        // which reads the Password property directly after DialogResult = true.
        LoginSucceeded?.Invoke();
    }

    private bool CanConfirm() => !IsBusy;

    [RelayCommand]
    private void Cancel()
    { /* DialogResult = false handled in code-behind */ }
}