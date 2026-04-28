using CommunityToolkit.Mvvm.ComponentModel;
using System.Security.Policy;
using VaultIQ.Core.Crypto;
using VaultIQ.Core.Entities;

namespace VaultIQ.WPF.ViewModels;

public partial class EntryDialogViewModel : ObservableObject
{
    // ── Entry being edited (direct reference — spec says "modifie directement") ──
    public PasswordEntry Entry { get; }

    // ── Groups ────────────────────────────────────────────────────
    public List<PasswordGroup> Groups { get; }

    [ObservableProperty] private PasswordGroup? _selectedGroup;

    // ── Bound fields (mirroring Entry properties for live validation) ──
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _url = string.Empty;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private bool _isFavorite;

    // ── Strength (computed) ───────────────────────────────────────
    [ObservableProperty] private int _strengthScore;
    [ObservableProperty] private string _strengthLabel = string.Empty;
    [ObservableProperty] private string _strengthColor = "#606080";

    partial void OnPasswordChanged(string value)
    {
        StrengthScore = PasswordStrengthHelper.Evaluate(value);
        StrengthLabel = PasswordStrengthHelper.Label(StrengthScore);
        StrengthColor = PasswordStrengthHelper.Color(StrengthScore);
    }

    // ── Generator options ─────────────────────────────────────────
    [ObservableProperty] private int _generatorLength = 20;
    [ObservableProperty] private bool _useUppercase = true;
    [ObservableProperty] private bool _useLowercase = true;
    [ObservableProperty] private bool _useDigits = true;
    [ObservableProperty] private bool _useSymbols = false;
    [ObservableProperty] private bool _noAmbiguous = true;
    [ObservableProperty] private string _generatedPreview = string.Empty;
    [ObservableProperty] private string _entropyLabel = string.Empty;

    public EntryDialogViewModel(PasswordEntry entry, List<PasswordGroup> groups)
    {
        Entry = entry;
        Groups = groups;

        // Mirror entry fields for binding
        Title = entry.Title;
        Username = entry.Username;
        Password = entry.Password;
        Url = entry.Url;
        Notes = entry.Notes;
        IsFavorite = entry.IsFavorite;

        SelectedGroup = groups.FirstOrDefault(g => g.Id == entry.GroupId)
                     ?? groups.FirstOrDefault();
    }

    // ── Generator ─────────────────────────────────────────────────
    public void Regenerate()
    {
        GeneratedPreview = VaultCrypto.GeneratePassword(
            length: GeneratorLength,
            uppercase: UseUppercase,
            lowercase: UseLowercase,
            digits: UseDigits,
            symbols: UseSymbols,
            noAmbiguous: NoAmbiguous);

        // Entropy = length × log2(pool size)
        int pool = 0;
        if (UseUppercase) pool += NoAmbiguous ? 23 : 26;   // minus O,I,l if noAmbiguous
        if (UseLowercase) pool += NoAmbiguous ? 24 : 26;
        if (UseDigits) pool += NoAmbiguous ? 8 : 10;
        if (UseSymbols) pool += 32;
        pool = Math.Max(pool, 2);

        int entropy = (int)(GeneratorLength * Math.Log2(pool));
        EntropyLabel = $"Entropie ≈ {entropy} bits";
    }
}