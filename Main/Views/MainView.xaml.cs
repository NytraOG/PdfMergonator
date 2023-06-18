using System;
using System.Collections.Generic;
using System.Windows;
using Main.Models;

namespace Main.Views;

public partial class MainView : Window
{
    public MainView() => InitializeComponent();

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var view = new SettingsView
        {
            Content = new SettingsViewModel(new MainViewModel(new List<string>())),
            Owner   = this
        };

        view.ShowDialog();
    }
}