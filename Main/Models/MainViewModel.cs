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
    private string[]  fileNames;
    private ICommand? importFilesCommand;
    private ICommand? openSettingsCommand;
    private string    pdfTextContent = "Empty";

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

    public string PdfTextContent
    {
        get => pdfTextContent;
        set
        {
            pdfTextContent = value;
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
                       PdfTextContent = string.Empty;
                       Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, () => Pages.Clear());
                   })
                  .ConfigureAwait(false);
    }, () => true);

    public ICommand ExportFilesCommand => exportFilesCommand ??= new CommandHandler(async () =>
    {
        await Task.Run(() =>
                   {
                       var pagesToMerge = Pages.Where(p => p.Keep).ToList();
                       var stream       = new FileStream(SettingsViewModel.FilePath + "\\" + "Export.pdf", FileMode.Create, FileAccess.Write);

                       var document = new Document(PageSize.A4);
                       var copy     = new PdfCopy(document, stream);
                       document.Open();

                       foreach (var fileName in fileNames)
                       {
                           using var reader = new PdfReader(fileName);

                           foreach (var page in pagesToMerge.Where(p => fileName.Contains(p.FileName)))
                           {
                               var pdfPage = copy.GetImportedPage(reader, page.PageNumber);
                               copy.AddPage(pdfPage);
                           }
                       }

                       document.Close();
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

                       foreach (var fileName in fileNames)
                       {
                           if (dialog.CheckFileExists)
                           {
                               var reader = new PdfReader(fileName);

                               for (var i = 1; i <= reader.NumberOfPages; i++)
                               {
                                   var content          = PdfTextExtractor.GetTextFromPage(reader, i, new LocationTextExtractionStrategy());
                                   var formattedContent = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(content)));

                                   PdfTextContent += i == 1 ? formattedContent : Environment.NewLine + Environment.NewLine + formattedContent;

                                   var pageViewModel = new PdfPageViewModel(this)
                                   {
                                       FileName   = fileName.Split('\\')[^1],
                                       Content    = formattedContent,
                                       PageNumber = i
                                   };

                                   Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, () => Pages.Add(pageViewModel));
                                   OnPropertyChanged(nameof(Pages));
                               }

                               reader.Close();
                           }
                       }
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