using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Main.Models;

public class SettingsViewModel : BaseViewModel
{
    private ICommand? cancelCommand;
    private string    outputDirectory;
    private ICommand? saveCommand;

    public SettingsViewModel(MainViewModel mainViewModel)
    {
        MainViewModel = mainViewModel;
        var path          = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var subFolderPath = Path.Combine(path, "Mergonator");

        if (!Directory.Exists(subFolderPath))
            Directory.CreateDirectory(subFolderPath);

        outputDirectory = subFolderPath;
    }

    public MainViewModel MainViewModel { get; }

    public ICommand SaveCommand => saveCommand ??= new CommandHandler(async () =>
    {
        await Task.Run(() => { })
                  .ConfigureAwait(false);
    }, () => true);

    public ICommand CancelCommand => cancelCommand ??= new CommandHandler(async () =>
    {
        await Task.Run(() =>
                   {
                       MainViewModel.CloseSettings();
                   })
                  .ConfigureAwait(false);
    }, () => true);

    public string OutputDirectory
    {
        get => outputDirectory;
        set
        {
            outputDirectory = value;
            OnPropertyChanged();
        }
    }
}