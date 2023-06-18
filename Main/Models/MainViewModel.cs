using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.Win32;

namespace Main.Models;

public class MainViewModel : BaseViewModel
{
    private ICommand? clearCommand;
    private ICommand? importFilesCommand;
    private string    pdfTextContent = "Empty";

    public MainViewModel(List<string> arguments)
    {
        Arguments        = arguments;
        PdfPageViewModel = new PdfPageViewModel(this);
    }

    public ObservableCollection<PdfPageViewModel> Pages            { get; } = new();
    public PdfPageViewModel                       PdfPageViewModel { get; }
    public List<string>                           Arguments        { get; }

    public string PdfTextContent
    {
        get => pdfTextContent;
        set
        {
            pdfTextContent = value;
            OnPropertyChanged();
        }
    }

    public ICommand ClearCommand => clearCommand ??= new CommandHandler(async () =>
    {
        await Task.Run(() =>
                   {
                       PdfTextContent = string.Empty;
                       Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, () => Pages.Clear());
                   })
                  .ConfigureAwait(false);
    }, () => true);

    public ICommand ImportFilesCommand => importFilesCommand ??= new CommandHandler(async () =>
    {
        await Task.Run(async () =>
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

                       var fileNames = dialog.FileNames;

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

                                   await Task.Delay(300);

                                   Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, () => Pages.Add(pageViewModel));
                                   OnPropertyChanged(nameof(Pages));
                               }

                               reader.Close();
                           }
                       }
                   })
                  .ConfigureAwait(false);
    }, () => true);
}