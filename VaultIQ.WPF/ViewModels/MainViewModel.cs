using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Threading;
using VaultIQ.Core.Entities;
using VaultIQ.Core.Storage;

namespace VaultIQ.WPF.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // ── State ─────────────────────────────────────────────────────
    [ObservableProperty]
    private VaultDatabase? _database;

    [ObservableProperty]
    private PasswordGroup? _selectedGroup;

    [ObservableProperty]
    private PasswordEntry? _selectedEntry;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _windowTitle = "VaultIQ";

    [ObservableProperty]
    private bool _isLocked = true;

    [ObservableProperty]
    private bool _isDatabaseOpen;

    [ObservableProperty]
    private string _statusText = "Prêt";

    [ObservableProperty]
    private int _clipboardCountdown;

    // Collections affichées
    public ObservableCollection<PasswordGroup> Groups { get; } = [];

    public ObservableCollection<PasswordEntry> Entries { get; } = [];

    private string _currentFilePath = string.Empty;
    private string _masterPassword = string.Empty; // effacé au verrouillage
    private DispatcherTimer? _clipTimer;

    // ── Search reactive ───────────────────────────────────────────
    partial void OnSearchTextChanged(string value) => RefreshEntries();

    partial void OnSelectedGroupChanged(PasswordGroup? value) => RefreshEntries();

    // ── File commands ─────────────────────────────────────────────
    [RelayCommand]
    private void NewDatabase()
    {
        var dlg = new Views.NewDatabaseDialog();
        if (dlg.ShowDialog() != true) return;

        var db = new VaultDatabase { Name = dlg.DatabaseName };
        db.Groups.Add(new PasswordGroup { Name = "Général", IconPath = "" });

        var saveDlg = new SaveFileDialog
        {
            Title = "Enregistrer la base VaultIQ",
            Filter = "Base VaultIQ (*.viq)|*.viq",
            FileName = db.Name,
            DefaultExt = ".viq"
        };
        if (saveDlg.ShowDialog() != true) return;

        VaultFile.Save(db, saveDlg.FileName, dlg.MasterPassword);
        LoadIntoMemory(db, saveDlg.FileName, dlg.MasterPassword);
        StatusText = $"Base créée : {db.Name}";
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

        var loginDlg = new Views.LoginDialog(VaultFile.ReadPublicName(dlg.FileName));
        if (loginDlg.ShowDialog() != true) return;

        TryOpen(dlg.FileName, loginDlg.Password);
    }

    private void TryOpen(string path, string password)
    {
        try
        {
            var db = VaultFile.Load(path, password);
            LoadIntoMemory(db, path, password);
            StatusText = $"Ouvert : {db.Name}";
        }
        catch (CryptographicException)
        {
            MessageBox.Show("Mot de passe incorrect.", "VaultIQ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur : {ex.Message}", "VaultIQ", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void SaveDatabase()
    {
        if (Database is null) return;
        VaultFile.Save(Database, _currentFilePath, _masterPassword);
        WindowTitle = $"VaultIQ — {Database.Name}";
        StatusText = "Enregistré";
    }

    private bool CanSave() => IsDatabaseOpen && !IsLocked;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Lock()
    {
        _masterPassword = string.Empty;
        IsLocked = true;
        StatusText = "Base verrouillée";
        WindowTitle = $"VaultIQ — {Database?.Name ?? ""} [Verrouillé]";
    }

    [RelayCommand(CanExecute = nameof(IsLocked))]
    private void Unlock()
    {
        if (_currentFilePath == string.Empty || Database is null) return;
        var dlg = new Views.LoginDialog(Database.Name);
        if (dlg.ShowDialog() != true) return;
        try
        {
            var db = VaultFile.Load(_currentFilePath, dlg.Password);
            LoadIntoMemory(db, _currentFilePath, dlg.Password);
        }
        catch (CryptographicException)
        {
            MessageBox.Show("Mot de passe incorrect.", "VaultIQ", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ── Entry commands ────────────────────────────────────────────
    [RelayCommand(CanExecute = nameof(CanSave))]
    private void AddEntry()
    {
        if (SelectedGroup is null) return;
        var entry = new PasswordEntry { GroupId = SelectedGroup.Id, Title = "Nouvelle entrée" };
        var dlg = new Views.EntryDialog(entry, Groups.ToList());
        if (dlg.ShowDialog() != true) return;
        SelectedGroup.Entries.Add(entry);
        Database!.Touch();
        RefreshEntries();
        AutoSave();
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void EditEntry()
    {
        if (SelectedEntry is null || SelectedGroup is null) return;
        var dlg = new Views.EntryDialog(SelectedEntry, Groups.ToList());
        dlg.ShowDialog();
        SelectedEntry.Touch();
        Database!.Touch();
        RefreshEntries();
        AutoSave();
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void DeleteEntry()
    {
        if (SelectedEntry is null) return;
        var result = MessageBox.Show(
            $"Supprimer « {SelectedEntry.Title} » ?",
            "VaultIQ", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;
        var group = Database!.FindGroupOf(SelectedEntry.Id);
        group?.Entries.Remove(SelectedEntry);
        Database.Touch();
        RefreshEntries();
        AutoSave();
    }

    private bool CanEditEntry() => SelectedEntry is not null && !IsLocked;

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void CopyPassword()
    {
        if (SelectedEntry is null) return;
        Clipboard.SetText(SelectedEntry.Password);
        StartClipboardCountdown(15);
        StatusText = "Mot de passe copié — effacement dans 15 s";
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void CopyUsername()
    {
        if (SelectedEntry is null) return;
        Clipboard.SetText(SelectedEntry.Username);
        StatusText = "Identifiant copié";
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void CopyUrl()
    {
        if (SelectedEntry is null) return;
        Clipboard.SetText(SelectedEntry.Url);
        StatusText = "URL copié";
    }

    [RelayCommand(CanExecute = nameof(CanEditEntry))]
    private void OpenUrl()
    {
        if (string.IsNullOrEmpty(SelectedEntry?.Url)) return;
        System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo(SelectedEntry.Url) { UseShellExecute = true });
    }

    // ── Password Generator ────────────────────────────────────────
    [RelayCommand]
    private void OpenGenerator()
    {
        var dlg = new Views.GeneratorDialog();
        dlg.ShowDialog();
    }

    // ── Internal helpers ──────────────────────────────────────────
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
    }

    private void RefreshEntries()
    {
        Entries.Clear();
        if (SelectedGroup is null) return;
        var source = string.IsNullOrWhiteSpace(SearchText)
            ? SelectedGroup.Entries
            : SelectedGroup.Entries.Where(e =>
                e.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                e.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        foreach (var e in source.OrderByDescending(e => e.IsFavorite).ThenBy(e => e.Title))
            Entries.Add(e);
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
        _clipTimer = new System.Windows.Threading.DispatcherTimer
        { Interval = TimeSpan.FromSeconds(1) };
        _clipTimer.Tick += (_, _) =>
        {
            ClipboardCountdown--;
            if (ClipboardCountdown <= 0)
            {
                _clipTimer.Stop();
                Clipboard.Clear();
                StatusText = "Presse-papier effacé";
            }
        };
        _clipTimer.Start();
    }

    public void OpenFileOnStartup(string filePath)
    {
        var dlg = new Views.LoginDialog(VaultFile.ReadPublicName(filePath));
        if (dlg.ShowDialog() == true)
            TryOpen(filePath, dlg.Password);
    }
}