using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VaultIQ.Core.Entities;
using VaultIQ.WPF.ViewModels;

namespace VaultIQ.WPF.Views;

public partial class EntryDialog : Window
{
    // Déclaré null! — garanti non-null avant InitializeComponent()
    private EntryDialogViewModel _vm = null!;
    private bool _isNewEntry;

    public EntryDialog(PasswordEntry entry, IEnumerable<PasswordGroup> groups)
    {
        // ═══════════════════════════════════════════════════════════
        //  ORDRE CRITIQUE :
        //  1. _vm AVANT InitializeComponent()
        //     → InitializeComponent() déclenche les handlers XAML
        //       (Slider.ValueChanged, CheckBox.Checked, ComboBox.SelectionChanged…)
        //       Si _vm est null à ce moment, NullReferenceException.
        // ═══════════════════════════════════════════════════════════
        _isNewEntry = entry.Title == "Nouvelle entrée" && entry.Username == string.Empty;
        _vm = new EntryDialogViewModel(entry, groups.ToList());

        InitializeComponent();
        DataContext = _vm;

        // ── Abonnement tardif des events générateur ───────────────
        // Ces controls ont IsChecked/Value définis dans le XAML.
        // En s'abonnant ICI (après InitializeComponent), on évite que
        // Checked/ValueChanged se déclenche avant que les autres
        // contrôles du même handler soient initialisés.
        GenLengthSlider.ValueChanged += GenSlider_Changed;
        ChkUppercase.Checked += GenOption_Changed;
        ChkUppercase.Unchecked += GenOption_Changed;
        ChkLowercase.Checked += GenOption_Changed;
        ChkLowercase.Unchecked += GenOption_Changed;
        ChkDigits.Checked += GenOption_Changed;
        ChkDigits.Unchecked += GenOption_Changed;
        ChkSymbols.Checked += GenOption_Changed;
        ChkSymbols.Unchecked += GenOption_Changed;
        ChkNoAmbiguous.Checked += GenOption_Changed;
        ChkNoAmbiguous.Unchecked += GenOption_Changed;

        // ── Remplissage initial des champs UI ─────────────────────
        DialogTitle.Text = _isNewEntry ? "➕ Nouvelle entrée" : "✏️ Modifier l'entrée";
        DialogSubtitle.Text = _isNewEntry
            ? "Remplir les champs ci-dessous"
            : $"Groupe : {_vm.SelectedGroup?.Name ?? "—"}";

        FavoriteToggle.IsChecked = entry.IsFavorite;  // image switche via ControlTemplate.Triggers

        GroupCombo.ItemsSource = _vm.Groups;
        GroupCombo.DisplayMemberPath = "Name";
        GroupCombo.SelectedItem = _vm.SelectedGroup;

        PasswordBox.Password = entry.Password;

        if (!_isNewEntry)
        {
            MetaPanel.Visibility = Visibility.Visible;
            MetaCreated.Text = entry.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            MetaModified.Text = entry.ModifiedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        }

        // ── Barre de force + générateur initial ───────────────────
        // Utiliser Loaded car ActualWidth == 0 avant le premier rendu
        Loaded += (_, _) =>
        {
            UpdateStrengthBar(entry.Password);
            _vm.Regenerate();
            GenLengthValue.Text = ((int)GenLengthSlider.Value).ToString();
        };

        // ── Notifications VM → UI ─────────────────────────────────
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(EntryDialogViewModel.GeneratedPreview))
                GenPreview.Text = _vm.GeneratedPreview;
            if (e.PropertyName == nameof(EntryDialogViewModel.EntropyLabel))
                GenEntropyLabel.Text = _vm.EntropyLabel;
        };
    }

    // ── Header drag ───────────────────────────────────────────────
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        => DragMove();

    // ── Favori ────────────────────────────────────────────────────
    // L'image étoile est gérée par ControlTemplate.Triggers sur IsChecked.
    // Ce handler existe uniquement si une logique supplémentaire est nécessaire.
    private void FavoriteToggle_Changed(object sender, RoutedEventArgs e)
    {
        // Rien à faire : le template XAML switche automatiquement
        // star_empty_18.png → star_18.png selon IsChecked.
    }

    // ── Toggle œil ───────────────────────────────────────────────
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

    // ── Barre de force ────────────────────────────────────────────
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        => UpdateStrengthBar(PasswordBox.Password);

    private void PasswordClearBox_TextChanged(object sender,
        System.Windows.Controls.TextChangedEventArgs e)
        => UpdateStrengthBar(PasswordClearBox.Text);

    private void UpdateStrengthBar(string password)
    {
        int score = PasswordStrengthHelper.Evaluate(password);
        string label = PasswordStrengthHelper.Label(score);
        string hex = PasswordStrengthHelper.Color(score);
        var brush = new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(hex));

        double ratio = Math.Clamp(score / 100.0, 0, 1);
        StrengthFill.Width = StrengthContainer.ActualWidth * ratio;
        StrengthFill.Background = brush;
        StrengthLabel.Text = label;
        StrengthLabel.Foreground = brush;
        StrengthScore.Text = score > 0 ? $"{score}/100" : string.Empty;
        FooterStrength.Text = score > 0 ? $"Force : {label} — {score}/100" : string.Empty;
    }

    // ── Combo groupe — garde null pour InitializeComponent() ──────
    private void GroupCombo_SelectionChanged(object sender,
        System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_vm is null) return;
        if (GroupCombo.SelectedItem is PasswordGroup g)
            _vm.SelectedGroup = g;
    }

    // ── Copier ────────────────────────────────────────────────────
    private void CopyUsernameBtn_Click(object sender, RoutedEventArgs e)
        => Clipboard.SetText(UsernameBox.Text);

    private void CopyPasswordBtn_Click(object sender, RoutedEventArgs e)
    {
        string pwd = PasswordBox.Visibility == Visibility.Visible
            ? PasswordBox.Password
            : PasswordClearBox.Text;
        Clipboard.SetText(pwd);
    }

    // ── Ouvrir URL ────────────────────────────────────────────────
    private void OpenUrlBtn_Click(object sender, RoutedEventArgs e)
    {
        string url = UrlBox.Text.Trim();
        if (string.IsNullOrEmpty(url)) return;
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url;
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    // ── Panneau générateur intégré ────────────────────────────────
    private void GenerateBtn_Click(object sender, RoutedEventArgs e)
    {
        bool opening = GeneratorPanel.Visibility != Visibility.Visible;
        GeneratorPanel.Visibility = opening ? Visibility.Visible : Visibility.Collapsed;
        MetaPanel.Visibility = (!opening && !_isNewEntry)
            ? Visibility.Visible : Visibility.Collapsed;
        if (opening) _vm.Regenerate();
    }

    private void CloseGeneratorPanel_Click(object sender, RoutedEventArgs e)
    {
        GeneratorPanel.Visibility = Visibility.Collapsed;
        MetaPanel.Visibility = _isNewEntry ? Visibility.Collapsed : Visibility.Visible;
    }

    private void GenSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // _vm peut être null si le slider émet ValueChanged pendant InitializeComponent()
        if (_vm is null || GenLengthValue is null) return;
        int len = (int)GenLengthSlider.Value;
        GenLengthValue.Text = len.ToString();
        _vm.GeneratorLength = len;
        _vm.Regenerate();
    }

    private void GenOption_Changed(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        _vm.UseUppercase = ChkUppercase.IsChecked == true;
        _vm.UseLowercase = ChkLowercase.IsChecked == true;
        _vm.UseDigits = ChkDigits.IsChecked == true;
        _vm.UseSymbols = ChkSymbols.IsChecked == true;
        _vm.NoAmbiguous = ChkNoAmbiguous.IsChecked == true;
        _vm.Regenerate();
    }

    private void RegenerateBtn_Click(object sender, RoutedEventArgs e)
        => _vm?.Regenerate();

    private void UseGeneratedBtn_Click(object sender, RoutedEventArgs e)
    {
        PasswordBox.Password = _vm.GeneratedPreview;
        PasswordClearBox.Text = _vm.GeneratedPreview;
        PasswordBox.Visibility = Visibility.Visible;
        PasswordClearBox.Visibility = Visibility.Collapsed;
        UpdateStrengthBar(_vm.GeneratedPreview);
        GeneratorPanel.Visibility = Visibility.Collapsed;
        MetaPanel.Visibility = _isNewEntry ? Visibility.Collapsed : Visibility.Visible;
    }

    // ── Enregistrer ───────────────────────────────────────────────
    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleBox.Text))
        {
            MessageBox.Show("Le titre est obligatoire.", "VaultIQ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TitleBox.Focus();
            return;
        }

        _vm.Entry.Title = TitleBox.Text.Trim();
        _vm.Entry.Username = UsernameBox.Text.Trim();
        _vm.Entry.Password = PasswordBox.Visibility == Visibility.Visible
            ? PasswordBox.Password : PasswordClearBox.Text;
        _vm.Entry.Url = UrlBox.Text.Trim();
        _vm.Entry.Notes = NotesBox.Text;
        _vm.Entry.IsFavorite = FavoriteToggle.IsChecked == true;
        _vm.Entry.GroupId = (_vm.SelectedGroup ?? _vm.Groups.FirstOrDefault())?.Id
                               ?? _vm.Entry.GroupId;
        _vm.Entry.Touch();

        DialogResult = true;
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;

    // ── Resize → mettre à jour la barre de force ──────────────────
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        if (_vm is null) return;
        string pwd = PasswordBox.Visibility == Visibility.Visible
            ? PasswordBox.Password : PasswordClearBox.Text;
        UpdateStrengthBar(pwd);
    }
}
