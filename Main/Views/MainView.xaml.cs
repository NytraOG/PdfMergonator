using System.Windows;

namespace Main.Views;

public partial class MainView : Window
{
    public MainView() => InitializeComponent();

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        // var view = new SettingsView
        // {
        //     Content = new SettingsViewModel(new MainViewModel(new List<string>())),
        //     Owner   = this
        // };
        //
        // view.ShowDialog();
    }

    private void AddPresetButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { ContextMenu: not null } addButton)
            addButton.ContextMenu.IsOpen = true;
    }
}