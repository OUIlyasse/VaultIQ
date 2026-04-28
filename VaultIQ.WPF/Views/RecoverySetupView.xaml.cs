using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VaultIQ.WPF.ViewModels;

namespace VaultIQ.Views;

public partial class RecoverySetupView : Window
{
    private readonly RecoverySetupViewModel _vm;

    // Step descriptions (steps are 1-indexed; index 0 unused)
    private static readonly string[] StepSubtitles =
    [
        "",
        "Étape 1 / 4 — Méthode de récupération",
        "Étape 2 / 4 — Téléphone de récupération",
        "Étape 3 / 4 — Code de vérification",
        "Étape 4 / 4 — Confirmation"
    ];

    private static readonly string[] SectionLabels =
    [
        "",
        "MÉTHODE",
        "TÉLÉPHONE DE RÉCUPÉRATION",
        "VÉRIFICATION",
        "CONFIRMATION"
    ];

    private static readonly string[] Descriptions =
    [
        "",
        "Choisissez comment vous souhaitez récupérer l'accès en cas d'oubli du mot de passe.",
        "Votre numéro sera hashé avec PBKDF2 et jamais stocké en clair. Il servira uniquement en cas d'oubli du mot de passe.",
        "Un code à 6 chiffres sera envoyé sur le numéro renseigné pour vérifier votre accès.",
        "Récupération configurée avec succès. Vous pouvez fermer cette fenêtre."
    ];

    public RecoverySetupView()
    {
        InitializeComponent();
        _vm = new RecoverySetupViewModel();
        DataContext = _vm;

        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(RecoverySetupViewModel.CurrentStep))
                RefreshStep();
        };

        RefreshStep();
    }

    // ── Public result ─────────────────────────────────────────────
    public string RecoveryPhone => _vm.PhoneNumber;
    public string CountryCode => _vm.CountryCode;

    // ── Header drag ───────────────────────────────────────────────
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    // ── Step rendering ────────────────────────────────────────────
    private void RefreshStep()
    {
        int step = _vm.CurrentStep;
        StepSubtitle.Text = StepSubtitles[step];
        SectionLabel.Text = SectionLabels[step];
        DescText.Text = Descriptions[step];

        UpdateStepDots(step);

        // Show/hide step-specific controls
        CountryRow.Visibility = step == 2 ? Visibility.Visible : Visibility.Collapsed;
        PhoneRow.Visibility = step == 2 ? Visibility.Visible : Visibility.Collapsed;

        // Last step: change button label
        NextBtn.Content = step == 4 ? "Terminer" : "Suivant →";
    }

    private void UpdateStepDots(int active)
    {
        // Reusable helpers
        void SetDone(Border b, System.Windows.Controls.TextBlock t)
        {
            b.Style = (Style)FindResource("StepDone");
            t.Text = "✓";
            t.Foreground = new SolidColorBrush(Color.FromRgb(0x07, 0x0E, 0x1C));
        }
        void SetActive(Border b, System.Windows.Controls.TextBlock t, string label)
        {
            b.Style = (Style)FindResource("StepActive");
            t.Text = label;
            t.Foreground = Brushes.White;
        }
        void SetPending(Border b, System.Windows.Controls.TextBlock t, string label)
        {
            b.Style = (Style)FindResource("StepPending");
            t.Text = label;
            t.Foreground = (Brush)FindResource("Text3");
        }

        (Border, System.Windows.Controls.TextBlock, string)[] steps =
        [
            (Step1Border, Step1Text, "1"),
            (Step2Border, Step2Text, "2"),
            (Step3Border, Step3Text, "3"),
            (Step4Border, Step4Text, "4")
        ];

        for (int i = 0; i < steps.Length; i++)
        {
            var (border, text, label) = steps[i];
            int stepNum = i + 1;
            if (stepNum < active) SetDone(border, text);
            else if (stepNum == active) SetActive(border, text, label);
            else SetPending(border, text, label);
        }

        // Update connector lines
        var doneBrush = (Brush)FindResource("Teal2");
        var pendingBrush = (Brush)FindResource("Border1");
        Line12.Background = active > 1 ? doneBrush : pendingBrush;
        Line23.Background = active > 2 ? doneBrush : pendingBrush;
        Line34.Background = active > 3 ? doneBrush : pendingBrush;
    }

    // ── Country / Phone ───────────────────────────────────────────
    private void CountryCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (CountryCombo.SelectedItem is System.Windows.Controls.ComboBoxItem item)
        {
            // Extract dial code from string like "🇫🇷 France (+33)"
            var match = Regex.Match(item.Content.ToString()!, @"\((\+\d+)\)");
            _vm.CountryCode = match.Success ? match.Groups[1].Value : "+33";
            UpdatePhoneValidation();
        }
    }

    private void PhoneBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        _vm.PhoneNumber = PhoneBox.Text;
        UpdatePhoneValidation();
    }

    private void UpdatePhoneValidation()
    {
        string full = _vm.CountryCode + _vm.PhoneNumber.TrimStart('0');
        bool valid = Regex.IsMatch(_vm.PhoneNumber, @"^0\d{9}$");
        PhoneValidText.Visibility = valid ? Visibility.Visible : Visibility.Collapsed;
        if (valid)
        {
            PhoneValidText.Text = $"✓ Numéro valide → {full}";
            MaskedPhone.Text = MaskPhone(full);
        }
    }

    private static string MaskPhone(string phone)
    {
        if (phone.Length < 6) return phone;
        // Keep prefix (+33) + first 2 + mask middle + last 2
        return phone[..5] + "** **** " + phone[^2..];
    }

    // ── Button handlers ───────────────────────────────────────────
    private void NextBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.CurrentStep == 2 && !Regex.IsMatch(_vm.PhoneNumber, @"^0\d{9}$"))
        {
            MessageBox.Show("Veuillez saisir un numéro valide (ex: 0612345678).", "VaultIQ",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_vm.CurrentStep >= 4)
            DialogResult = true;
        else
            _vm.CurrentStep++;
    }

    private void BackBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.CurrentStep <= 1)
            DialogResult = false;
        else
            _vm.CurrentStep--;
    }

    private void SkipBtn_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}