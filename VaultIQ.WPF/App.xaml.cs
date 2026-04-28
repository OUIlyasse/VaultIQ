using System.Windows;

namespace VaultIQ.WPF;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var vm = new ViewModels.MainViewModel();
        var window = new Views.MainWindow { DataContext = vm };
        MainWindow = window;
        window.Show();

        // Ouvrir un fichier .viq passé en argument de ligne de commande
        if (e.Args.Length > 0 && System.IO.File.Exists(e.Args[0]))
            vm.OpenFileOnStartup(e.Args[0]);
    }
}