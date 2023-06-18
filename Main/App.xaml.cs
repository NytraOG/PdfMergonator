using System.Linq;
using System.Windows;
using Main.Models;
using Main.Views;

namespace Main;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        MainWindow             = new MainView();
        MainWindow.DataContext = new MainViewModel(e.Args.ToList());
        MainWindow.Show();
    }
}