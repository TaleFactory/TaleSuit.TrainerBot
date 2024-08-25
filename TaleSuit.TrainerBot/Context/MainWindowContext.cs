using System.Collections.ObjectModel;
using AdonisUI.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoenixWrapped;
using TaleKit.Game;
using TaleKit.Phoenix;

namespace TaleSuit.TrainerBot.Context;

public partial class MainWindowContext : ObservableObject
{
    public ObservableCollection<string> Characters { get; } = [];

    [ObservableProperty]
    private string? character;

    public IRelayCommand LoadedCommand { get; }
    public IRelayCommand SelectCharacterCommand { get; }
    public IRelayCommand CaptureCommand { get; }
    public IRelayCommand ReleaseCommand { get; }

    [ObservableProperty]
    private Session? session;

    [ObservableProperty]
    private bool capturing;
    
    [ObservableProperty]
    private bool releasing;

    private Thread? thread;
    private CancellationTokenSource? cts;

    public MainWindowContext()
    {
        LoadedCommand = new RelayCommand(OnLoaded);
        SelectCharacterCommand = new AsyncRelayCommand(OnSelectCharacter);
        CaptureCommand = new RelayCommand(OnCapture);
        ReleaseCommand = new RelayCommand(OnRelease);
    }

    private void OnCapture()
    {
        if (Capturing)
        {
            cts?.Cancel();

            Capturing = false;
            return;
        }

        cts = new CancellationTokenSource();
        thread = new Thread(() =>
        {
            if (Session == null)
            {
                return;
            }
            
            var skill = Session.Character.Skills.First(x => x.CastId == 1);

            var token = cts.Token;
            while (!token.IsCancellationRequested)
            {
                var chicken = Session
                    .Character
                    .Map
                    .Monsters
                    .Where(x => x.Name == "Poule")
                    .MinBy(x => x.Position.GetDistance(Session.Character.Position));

                if (chicken == null)
                {
                    continue;
                }

                if (chicken.HpPercentage > 50)
                {
                    Session.Character.Attack(chicken);
                }
                else
                {
                    if (!skill.IsOnCooldown)
                    {
                        Session.Character.Attack(chicken, skill);
                        Thread.Sleep(2000);
                    }
                }
                
                Thread.Sleep(1000);
            }
        });
        
        thread.Start();

        Capturing = true;
    }

    private void OnRelease()
    {
        MessageBox.Show("Not implemented yet", caption: "Error");
    }

    private Task OnSelectCharacter()
    {
        return Task.Run(() =>
        {
            if (Character == null)
            {
                return;
            }

            Session = PhoenixFactory.CreateSession(Character);
        });
    }
    
    private void OnLoaded()
    {
        Characters.Clear();
        
        var windows = PhoenixClientFactory.GetWindows();
        foreach (var window in windows)
        {
            Characters.Add(window.Character);
        }
    }
}