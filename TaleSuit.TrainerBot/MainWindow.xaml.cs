using AdonisUI.Controls;
using TaleSuit.TrainerBot.Context;

namespace TaleSuit.TrainerBot;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : AdonisWindow
{
    public MainWindow()
    {
        DataContext = new MainWindowContext();
        InitializeComponent();
    }
}