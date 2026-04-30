using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using VaultIQ.Core.Entities;

namespace VaultIQ.WPF.ViewModels;

public partial class NewDatabaseViewModel : ObservableObject
{
    [ObservableProperty] private string _databaseName = "MesComptes";
    [ObservableProperty] private string _masterPassword = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;
    [ObservableProperty] private string _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VaultIQ", "MesComptes.viq");
    [ObservableProperty] private bool _enablePin = true;
    [ObservableProperty] private bool _enableRecovery = true;
    [ObservableProperty] private int _passwordStrength;
    [ObservableProperty] private int _entropyBits;
    [ObservableProperty] private bool _passwordsMatch;

    // Auto-fill file path when name changes
    partial void OnDatabaseNameChanged(string value)
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        FilePath = Path.Combine(docs, "VaultIQ", $"{value}.viq");
    }

    partial void OnMasterPasswordChanged(string value) => UpdateStrength();

    partial void OnConfirmPasswordChanged(string value)
      => PasswordsMatch = !string.IsNullOrEmpty(value) && value == MasterPassword;

    // ── Strength calculation ──────────────────────────────────────
    public void UpdateStrength()
    {
        var pwd = MasterPassword;
        if (string.IsNullOrEmpty(pwd))
        {
            PasswordStrength = 0;
            EntropyBits = 0;
            return;
        }

        int pool = 0;

        if (pwd.Any(char.IsLower)) { pool += 26; }
        if (pwd.Any(char.IsUpper)) { pool += 26; }
        if (pwd.Any(char.IsDigit)) { pool += 10; }
        if (pwd.Any(c => !char.IsLetterOrDigit(c))) { pool += 32; }

        PasswordStrength = PasswordStrengthHelper.Evaluate(pwd);
        EntropyBits = pool > 0
            ? (int)(pwd.Length * Math.Log2(pool))
            : 0;

        PasswordsMatch = !string.IsNullOrEmpty(ConfirmPassword)
                         && ConfirmPassword == pwd;
    }

    // ── Validation ────────────────────────────────────────────────
    public bool Validate(out string error)
    {
        if (string.IsNullOrWhiteSpace(DatabaseName))
        { error = "Veuillez saisir un nom pour la base."; return false; }
        if (MasterPassword.Length < 4)
        { error = "Le mot de passe doit contenir au moins 4 caractères."; return false; }
        if (MasterPassword != ConfirmPassword)
        { error = "Les mots de passe ne correspondent pas."; return false; }
        if (string.IsNullOrWhiteSpace(FilePath))
        { error = "Veuillez choisir un emplacement pour le fichier."; return false; }
        error = string.Empty;
        return true;
    }
}