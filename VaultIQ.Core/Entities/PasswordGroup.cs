using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VaultIQ.Core.Entities;

/// <summary>
/// PasswordGroup implémente INotifyPropertyChanged pour permettre
/// à la sidebar (ListBox + carte infos) de se mettre à jour quand
/// le nombre d'entrées change.
/// </summary>
public partial class PasswordGroup : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ── Identity ──────────────────────────────────────────────────
    public Guid Id { get; set; } = Guid.NewGuid();

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    private string _icon = "🔐";
    public string Icon
    {
        get => _icon;
        set { _icon = value; OnPropertyChanged(); }
    }

    // ── Entries ───────────────────────────────────────────────────
    public ObservableCollection<PasswordEntry> Entries { get; set; } = [];

    /// <summary>
    /// Nombre d'entrées — propriété calculée exposée pour le binding XAML.
    /// Appeler NotifyEntryCountChanged() après Add / Remove.
    /// </summary>
    public int EntryCount => Entries.Count;

    /// <summary>
    /// Force le rebind de EntryCount dans la ListBox sidebar et la carte infos.
    /// Appelé par MainViewModel.NotifyDatabaseStats() après toute mutation.
    /// </summary>
    public void NotifyEntryCountChanged()
        => OnPropertyChanged(nameof(EntryCount));
}