using System.Linq;
using System.Windows;
using Main.Models;
using Main.Views;
using Microsoft.Extensions.Configuration;

namespace Main;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App() => Config = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();

    public static IConfiguration Config { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        MainWindow             = new MainView();
        MainWindow.DataContext = new MainViewModel(e.Args.ToList());
        MainWindow.Show();
    }
}