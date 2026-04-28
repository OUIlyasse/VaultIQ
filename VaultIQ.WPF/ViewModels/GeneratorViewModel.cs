using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VaultIQ.Core.Crypto;

namespace VaultIQ.WPF.ViewModels;

public partial class GeneratorViewModel : ObservableObject
{
    // ── Options ───────────────────────────────────────────────────
    [ObservableProperty] private int _length = 20;
    [ObservableProperty] private bool _useUppercase = true;
    [ObservableProperty] private bool _useLowercase = true;
    [ObservableProperty] private bool _useDigits = true;
    [ObservableProperty] private bool _useSymbols = false;
    [ObservableProperty] private bool _noAmbiguous = true;

    // ── Output ────────────────────────────────────────────────────
    /// <summary>The most recently generated password. Read by caller after DialogResult = true.</summary>
    [ObservableProperty] private string _generatedPassword = string.Empty;
    [ObservableProperty] private string _entropyLabel = string.Empty;
    [ObservableProperty] private int _entropyBits;

    // ── History (last 5, newest first) ───────────────────────────
    private readonly List<string> _history = [];
    public IReadOnlyList<string> History => _history;

    // ── Regenerate ────────────────────────────────────────────────
    [RelayCommand]
    public void Regenerate()
    {
        // Guard: at least one character type must be selected
        if (!UseUppercase && !UseLowercase && !UseDigits && !UseSymbols)
            UseDigits = true;

        string pwd = VaultCrypto.GeneratePassword(
            length: Length,
            uppercase: UseUppercase,
            lowercase: UseLowercase,
            digits: UseDigits,
            symbols: UseSymbols,
            noAmbiguous: NoAmbiguous);

        GeneratedPassword = pwd;

        // Entropy calculation — pool size
        int pool = 0;
        if (UseUppercase) pool += NoAmbiguous ? 23 : 26;
        if (UseLowercase) pool += NoAmbiguous ? 24 : 26;
        if (UseDigits) pool += NoAmbiguous ? 8 : 10;
        if (UseSymbols) pool += 32;
        pool = Math.Max(pool, 2);

        EntropyBits = (int)(Length * Math.Log2(pool));
        EntropyLabel = $"Entropie ≈ {EntropyBits} bits";

        // History — keep last 5, avoid duplicates
        if (_history.Count == 0 || _history[0] != pwd)
        {
            _history.Insert(0, pwd);
            if (_history.Count > 5) _history.RemoveAt(5);
            OnPropertyChanged(nameof(History));
        }
    }
}