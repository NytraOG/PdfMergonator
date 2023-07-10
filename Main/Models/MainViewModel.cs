using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Main.Views;
using Microsoft.Win32;
using Wpf.Ui.Common;

namespace Main.Models;

public class MainViewModel : BaseViewModel,
                             INotifyPropertyChanged
{
    private ICommand? clearCommand;
    private object    currentView;
    private ICommand? exportFilesCommand;
    private string    fileName  = string.Empty;
    private string[]  fileNames = null!;
    private ICommand? importFilesCommand;
    private ICommand? openOutputDirectoryCommand;
    private ICommand? openSettingsCommand;

    public MainViewModel()
    {
        PdfPageViewModel  = new PdfPageViewModel(this);
        SettingsViewModel = new SettingsViewModel(this);
    }

    public MainViewModel(List<string> arguments)
    {
        Arguments         = arguments;
        PdfPageViewModel  = new PdfPageViewModel(this);
        SettingsViewModel = new SettingsViewModel(this);
    }

    public object CurrentView
    {
        get => currentView;
        set
        {
            currentView = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<PdfPageViewModel> Pages             { get; } = new();
    public PdfPageViewModel                       PdfPageViewModel  { get; }
    public SettingsViewModel                      SettingsViewModel { get; }
    public List<string>                           Arguments         { get; } = null!;

    public string FileName
    {
        get => fileName;
        set
        {
            fileName = value;
            OnPropertyChanged();
        }
    }

    public ICommand LoadPdfQuetscheView => new RelayCommand(OpenSettingsView);
    public ICommand LoadMergonatorView  => new RelayCommand(OpenMergonatorView);

    public ICommand OpenSettingsCommand => openSettingsCommand ??= new CommandHandler(async () =>
    {
        await Task.Run(() =>
                   {
                       SettingsUiThread = new Thread(OpenSettingsView);
                       SettingsUiThread.SetApartmentState(ApartmentState.STA);
                       SettingsUiThread.IsBackground = false;
                       SettingsUiThread.Start();
                   })
                  .ConfigureAwait(false);
    }, () => true);

    public Thread SettingsUiThread { get; set; } = null!;

    public ICommand ClearCommand => clearCommand ??= new CommandHandler(async () =>
    {
        await Task.Run(() =>
                   {
                       Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, () => Pages.Clear());
                   })
                  .ConfigureAwait(false);
    }, () => true);

    public ICommand OpenOutputDirectoryCommand => openOutputDirectoryCommand ??= new CommandHandler(async () =>
    {
        await Task.Run(() =>
                   {
                       Process.Start("explorer", SettingsViewModel.OutputDirectory);
                   })
                  .ConfigureAwait(false);
    }, () => true);

    public ICommand ExportFilesCommand => exportFilesCommand ??= new CommandHandler(async () =>
    {
        await Task.Run(() =>
                   {
                       if (!Pages.Any() || !fileNames.Any())
                           return;

                       var pagesToMerge = Pages.Where(p => p.Keep).ToList();

                       if (!pagesToMerge.Any())
                       {
                           var caption = "Reminder";

                           var messageBoxText = "No Pages selected";

                           var button = MessageBoxButton.OK;
                           var icon   = MessageBoxImage.Warning;

                           MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                       }
                       else
                       {
                           var stream = new FileStream(SettingsViewModel.OutputDirectory + "\\" + $"{FileName}.pdf", FileMode.Create, FileAccess.Write);

                           var document = new Document(PageSize.A4);
                           var copy     = new PdfCopy(document, stream);
                           document.Open();

                           foreach (var file in fileNames)
                           {
                               using var reader = new PdfReader(file);

                               foreach (var page in pagesToMerge.Where(p => file.Contains(p.FileName)))
                               {
                                   var pdfPage = copy.GetImportedPage(reader, page.PageNumber);
                                   copy.AddPage(pdfPage);
                               }
                           }

                           document.Close();

                           var caption = "Success!";

                           var messageBoxText = "Generated Pdf saved to Output Directory" + Environment.NewLine + Environment.NewLine +
                                                $"{SettingsViewModel.OutputDirectory}\\{FileName}";

                           var button = MessageBoxButton.OK;
                           var icon   = MessageBoxImage.Information;

                           MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
                       }
                   })
                  .ConfigureAwait(false);
    }, () => true);

    public ICommand ImportFilesCommand => importFilesCommand ??= new CommandHandler(async () =>
    {
        await Task.Run(() =>
                   {
                       var dialog = new OpenFileDialog
                       {
                           DefaultExt  = ".pdf",
                           Filter      = "Pdf Files (.pdf)|*.pdf",
                           Multiselect = true
                       };

                       var result = dialog.ShowDialog();

                       if (result != true)
                           return;

                       fileNames = dialog.FileNames;

                       foreach (var file in fileNames)
                       {
                           if (dialog.CheckFileExists)
                           {
                               var reader = new PdfReader(file);

                               for (var i = 1; i <= reader.NumberOfPages; i++)
                               {
                                   var lines = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy())
                                                               .Split("\n");

                                   var content = lines.Length switch
                                   {
                                       1 => lines[0],
                                       2 => lines[0] + Environment.NewLine + lines[1],
                                       >= 3 => lines[0] + Environment.NewLine + lines[1] + Environment.NewLine + lines[2],
                                       _ => string.Empty
                                   };

                                   var formattedContent = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(content)));

                                   var pageViewModel = new PdfPageViewModel(this)
                                   {
                                       FileName   = file.Split('\\')[^1],
                                       Content    = formattedContent,
                                       PageNumber = i
                                   };

                                   Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, () => Pages.Add(pageViewModel));
                                   OnPropertyChanged(nameof(Pages));
                               }

                               reader.Close();
                           }
                       }

                       FileName = string.Join('_', Pages.Select(p => p.FileName.Split('.')[0]).Distinct());
                   })
                  .ConfigureAwait(false);
    }, () => true);


    private void OpenMergonatorView() => CurrentView = new MainView();

    private void OpenSettingsView()
    {
        var view = new TestControl()
        {
            //Topmost     = true,
            DataContext = SettingsViewModel
        };

        CurrentView = view;

        Dispatcher.Run();
    }

    public void CloseSettings() { }
}