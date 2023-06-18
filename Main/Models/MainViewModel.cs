using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Main.Models;

public class MainViewModel : BaseViewModel
{
    private ICommand? clearCommand;
    private ICommand? exportFilesCommand;
    private string    fileName = string.Empty;
    private string[]  fileNames;
    private ICommand? importFilesCommand;
    private ICommand? openSettingsCommand;

    public MainViewModel(List<string> arguments)
    {
        Arguments         = arguments;
        PdfPageViewModel  = new PdfPageViewModel(this);
        SettingsViewModel = new SettingsViewModel(this);
    }

    public ObservableCollection<PdfPageViewModel> Pages             { get; } = new();
    public PdfPageViewModel                       PdfPageViewModel  { get; }
    public SettingsViewModel                      SettingsViewModel { get; }
    public List<string>                           Arguments         { get; }

    public string FileName
    {
        get => fileName;
        set
        {
            fileName = value;
            OnPropertyChanged();
        }
    }

    public ICommand OpenSettingsCommand => openSettingsCommand ??= new CommandHandler(async () =>
    {
        await Task.Run(() =>
                   {
                       SettingsUiThread = new Thread(OpenSettings);
                       SettingsUiThread.SetApartmentState(ApartmentState.STA);
                       SettingsUiThread.IsBackground = false;
                       SettingsUiThread.Start();
                   })
                  .ConfigureAwait(false);
    }, () => true);

    public Thread SettingsUiThread { get; set; }

    public ICommand ClearCommand => clearCommand ??= new CommandHandler(async () =>
    {
        await Task.Run(() =>
                   {
                       Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, () => Pages.Clear());
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
                       var stream       = new FileStream(SettingsViewModel.FilePath + "\\" + $"{FileName}.pdf", FileMode.Create, FileAccess.Write);

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
                                            $"{SettingsViewModel.FilePath}\\{FileName}";

                       var button = MessageBoxButton.OK;
                       var icon   = MessageBoxImage.Information;

                       MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.Yes);
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

    private void OpenSettings()
    {
        var view = new SettingsView
        {
            Topmost     = true,
            DataContext = SettingsViewModel
        };
        view.Show();

        Dispatcher.Run();
    }

    public void CloseSettings() { }
}