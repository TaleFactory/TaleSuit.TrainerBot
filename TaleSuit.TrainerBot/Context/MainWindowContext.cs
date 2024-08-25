using System.Collections.ObjectModel;
using System.Windows;
using AdonisUI.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoenixWrapped;
using TaleKit.Extension;
using TaleKit.Game;
using TaleKit.Game.Entities;
using TaleKit.Game.Storage;
using TaleKit.Phoenix;
using MessageBox = AdonisUI.Controls.MessageBox;

namespace TaleSuit.TrainerBot.Context;

public partial class MainWindowContext : ObservableObject
{
    private const int EssenceOne = 2666;
    private const int EssenceTwo = 2667;
    private const int EssenceThree = 2668;
    private const int EssenceFour = 2669;
    private const int EssenceFive = 2670;
    
    private readonly Dictionary<int, int> EssenceVirtualNumbers = new()
    {
        { 1, EssenceOne },
        { 2, EssenceTwo },
        { 3, EssenceThree },
        { 4, EssenceFour },
        { 5, EssenceFive }
    };
    
    public ObservableCollection<string> Characters { get; } = [];

    [ObservableProperty]
    private string? character;

    public IRelayCommand LoadedCommand { get; }
    public IRelayCommand SelectCharacterCommand { get; }
    public IRelayCommand CaptureCommand { get; }
    public IRelayCommand ReleaseCommand { get; }
    public IRelayCommand UpgradeCommand { get; }
    public IRelayCommand StopCommand { get; }

    [ObservableProperty]
    private Session? session;

    [ObservableProperty]
    private bool running;

    [ObservableProperty]
    private bool stopping;

    private Thread? thread;
    private CancellationTokenSource? cts;

    public MainWindowContext()
    {
        LoadedCommand = new RelayCommand(OnLoaded);
        SelectCharacterCommand = new AsyncRelayCommand(OnSelectCharacter);
        CaptureCommand = new RelayCommand(OnCapture);
        ReleaseCommand = new RelayCommand(OnRelease);
        UpgradeCommand = new RelayCommand(OnUpgrade);
        StopCommand = new AsyncRelayCommand(OnStop);
    }

    private Task OnStop()
    {
        return Task.Run(() =>
        {
            Stopping = true;
            cts?.Cancel();
            thread?.Join();
            Stopping = false;
        });
    }
    
    private void OnCapture()
    {
        if (Session == null)
        {
            return;
        }

        if (Session.Character.Map.Id != 1)
        {
            MessageBox.Show("You must be in NosVille to capture chickens.", "Error");
            return;
        }
        
        cts = new CancellationTokenSource();
        thread = new Thread(() =>
        {
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
                
                var range = Session.Character.Position.IsInRange(chicken.Position, 2);
                if (!range)
                {
                    Session.Character.Walk(chicken.Position);

                    while (!Session.Character.Position.IsInRange(chicken.Position, 2))
                    {
                        if (cts.IsCancellationRequested)
                        {
                            break;
                        }
                        
                        Thread.Sleep(1000);
                    }
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
            
            Running = false;
        });
        
        thread.Start();
        
        Running = true;
    }

    private void OnRelease()
    {
        if (Session == null)
        {
            return;
        }

        if (Session.Character.Map.Id != 20001)
        {
            MessageBox.Show("You must be in the Miniland to release chickens.", "Error");
            return;
        }
        
        cts = new CancellationTokenSource();
        thread = new Thread(() =>
        {
            var skill = Session.Character.Skills.First(x => x.CastId == 3);

            var token = cts.Token;
            while (!token.IsCancellationRequested)
            {
                var chickens = Session
                    .Character
                    .Nosmates
                    .Where(x => x.VirtualNumber == 333)
                    .ToList();

                var releasable = chickens
                    .Where(x => x.HeroLevel == x.Stars * 10)
                    .ToList();

                foreach (var chicken in releasable)
                {
                    var entity = Session.Character.Map.GetEntity<Npc>(EntityType.Npc, chicken.Id);
                    if (entity == null)
                    {
                        continue;
                    }

                    var range = Session.Character.Position.IsInRange(entity.Position, 1);
                    if (!range)
                    {
                        Session.Character.Walk(entity.Position);

                        while (!Session.Character.Position.IsInRange(entity.Position, 1))
                        {
                            if (cts.IsCancellationRequested)
                            {
                                break;
                            }
                            
                            Thread.Sleep(1000);
                        }
                    }

                    Session.SendPacket($"u_s {skill.CastId} {(int)entity.EntityType} {entity.Id}");
                    Thread.Sleep(2000);
                    
                    Session.SendPacket($"#guri^451^{chicken.Index}^333^2");
                    Thread.Sleep(1000);
                }
                
                Thread.Sleep(1000);
            }
            
            Running = false;
        });
        
        thread.Start();
        Running = true;
    }
    
    private void OnUpgrade()
    {
        if (Session == null)
        {
            return;
        }

        if (Session.Character.Map.Id != 20001)
        {
            MessageBox.Show("You must be in the Miniland to upgrade chickens.", "Error");
            return;
        }
        
        cts = new CancellationTokenSource();
        thread = new Thread(() =>
        {
            var chickens = Session
                .Character
                .Nosmates
                .Where(x => x.VirtualNumber == 333)
                .Where(x => x.Stars != 6)
                .ToList();

            var releasable = chickens
                .Where(x => x.HeroLevel == x.Stars * 10)
                .ToList();

            foreach (var chicken in releasable)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }
                
                var essence = Session.Character.Inventory
                    .GetItems(InventoryType.Etc)
                    .FirstOrDefault(x => x.VirtualNumber == EssenceVirtualNumbers[chicken.Stars]);

                if (essence == null)
                {
                    continue;
                }

                var entity = Session.Character.Map.GetEntity<Npc>(EntityType.Npc, chicken.Id);
                if (entity == null)
                {
                    continue;
                }

                var range = Session.Character.Position.IsInRange(entity.Position, 1);
                if (!range)
                {
                    Session.Character.Walk(entity.Position);

                    while (!Session.Character.Position.IsInRange(entity.Position, 1))
                    {
                        if (cts.IsCancellationRequested)
                        {
                            break;
                        }
                        
                        Thread.Sleep(1000);
                    }
                }

                Session.Character.Dance(-2, 5000, -3).ThenExecute(() =>
                {
                    Session.SendPacket($"up_gr 103 {chicken.Index} 333");
                }).GetAwaiter().GetResult();

                Thread.Sleep(1000);
            }

            Running = false;
        });
        
        thread.Start();
        Running = true;
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