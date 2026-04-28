using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VaultIQ.Core.Entities;
using VaultIQ.WPF.ViewModels;

namespace VaultIQ.WPF.Views;

public partial class EntryDialog : Window
{
    private readonly EntryDialogViewModel _vm;
    private bool _isNewEntry;

    /// <summary>
    /// Edit an existing entry in-place (EntryDialog modifies PasswordEntry directly per spec).
    /// </summary>
    public EntryDialog(PasswordEntry entry, IEnumerable<PasswordGroup> groups)
    {
        InitializeComponent();
        _isNewEntry = entry.Title == "Nouvelle entrée" && entry.Username == string.Empty;
        _vm = new EntryDialogViewModel(entry, groups.ToList());
        DataContext = _vm;

        // Header
        DialogTitle.Text = _isNewEntry ? "➕ Nouvelle entrée" : "✏️ Modifier l'entrée";
        DialogSubtitle.Text = _isNewEntry ? "Remplir les champs ci-dessous"
                                          : $"Groupe : {_vm.SelectedGroup?.Name ?? "—"}";

        // Favorite toggle
        FavoriteToggle.IsChecked = entry.IsFavorite;
        FavoriteToggle.Content = entry.IsFavorite ? "★" : "☆";

        // Groups
        GroupCombo.ItemsSource = _vm.Groups;
        GroupCombo.DisplayMemberPath = "Name";
        GroupCombo.SelectedItem = _vm.SelectedGroup;

        // Password field
        PasswordBox.Password = entry.Password;

        // Metadata (edit mode only)
        if (!_isNewEntry)
        {
            MetaPanel.Visibility = Visibility.Visible;
            MetaCreated.Text = entry.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            MetaModified.Text = entry.ModifiedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        }

        // Initial strength bar
        UpdateStrengthBar(entry.Password);

        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(EntryDialogViewModel.GeneratedPreview))
                GenPreview.Text = _vm.GeneratedPreview;
            if (e.PropertyName == nameof(EntryDialogViewModel.EntropyLabel))
                GenEntropyLabel.Text = _vm.EntropyLabel;
        };

        // Initial generator preview
        _vm.Regenerate();
        GenLengthValue.Text = ((int)GenLengthSlider.Value).ToString();
    }

    // ── Header drag ───────────────────────────────────────────────
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    // ── Favorite toggle ───────────────────────────────────────────
    private void FavoriteToggle_Changed(object sender, RoutedEventArgs e)
        => FavoriteToggle.Content = FavoriteToggle.IsChecked == true ? "★" : "☆";

    // ── Password field toggle (eye) ───────────────────────────────
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

    // ── Password strength ─────────────────────────────────────────
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        => UpdateStrengthBar(PasswordBox.Password);

    private void PasswordClearBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        => UpdateStrengthBar(PasswordClearBox.Text);

    private void UpdateStrengthBar(string password)
    {
        int score = PasswordStrengthHelper.Evaluate(password);
        string label = PasswordStrengthHelper.Label(score);
        string hex = PasswordStrengthHelper.Color(score);
        var brush = new SolidColorBrush((System.Windows.Media.Color)
                       System.Windows.Media.ColorConverter.ConvertFromString(hex));

        double ratio = Math.Clamp(score / 100.0, 0, 1);
        StrengthFill.Width = StrengthContainer.ActualWidth * ratio;
        StrengthFill.Background = brush;
        StrengthLabel.Text = label;
        StrengthLabel.Foreground = brush;
        StrengthScore.Text = score > 0 ? $"{score}/100" : "";
        FooterStrength.Text = score > 0 ? $"Force : {label} — {score}/100" : "";
    }

    // ── Group combo ───────────────────────────────────────────────
    private void GroupCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (GroupCombo.SelectedItem is PasswordGroup g)
            _vm.SelectedGroup = g;
    }

    // ── Copy buttons ──────────────────────────────────────────────
    private void CopyUsernameBtn_Click(object sender, RoutedEventArgs e)
        => System.Windows.Clipboard.SetText(UsernameBox.Text);

    private void CopyPasswordBtn_Click(object sender, RoutedEventArgs e)
    {
        string pwd = PasswordBox.Visibility == Visibility.Visible
            ? PasswordBox.Password
            : PasswordClearBox.Text;
        System.Windows.Clipboard.SetText(pwd);
    }

    // ── URL open ──────────────────────────────────────────────────
    private void OpenUrlBtn_Click(object sender, RoutedEventArgs e)
    {
        string url = UrlBox.Text.Trim();
        if (string.IsNullOrEmpty(url)) return;
        if (!url.StartsWith("http")) url = "https://" + url;
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    // ── Integrated generator ─────────────────────────────────────
    private void GenerateBtn_Click(object sender, RoutedEventArgs e)
    {
        GeneratorPanel.Visibility = GeneratorPanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
        MetaPanel.Visibility = GeneratorPanel.Visibility == Visibility.Visible
            ? Visibility.Collapsed
            : (_isNewEntry ? Visibility.Collapsed : Visibility.Visible);
        if (GeneratorPanel.Visibility == Visibility.Visible)
            _vm.Regenerate();
    }

    private void CloseGeneratorPanel_Click(object sender, RoutedEventArgs e)
    {
        GeneratorPanel.Visibility = Visibility.Collapsed;
        MetaPanel.Visibility = _isNewEntry ? Visibility.Collapsed : Visibility.Visible;
    }

    private void GenSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (GenLengthValue is null) return;
        int len = (int)GenLengthSlider.Value;
        GenLengthValue.Text = len.ToString();
        _vm.GeneratorLength = len;
        _vm.Regenerate();
    }

    private void GenOption_Changed(object sender, RoutedEventArgs e)
    {
        _vm.UseUppercase = ChkUppercase.IsChecked == true;
        _vm.UseLowercase = ChkLowercase.IsChecked == true;
        _vm.UseDigits = ChkDigits.IsChecked == true;
        _vm.UseSymbols = ChkSymbols.IsChecked == true;
        _vm.NoAmbiguous = ChkNoAmbiguous.IsChecked == true;
        _vm.Regenerate();
    }

    private void RegenerateBtn_Click(object sender, RoutedEventArgs e) => _vm.Regenerate();

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

    // ── Save / Cancel ─────────────────────────────────────────────
    private void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleBox.Text))
        {
            MessageBox.Show("Le titre est obligatoire.", "VaultIQ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TitleBox.Focus();
            return;
        }

        // Write back directly into the PasswordEntry (spec: "modifie PasswordEntry directement")
        _vm.Entry.Title = TitleBox.Text.Trim();
        _vm.Entry.Username = UsernameBox.Text.Trim();
        _vm.Entry.Password = PasswordBox.Visibility == Visibility.Visible
                               ? PasswordBox.Password
                               : PasswordClearBox.Text;
        _vm.Entry.Url = UrlBox.Text.Trim();
        _vm.Entry.Notes = NotesBox.Text;
        _vm.Entry.IsFavorite = FavoriteToggle.IsChecked == true;
        _vm.Entry.GroupId = (_vm.SelectedGroup ?? _vm.Groups.FirstOrDefault())?.Id
                               ?? _vm.Entry.GroupId;
        _vm.Entry.Touch();

        DialogResult = true;
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    // ── Resize → update strength bar ─────────────────────────────
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        string pwd = PasswordBox.Visibility == Visibility.Visible
            ? PasswordBox.Password : PasswordClearBox.Text;
        UpdateStrengthBar(pwd);
    }
}