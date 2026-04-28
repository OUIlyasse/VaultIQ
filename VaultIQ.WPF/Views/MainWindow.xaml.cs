using System.Windows;
using System.Windows.Input;
using VaultIQ.WPF.ViewModels;

namespace VaultIQ.WPF.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel();
        DataContext = _vm;

        // Sync window title from ViewModel
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.WindowTitle))
                Title = _vm.WindowTitle;
        };

        Loaded += OnLoaded;
    }

    // Overload for startup file-open (e.g. drag-and-drop on .exe)
    public MainWindow(string filePath) : this()
    {
        Loaded += (_, _) => _vm.OpenFileOnStartup(filePath);
    }

    // ── Loaded ────────────────────────────────────────────────────
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Re-render strength bar whenever the selected entry changes
        _vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(MainViewModel.SelectedEntry))
                UpdateStrengthBar();
        };
    }

    // ── Strength bar (Grid width, bound manually) ─────────────────
    // StrengthBarContainer and StrengthBarFill are x:Name'd in the XAML
    private void UpdateStrengthBar()
    {
        if (StrengthBarContainer is null || StrengthBarFill is null) return;

        var entry = _vm.SelectedEntry;
        if (entry is null)
        {
            StrengthBarFill.Width = 0;
            return;
        }

        double ratio = Math.Clamp(entry.StrengthScore / 100.0, 0.0, 1.0);
        StrengthBarFill.Width = StrengthBarContainer.ActualWidth * ratio;

        StrengthBarFill.Background = entry.StrengthScore switch
        {
            >= 80 => (System.Windows.Media.Brush)FindResource("Green"),
            >= 50 => (System.Windows.Media.Brush)FindResource("Amber"),
            _ => (System.Windows.Media.Brush)FindResource("Red")
        };
    }

    // ── Title bar ─────────────────────────────────────────────────
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
            ToggleMaximize();
        else
            DragMove();
    }

    private void CloseBtn_Click(object sender, MouseButtonEventArgs e) => Close();

    private void MinimizeBtn_Click(object sender, MouseButtonEventArgs e) => WindowState = WindowState.Minimized;

    private void MaximizeBtn_Click(object sender, MouseButtonEventArgs e) => ToggleMaximize();

    private void ToggleMaximize() =>
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    // ── Keyboard shortcuts ────────────────────────────────────────
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        bool ctrl = Keyboard.Modifiers == ModifierKeys.Control;

        if (ctrl && e.Key == Key.N) { _vm.NewDatabaseCommand.Execute(null); e.Handled = true; }
        else if (ctrl && e.Key == Key.O) { _vm.OpenDatabaseCommand.Execute(null); e.Handled = true; }
        else if (ctrl && e.Key == Key.S) { _vm.SaveDatabaseCommand.Execute(null); e.Handled = true; }
        else if (ctrl && e.Key == Key.I) { _vm.AddEntryCommand.Execute(null); e.Handled = true; }
        else if (ctrl && e.Key == Key.E) { _vm.EditEntryCommand.Execute(null); e.Handled = true; }
        else if (ctrl && e.Key == Key.B) { _vm.CopyUsernameCommand.Execute(null); e.Handled = true; }
        else if (ctrl && e.Key == Key.U) { _vm.OpenUrlCommand.Execute(null); e.Handled = true; }
        else if (ctrl && e.Key == Key.G) { _vm.OpenGeneratorCommand.Execute(null); e.Handled = true; }
        else if (ctrl && e.Key == Key.L) { _vm.LockCommand.Execute(null); e.Handled = true; }
        else if (e.Key == Key.Delete) { _vm.DeleteEntryCommand.Execute(null); e.Handled = true; }
        // Ctrl+C : copy password only when an entry is focused (avoid stomping normal clipboard)
        else if (ctrl && e.Key == Key.C && _vm.SelectedEntry is not null)
        {
            _vm.CopyPasswordCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+Q / Ctrl+D : placeholders — wire to commands once added in ViewModel
    }

    // ── Resize → keep strength bar proportional ──────────────────
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateStrengthBar();
    }
}