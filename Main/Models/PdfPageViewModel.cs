namespace Main.Models;

public class PdfPageViewModel : BaseViewModel
{
    private string content  = string.Empty;
    private string fileName = string.Empty;
    private int    pageNumber;

    public PdfPageViewModel(MainViewModel mainViewModel) => MainViewModel = mainViewModel;

    public MainViewModel MainViewModel { get; }
    public bool          Keep          { get; set; }

    public int PageNumber
    {
        get => pageNumber;
        set
        {
            pageNumber = value;
            OnPropertyChanged();
        }
    }

    public string FileName
    {
        get => fileName;
        set
        {
            fileName = value;
            OnPropertyChanged();
        }
    }

    public string Content
    {
        get => content;
        set
        {
            content = value;
            OnPropertyChanged();
        }
    }
}