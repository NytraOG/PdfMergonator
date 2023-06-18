namespace Main.Models;

public class PdfPageViewModel : BaseViewModel
{
    public PdfPageViewModel(MainViewModel mainViewModel) => MainViewModel = mainViewModel;

    public MainViewModel MainViewModel { get; }
}