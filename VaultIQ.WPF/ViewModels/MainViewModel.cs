using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Threading;
using VaultIQ.Core.Entities;
using VaultIQ.Core.Storage;
using VaultIQ.WPF.Views;

namespace VaultIQ.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // ── State ─────────────────────────────────────────────────────
    [ObservableProperty] private VaultDatabase? _database;
    [ObservableProperty] private PasswordGroup? _selectedGroup;
    [ObservableProperty] private PasswordEntry? _selectedEntry;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _windowTitle = "VaultIQ";
    [ObservableProperty] private bool _isLocked = true;
    [ObservableProperty] private bool _isEntrySelected;
    [ObservableProperty] private bool _isDatabaseOpen = false;
    [ObservableProperty] private string _statusText = "Prêt";
    [ObservableProperty] private int _clipboardCountdown;
    [ObservableProperty] private bool _isClipboardCountingDown;
    [ObservableProperty] private bool _favoritesFirst = true;

    // Sort state (Title / Date / Strength)
    private enum SortMode { Title, Date, Strength }
    private SortMode _currentSort = SortMode.Title;

    // Collections
    public ObservableCollection<PasswordGroup> Groups { get; } = [];
    public ObservableCollection<PasswordEntry> Entries { get; } = [];

    /// <summary>Total entrées dans le groupe sélectionné — rebindé par NotifyDatabaseStats.</summary>
    public int TotalEntries => Groups.Sum(g => g.EntryCount);

    /// <summary>Total groupes — rebindé par NotifyDatabaseStats.</summary>
    public int TotalGroups => Groups.Count;

    private string _currentFilePath = string.Empty;
    private string _masterPassword = string.Empty;
    private DispatcherTimer? _clipTimer;
    private DispatcherTimer? _statusTimer;   // bascule StatusText → chemin après 8 s

    // ── Reactive ──────────────────────────────────────────────────
    partial void OnSearchTextChanged(string value) => RefreshEntries();
    partial void OnSelectedGroupChanged(PasswordGroup? value) => RefreshEntries();
    partial void OnSelectedEntryChanged(PasswordEntry? value)
    {
        IsEntrySelected = value is not null;  // propriété bool publique générée par [ObservableProperty]

        EditEntryCommand.NotifyCanExecuteChanged();
        DuplicateEntryCommand.NotifyCanExecuteChanged();
        DeleteEntryCommand.NotifyCanExecuteChanged();
        CopyPasswordCommand.NotifyCanExecuteChanged();
        CopyUsernameCommand.NotifyCanExecuteChanged();
        CopyUrlCommand.NotifyCanExecuteChanged();
        OpenUrlCommand.NotifyCanExecuteChanged();
    }
    partial void OnFavoritesFirstChanged(bool value) => RefreshEntries();

    partial void OnClipboardCountdownChanged(int value)
        => IsClipboardCountingDown = value > 0;

    // ── CanExecute helpers ────────────────────────────────────────
    private bool CanSave() => IsDatabaseOpen && !IsLocked;
    private bool CanEditEntry() => SelectedEntry is not null && !IsLocked;
    private bool CanGroup() => IsDatabaseOpen && !IsLocked;

    // ═════════════════════════════════════════════════════════════
    //  FILE COMMANDS
    // ═════════════════════════════════════════════════════════════

    [RelayCommand]
    private void NewDatabase()
    {
        var dlg = new NewDatabaseDialog();
        if (dlg.ShowDialog() != true) return;

        var db = new VaultDatabase { Name = dlg.DatabaseName };
        db.Groups.Add(new PasswordGroup { Name = "Général", Icon = "🔐" });

        //var saveDlg = new SaveFileDialog
        //{
        //    Title = "Enregistrer la base VaultIQ",
        //    Filter = "Base VaultIQ (*.viq)|*.viq",
        //    FileName = db.Name,
        //    DefaultExt = ".viq"
        //};
        //if (saveDlg.ShowDialog() != true) return;

        VaultFile.Save(db, dlg.FilePath, dlg.MasterPassword);
        LoadIntoMemory(db, dlg.FilePath, dlg.MasterPassword);
        NotifyDatabaseStats();
        SetStatusWithFallback($"Base créée : {db.Name}");
    }

    [RelayCommand]
    private void OpenDatabase()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Ouvrir une base VaultIQ",
            Filter = "Base VaultIQ (*.viq)|*.viq"
        };
        if (dlg.ShowDialog() != true) return;

        var loginDlg = new LoginDialog(VaultFile.ReadPublicName(dlg.FileName));
        if (loginDlg.ShowDialog() != true) return;

        TryOpen(dlg.FileName, loginDlg.Password);
    }

    private void TryOpen(string path, string password)
    {
        try
        {
            var db = VaultFile.Load(path, password);
            LoadIntoMemory(db, path, password);
            NotifyDatabaseStats();
            SetStatusWithFallback($"Ouvert : {db.Name}");
        }
        catch (CryptographicException)
        {
            MessageBox.Show("Mot de passe incorrect.", "VaultIQ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur : {ex.Message}", "VaultIQ",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void SaveDatabase()
    {
        if (Database is null) return;
        VaultFile.Save(Database, _currentFilePath, _masterPassword);
        WindowTitle = $"VaultIQ — {Database.Name}";
        SetStatusWithFallback("Enregistré");
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Lock()
    {
        _masterPassword = string.Empty;
        IsLocked = true;
        SetStatusWithFallback("Base verrouillée");
        WindowTitle = $"VaultIQ — {Database?.Name ?? ""} [Verrouillé]";
    }

    [RelayCommand]
    private void Unlock()
    {
        if (string.IsNullOrEmpty(_currentFilePath) || Database is null) return;
        var dlg = new LoginDialog(Database.Name);
        if (dlg.ShowDialog() != true) return;
        try
        {
            var db = VaultFile.Load(_currentFilePath, dlg.Password);
            LoadIntoMemory(db, _currentFilePath, dlg.Password);
        }
        catch (CryptographicException)
        {
            MessageBox.Show("Mot de passe incorrect.", "VaultIQ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void Exit() => Application.Current.Shutdown();

    // ═════════════════════════════════════════════════════════════
    //  GROUP COMMANDS
    // ═════════════════════════════════════════════════════════════

    [RelayCommand(CanExecute = nameof(CanGroup))]
    private void AddGroup()
    {
        string name = Microsoft.VisualBasic.Interaction.InputBox(
            "Nom du nouveau groupe :", "VaultIQ — Nouveau groupe", "Nouveau groupe");
        if (string.IsNullOrWhiteSpace(name)) return;

        var group = new PasswordGroup { Name = name.Trim(), Icon = "🔐" };
        Database!.Groups.Add(group);
        Groups.Add(group);
        SelectedGroup = group;
        Database.Touch();
        NotifyDatabaseStats();
        AutoSave();
        SetStatusWithFallback($"Groupe créé : {group.Name}");
    }

    private void NotifyDatabaseStats()
    {
        OnPropertyChanged(nameof(TotalEntries));  // force rebind de la carte sidebar
        OnPropertyChanged(nameof(TotalGroups));
        foreach (var g in Groups)
            g.NotifyEntryCountChanged();           // rebind EntryCount dans la ListBox
    }

    [RelayCommand(CanExecute = nameof(CanGroup))]
    private void RenameGroup()
    {
        if (SelectedGroup is null) return;
        string name = Microsoft.VisualBasic.Interaction.InputBox(
            "Nouveau nom du groupe :", "VaultIQ — Renommer",
            SelectedGroup.Name);
        if (string.IsNullOrWhiteSpace(name) || name == SelectedGroup.Name) return;

        SelectedGroup.Name = name.Trim();
        // Force ListBox refresh
        var tmp = SelectedGroup;
        SelectedGroup = null;
        SelectedGroup = tmp;
        Database!.Touch();
        AutoSave();
        SetStatusWithFallback($"Groupe renommé : {name}");
    }

    [RelayCommand(CanExecute = nameof(CanGroup))]
    private void DeleteGroup()
    {
        if (SelectedGroup is null) return;
        if (Groups.Count <= 1)
        {
            MessageBox.Show("Impossible de supprimer le dernier groupe.", "VaultIQ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var res = MessageBox.Show(
            $"Supprimer le groupe « {SelectedGroup.Name} » et ses {SelectedGroup.EntryCount} entrée(s) ?",
            "VaultIQ", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (res != MessageBoxResult.Yes) return;

        Database!.Groups.Remove(SelectedGroup);
        Groups.Remove(SelectedGroup);
        SelectedGroup = Groups.FirstOrDefault();
        Database.Touch();
        NotifyDatabaseStats();
        AutoSave();
        SetStatusWithFallback("Groupe supprimé");
    }

    // ═════════════════════════════════════════════════════════════
    //  ENTRY COMMANDS
    // ═════════════════════════════════════════════════════════════

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void AddEntry()
    {
        if (SelectedGroup is null) return;
        var entry = new PasswordEntry { GroupId = SelectedGroup.Id, Title = "Nouvelle entrée" };
        var dlg = new EntryDialog(entry, Groups.ToList());
        if (dlg.ShowDialog() != true) return;
        SelectedGroup.Entries.Add(entry);
        Database!.Touch();
        RefreshEntries();
        NotifyDatabaseStats();
        AutoSave();
        SetStatusWithFallback($"Entrée ajoutée : {entry.Title}");
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void EditEntry()
    {
        if (SelectedEntry is null) return;
        // Capturer la référence locale AVANT RefreshEntries()
        // qui remet temporairement SelectedEntry à null via DataGrid.Clear()
        var entry = SelectedEntry;
        var dlg = new EntryDialog(entry, Groups.ToList());
        if (dlg.ShowDialog() != true) return;
        entry.Touch();
        Database!.Touch();
        RefreshEntries();          // restaure SelectedEntry via previousId
        NotifyDatabaseStats();
        AutoSave();
        SetStatusWithFallback($"Entrée modifiée : {entry.Title}");
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void DuplicateEntry()
    {
        if (SelectedEntry is null || SelectedGroup is null) return;
        var copy = SelectedEntry.Clone();
        SelectedGroup.Entries.Add(copy);
        Database!.Touch();
        RefreshEntries();
        SelectedEntry = copy;
        NotifyDatabaseStats();
        AutoSave();
        SetStatusWithFallback($"Dupliqué : {copy.Title}");
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void DeleteEntry()
    {
        if (SelectedEntry is null) return;
        var res = MessageBox.Show(
            $"Supprimer « {SelectedEntry.Title} » ?",
            "VaultIQ", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (res != MessageBoxResult.Yes) return;
        var group = Database!.FindGroupOf(SelectedEntry.Id);
        group?.Entries.Remove(SelectedEntry);
        Database.Touch();
        RefreshEntries();
        NotifyDatabaseStats();
        AutoSave();
        SetStatusWithFallback("Entrée supprimée");
    }

    // ═════════════════════════════════════════════════════════════
    //  CLIPBOARD COMMANDS
    // ═════════════════════════════════════════════════════════════

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void CopyPassword()
    {
        if (SelectedEntry is null) return;
        if (string.IsNullOrEmpty(SelectedEntry.Password))
        {
            SetStatusWithFallback("Aucun mot de passe à copier");
            return;
        }
        Clipboard.SetText(SelectedEntry.Password);
        //StartClipboardCountdown(15);
        SetStatusWithFallback("Mot de passe copié");
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void CopyUsername()
    {
        if (SelectedEntry is null) return;
        if (string.IsNullOrEmpty(SelectedEntry.Username))
        {
            SetStatusWithFallback("Aucun identifiant à copier");
            return;
        }
        Clipboard.SetText(SelectedEntry.Username);
        SetStatusWithFallback("Identifiant copié");
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void CopyUrl()
    {
        if (SelectedEntry is null) return;
        if (string.IsNullOrEmpty(SelectedEntry?.Url))
        {
            SetStatusWithFallback("Aucun URL à copier");
            return;
        }
        Clipboard.SetText(SelectedEntry?.Url ?? string.Empty);
        SetStatusWithFallback("URL copiée");
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void OpenUrl()
    {
        if (string.IsNullOrEmpty(SelectedEntry?.Url)) return;
        if (string.IsNullOrWhiteSpace(SelectedEntry?.Url))
        {
            SetStatusWithFallback("Aucun URL à ouvrir");
            return;
        }
        if (!Uri.IsWellFormedUriString(SelectedEntry.Url, UriKind.Absolute)
            || !(SelectedEntry.Url.StartsWith("http://") || SelectedEntry.Url.StartsWith("https://")))
        {
            SetStatusWithFallback("URL invalide");
            return;
        }
        Process.Start(new ProcessStartInfo(SelectedEntry.Url) { UseShellExecute = true });
    }

    // ═════════════════════════════════════════════════════════════
    //  SORT / DISPLAY COMMANDS
    // ═════════════════════════════════════════════════════════════

    [RelayCommand]
    private void SortByTitle()
    {
        _currentSort = SortMode.Title;
        RefreshEntries();
        SetStatusWithFallback("Trié par titre");
    }

    [RelayCommand]
    private void SortByDate()
    {
        _currentSort = SortMode.Date;
        RefreshEntries();
        SetStatusWithFallback("Trié par date de modification");
    }

    [RelayCommand]
    private void SortByStrength()
    {
        _currentSort = SortMode.Strength;
        RefreshEntries();
        SetStatusWithFallback("Trié par force du mot de passe");
    }

    [RelayCommand]
    private void ToggleFavoritesFirst()
    {
        FavoritesFirst = !FavoritesFirst;
        RefreshEntries();
    }

    // ═════════════════════════════════════════════════════════════
    //  TOOLS COMMANDS
    // ═════════════════════════════════════════════════════════════

    [RelayCommand]
    private void OpenGenerator()
    {
        var dlg = new GeneratorDialog();
        dlg.ShowDialog();
    }

    [RelayCommand]
    private void FocusSearch()
    {
        // The code-behind's SearchBox.Focus() is called via the view
        // When triggered via Ctrl+F KeyBinding the Window handles it directly.
        // This relay exists so the Menu item can also bind to it.
    }

    [RelayCommand]
    private void OpenOptions()
    {
        MessageBox.Show("Options — disponible en v2.", "VaultIQ",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void OpenDoc()
    {
        Process.Start(new ProcessStartInfo("https://github.com/vaultiq/vaultiq")
        { UseShellExecute = true });
    }

    [RelayCommand]
    private void About()
    {
        MessageBox.Show(
            "VaultIQ v1.0\n.NET 8.0 · WPF · AES-256-CBC · PBKDF2 150k\n\nGPL v3 — 2025 VaultIQ Contributors",
            "À propos de VaultIQ", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ═════════════════════════════════════════════════════════════
    //  INTERNAL HELPERS
    // ═════════════════════════════════════════════════════════════

    private void LoadIntoMemory(VaultDatabase db, string path, string password)
    {
        Database = db;
        _currentFilePath = path;
        _masterPassword = password;
        IsLocked = false;
        IsDatabaseOpen = true;
        WindowTitle = $"VaultIQ — {db.Name}";

        Groups.Clear();
        foreach (var g in db.Groups) Groups.Add(g);
        SelectedGroup = Groups.FirstOrDefault();

        // Notify CanExecute for all commands
        SaveDatabaseCommand.NotifyCanExecuteChanged();
        LockCommand.NotifyCanExecuteChanged();
        AddEntryCommand.NotifyCanExecuteChanged();
        AddGroupCommand.NotifyCanExecuteChanged();
        RenameGroupCommand.NotifyCanExecuteChanged();
        DeleteGroupCommand.NotifyCanExecuteChanged();
    }

    private void RefreshEntries()
    {
        Entries.Clear();
        if (SelectedGroup is null) return;

        IEnumerable<PasswordEntry> source = SelectedGroup.Entries;

        // Text filter
        if (!string.IsNullOrWhiteSpace(SearchText))
            source = source.Where(e =>
                e.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                e.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                e.Url.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                e.Notes.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        // Sort
        IOrderedEnumerable<PasswordEntry> sorted = _currentSort switch
        {
            SortMode.Date => source.OrderByDescending(e => e.ModifiedAt),
            SortMode.Strength => source.OrderByDescending(e => e.StrengthScore),
            _ => source.OrderBy(e => e.Title)
        };

        // Favorites first (secondary sort)
        IEnumerable<PasswordEntry> final = FavoritesFirst
            ? sorted.ThenByDescending(e => e.IsFavorite)   // already ordered, boost favorites
            : sorted;

        // Re-apply favorites as primary when enabled
        if (FavoritesFirst)
        {
            var filtered = source.Where(e => string.IsNullOrWhiteSpace(SearchText)
                || e.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || e.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

            switch (_currentSort)
            {
                case SortMode.Date:
                    final = filtered
                        .OrderByDescending(e => e.IsFavorite)
                        .ThenByDescending(e => e.ModifiedAt);
                    break;

                case SortMode.Strength:
                    final = filtered
                        .OrderByDescending(e => e.IsFavorite)
                        .ThenByDescending(e => e.Strength);
                    break;

                default:
                    final = filtered
                        .OrderByDescending(e => e.IsFavorite)
                        .ThenBy(e => e.Title);
                    break;
            }
        }

        foreach (var e in final) Entries.Add(e);
    }

    private void AutoSave()
    {
        if (!string.IsNullOrEmpty(_currentFilePath) && Database is not null)
            VaultFile.Save(Database, _currentFilePath, _masterPassword);
    }

    private void StartClipboardCountdown(int seconds)
    {
        _clipTimer?.Stop();
        ClipboardCountdown = seconds;

        _clipTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clipTimer.Tick += (_, _) =>
        {
            ClipboardCountdown--;
            if (ClipboardCountdown <= 0)
            {
                _clipTimer!.Stop();
                Clipboard.Clear();
                ClipboardCountdown = 0;
                SetStatusWithFallback("Presse-papier effacé");
            }
        };
        _clipTimer.Start();
    }

    public void OpenFileOnStartup(string filePath)
    {
        var dlg = new LoginDialog(VaultFile.ReadPublicName(filePath));
        if (dlg.ShowDialog() == true)
            TryOpen(filePath, dlg.Password);
    }

    private void SetStatusWithFallback(string message, int delaySeconds = 8)
    {
        StatusText = message;
        _statusTimer?.Stop();          // annule le timer précédent si on re-clique vite
        _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(delaySeconds) };
        _statusTimer.Tick += (_, _) =>
        {
            _statusTimer.Stop();
            StatusText = !string.IsNullOrEmpty(_currentFilePath)
                ? _currentFilePath     // ← affiche le chemin complet
                : "Prêt";
        };
        _statusTimer.Start();
    }
}