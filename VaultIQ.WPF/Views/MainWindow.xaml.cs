using System.Windows;
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
    }

    /// <summary>Used by App.xaml.cs when a .viq file is passed on the command line.</summary>
    public MainWindow(string filePath) : this()
        => Loaded += (_, _) => _vm.OpenFileOnStartup(filePath);

    // ── Focus search (Ctrl+F) ────────────────────────────────────
    // Called from the FocusSearchCommand in the ViewModel via a relay,
    // OR directly here when the Window receives the KeyBinding.
    private void FocusSearch()
    {
        SearchBox.Focus();
        SearchBox.SelectAll();
    }
}