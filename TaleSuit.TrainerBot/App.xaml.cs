using System.Windows;
using Serilog;
using Serilog.Events;
using TaleKit;
using TaleKit.Game.Registry;

namespace TaleSuit.TrainerBot;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("Logs/info.log", LogEventLevel.Information)
            .WriteTo.File("Logs/error.log", LogEventLevel.Error)
            .CreateLogger();

        TaleKitSettings.Language = Language.French;
        TaleKitSettings.StorageDirectory = e.Args.Length == 0 
            ? AppDomain.CurrentDomain.BaseDirectory 
            : e.Args[0];
        
        Log.Information("Language: {Language}", TaleKitSettings.Language);
        Log.Information("Storage Directory: {Directory}", TaleKitSettings.StorageDirectory);
        
        base.OnStartup(e);
    }
}