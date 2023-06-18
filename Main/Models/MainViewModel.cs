using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Main.Models;

public class MainViewModel : BaseViewModel
{
    private ICommand? testCommand;
    private string    testText = "hihi";

    public MainViewModel(List<string> arguments)
    {
        Arguments        = arguments;
        PdfPageViewModel = new PdfPageViewModel(this);
    }

    public PdfPageViewModel PdfPageViewModel { get; }
    public List<string>     Arguments        { get; }

    public string TestText
    {
        get => testText;
        set
        {
            testText = value;
            OnPropertyChanged();
        }
    }

    public ICommand TestCommand => testCommand ??= new CommandHandler(async () =>
    {
        await Task.Delay(2000);

        await Task.Run(() =>
        {
            TestText = "Hupu";
        });
    }, () => true);
}