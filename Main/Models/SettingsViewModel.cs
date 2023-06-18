using System;
using System.IO;

namespace Main.Models;

public class SettingsViewModel : BaseViewModel
{
    private string filePath;

    public SettingsViewModel(MainViewModel mainViewModel)
    {
        MainViewModel = mainViewModel;
        var path          = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var subFolderPath = Path.Combine(path, "Mergonator");

        if (!Directory.Exists(subFolderPath))
            Directory.CreateDirectory(subFolderPath);

        filePath = subFolderPath;
    }

    public MainViewModel MainViewModel { get; }

    public string FilePath
    {
        get => filePath;
        set
        {
            filePath = value;
            OnPropertyChanged();
        }
    }
}