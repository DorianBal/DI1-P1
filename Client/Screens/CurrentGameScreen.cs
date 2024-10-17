using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using Client.Records;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

using Terminal.Gui;

namespace Client.Screens;

public class CurrentGameScreen(Window target, int gameId, string playerName)
{
    private readonly Window Target = target;
    private CurrentGameView? CurrentView = null;

    private readonly int GameId = gameId;
    private readonly string PlayerName = playerName;
    private GameOverview? CurrentGame = null;

    private bool CurrentGameLoading = true;
    private bool CurrentGameStarted = false;
    private bool CurrentGameEnded = false;
    
    private CurrentGameActionList.Action? CurrentRoundAction = null;

    public async Task Show()
    {
        await BeforeShow();
        await LoadGame();
        await DisplayMainView();
        await DisplayCompanyView();
    }

    private Task BeforeShow()
    {
        Target.RemoveAll();

        ReloadWindowTitle();

        return Task.CompletedTask;
    }

    private void ReloadWindowTitle()
    {
        var gameName = CurrentGame is null ? "..." : CurrentGame.Name;
        Target.Title = $"{MainWindow.Title} - [Game {gameName}]";
    }

    private async Task LoadGame()
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri($"{WssConfig.WebSocketServerScheme}://{WssConfig.WebSocketServerDomain}:{WssConfig.WebSocketServerPort}/games/{GameId}"), opts =>
            {
                opts.HttpMessageHandlerFactory = (message) =>
                {
                    if (message is HttpClientHandler clientHandler)
                    {
                        clientHandler.ServerCertificateCustomValidationCallback +=
                            (sender, certificate, chain, sslPolicyErrors) => { return true; };
                    }

                    return message;
                };
            })
            .AddJsonProtocol()
            .Build();

        hubConnection.On<GameOverview>("CurrentGameUpdated", async data =>
        {
            CurrentGame = data;
            ReloadWindowTitle();
            CurrentGameLoading = false;
            CurrentRoundAction = null;
            if (data.Status == "InProgress") { CurrentGameStarted = true; }
            if (data.Status == "Finished") { GameEnded(); } //CurrentGameEnded = true;
        });

        await hubConnection.StartAsync();

        var loadingDialog = new Dialog()
        {
            Width = 17,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Loading game...",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        loadingDialog.Add(loadingText);

        Target.Add(loadingDialog);

        while (CurrentGameLoading) { await Task.Delay(100); }

        Target.Remove(loadingDialog);
    }

    private async Task DisplayMainView()
    {
        Target.RemoveAll();

        var mainView = new CurrentGameMainView(CurrentGame!, PlayerName);

        CurrentView = mainView;

        mainView.X = mainView.Y = Pos.Center();
        mainView.OnStart = (_, __) => { CurrentGameStarted = true; };

        Target.Add(mainView);

        while (!CurrentGameStarted)
        {
            Target.Remove(mainView);
            mainView = new CurrentGameMainView(CurrentGame!, PlayerName);
            Target.Add(mainView);
            await Task.Delay(100);
        }
    }

    private async Task DisplayCompanyView()
    {
        Target.RemoveAll();

        var companyView = new CurrentGameCompanyView(CurrentGame!, PlayerName);

        CurrentView = companyView;

        companyView.X = companyView.Y = 5;
        companyView.Width = companyView.Height = Dim.Fill() - 5;
        companyView.OnRoundAction = (_, roundAction) => { CurrentRoundAction = roundAction; };

        Target.Add(companyView);

        while (CurrentRoundAction is null && !CurrentGameEnded)
        {
            await Task.Delay(100);
        }

        var lastRound = CurrentGame!.CurrentRound;

        await ActInRound();

        while (
            !CurrentGameEnded &&
            // CurrentGame!.CurrentRound != CurrentGame.MaximumRounds &&
            CurrentGame!.CurrentRound == lastRound
        )
        {
            await Task.Delay(100);
        }

        if (!CurrentGameEnded)
        {
            await DisplayCompanyView();
        }
    }

    private async void GameEnded()
    {
        // Suppression de tous les écrans
        Target.RemoveAll();

        // Instanciation des éléments visuel
        var loadingDialog = new Dialog()
        {
            Width = 20,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Game Ended !",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        // Affichage des éléments visuel
        loadingDialog.Add(loadingText);
        Target.Add(loadingDialog);

        await Task.Delay(3000);

        // Affichage du menu d'accueil
        var mainMenuScreen = new MainMenuScreen(Target);
        await mainMenuScreen.Show();
    }

    private async Task ActInRound()
    {
        Target.RemoveAll();

        var loadingDialog = new Dialog()
        {
            Width = 30,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Waiting for other players...",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        loadingDialog.Add(loadingText);

        Target.Add(loadingDialog);

        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
        };

        var httpClient = new HttpClient(httpHandler)
        {
            BaseAddress = new Uri($"{WssConfig.WebApiServerScheme}://{WssConfig.WebApiServerDomain}:{WssConfig.WebApiServerPort}"),
        };


        string ValuePayload = "{}";
        if (CurrentRoundAction == CurrentGameActionList.Action.RecruitAConsultant && CurrentView != null)
        {
            await Task.Delay(1000);
            ValuePayload = "{\"ConsultantId\":" + CurrentView.SelectedItemId + "}";
        }



        if (CurrentRoundAction == CurrentGameActionList.Action.FireAnEmployee && CurrentView != null)
        {
            await Task.Delay(1000);
            ValuePayload = "{\"EmployeeId\":" + CurrentView.SelectedItemId + "}";
        }

        if (CurrentRoundAction == CurrentGameActionList.Action.SendEmployeeForTraining && CurrentView != null)
        {
            var skills = new string[] { "Communication", "Programmation", "Reseau", "Cybersecurite", "Management" };

            // Titre et message centrés
            var title = "Training";
            var message = "Which skill would you like to level up ?";

            // Calculer la largeur nécessaire pour centrer le texte

            var centeredTitle = new string(' ',0) + title;
            var centeredMessage = new string(' ',0) + message;

            var numskill = MessageBox.Query(90, 5, centeredTitle, centeredMessage, skills);

            var nomskill = numskill switch
            {
                0 => "Communication",
                1 => "Programmation",
                2 => "Reseau",
                3 => "Cybersecurite",
                4 => "Management",
                _ => "Unknown"
            };

            var levelplus = new string[] { "1 level = 1 turn", "2 levels = 2 turns", "3 levels = 3 turns" };

            // Titre et message pour le niveau centrés
            var levelTitle = "Level";
            var levelMessage = "How much level do you want to level up this skill ?";

            var centeredLevelTitle = new string(' ', 0) + levelTitle;
            var centeredLevelMessage = new string(' ', 0) + levelMessage;

            var levelplusskill = MessageBox.Query(90, 5, centeredLevelTitle, centeredLevelMessage, levelplus);
            levelplusskill = 1;

            ValuePayload = $"{{\"EmployeeId\":{CurrentView.SelectedItemId}, \"nameofskillupgrade\":\"{nomskill}\", \"numberofleveltoimproveskill\":{levelplusskill}}}";
            Console.WriteLine("\n\n\n\n" + ValuePayload + "\n\n\n\n");
        }

        if (CurrentRoundAction == CurrentGameActionList.Action.ParticipateInCallForTenders && CurrentView != null)
        {
            var employees = new List<EmployeeOverview>();
            PlayerOverview leCurrentPlayer = CurrentGame!.Players.First(p => p.Name == PlayerName);
            foreach (var employee in leCurrentPlayer.Company.Employees.ToList())
            {
                employees.Add(employee);
            }

            var employeesAvailable = new List<string>();

            foreach (var employee in employees)
            {
                if (employee.enprojet == false && employee.enformation == false)
                {
                    var skillsemployee = new List<String>();
                    var skills = employee.Skills.ToList();

                    foreach (var skill in skills)
                    {
                        skillsemployee.Add($"{skill.Name} | {skill.Level}");
                    }
                    employeesAvailable.Add($"{employee.Name} - Skills: \n{string.Join("\n ", skillsemployee)}");
                }
                else
                {
                    //on ne fait rien l'employée n'est pas disponible=>rajouter une sécurité car il est possible que aucun employée ne soit disponible et qu'on soit bloquer dans la messagebox
                }
            }

            

            var employeeListString = employeesAvailable.ToArray();

            var employeeselectionnedfortheproject = MessageBox.Query(90 + ((employees.Count - 2 ) * 10), 10, "Call For tenders", "Which employee do you want to send to do this project", employeeListString);
            //je ne sais pas encore comment on va récupérer l'employeeid car on ne peut pas faire de recherche par nom car des employées peuvent avoir un même nom=>donc potentiellement rajoutée plus haut dans employeesAvailable une liste employee ID et si le résultat de la messagebox est le numéro de la réponse (réponse 0,1,2etc) on va faire listemployeeid[num de la réponse] afin de récupérer l'id correspondant à l'employée cliquer

            ValuePayload = $"{{\"CallForTendersId\":{CurrentView.SelectedItemId}, \"employeeid\":\"{employeeselectionnedfortheproject}\"}}";
            Console.WriteLine("\n\n\n\nYO : " + ValuePayload + "\n\n\n\n");
        }

        await Task.Delay(100);
        var request = httpClient.PostAsJsonAsync($"/rounds/{CurrentGame!.Rounds.MaxBy(r => r.Id)!.Id}/act", new
        {
            ActionType = CurrentRoundAction!.ToString(),
            ActionPayload = ValuePayload,
            PlayerId = CurrentGame.Players.First(p => p.Name == PlayerName).Id
        });


        await request;
    }
}

public abstract class CurrentGameView : View
{
    public int SelectedItemId = 0;
    public abstract Task Refresh(GameOverview game);
}

public class CurrentGameMainView : CurrentGameView
{
    private GameOverview Game;
    private readonly string PlayerName;

    private FrameView? Players;
    private FrameView? Status;
    private Button? StartButton;

    public EventHandler<HandledEventArgs> OnStart = (_, __) => { };

    public CurrentGameMainView(GameOverview game, string playerName)
    {
        Game = game;
        PlayerName = playerName;

        Width = Dim.Auto(DimAutoStyle.Auto);
        Height = Dim.Auto(DimAutoStyle.Auto);

        SetupPlayers();
        SetupStatus();
        SetupStartButton();
    }

    public override Task Refresh(GameOverview game)
    {
        Game = game;

        Remove(Players);
        Remove(Status);
        Remove(StartButton);

        SetupPlayers();
        SetupStatus();
        SetupStartButton();

        return Task.CompletedTask;
    }

    private void SetupPlayers()
    {
        Players = new()
        {
            Title = $"Players ({Game.PlayersCount}/{Game.MaximumPlayersCount})",
            X = 0,
            Y = 0,
            Width = 100,
            Height = 6 + Game.Players.Count,
            Enabled = false
        };

        Add(Players);

        var dataTable = new DataTable();

        dataTable.Columns.Add("Name");
        dataTable.Columns.Add("Company");
        dataTable.Columns.Add("Treasury");
        dataTable.Columns.Add("⭐");

        foreach (var player in Game.Players.ToList())
        {
            dataTable.Rows.Add([
                player.Name,
                player.Company.Name,
                $"{player.Company.Treasury} $",
                PlayerName == player.Name ? "⭐" : ""
            ]);
        }

        var dataTableSource = new DataTableSource(dataTable);

        var tableView = new TableView()
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = Game.Players.Max(p => p.Name.Length)
                + Game.Players.Max(p => p.Company.Name.Length)
                + Game.Players.Max(p => p.Company.Treasury.ToString().Length)
                + " $".Length
                + "⭐".Length
                + 6,
            Height = Dim.Fill(),
            Table = dataTableSource,
            Style = new TableStyle
            {
                ShowHorizontalBottomline = true,
                ExpandLastColumn = false,
            }
        };

        Players.Add(tableView);
    }

    private void SetupStatus()
    {
        Status = new()
        {
            Title = "Status",
            X = Pos.Left(Players!),
            Y = Pos.Bottom(Players!) + 2,
            Width = Players!.Width,
            Height = 3
        };

        Add(Status);

        var statusLabel = new Label() { Text = Game.Status is null ? "" : Game.Status, X = Pos.Center(), Y = Pos.Center() };

        Status.Add(statusLabel);
    }

    private void SetupStartButton()
    {
        if (PlayerName != Game.Players.First().Name) { return; }

        StartButton = new()
        {
            X = Pos.Center(),
            Y = Pos.Bottom(Status!) + 2,
            Width = Dim.Auto(DimAutoStyle.Text),
            Height = Dim.Auto(DimAutoStyle.Text),
            Text = "Start Game",
            // Enabled = Game.Players.Count >= 2,
            Enabled = true,
        };

        Console.WriteLine("\n\n\n\n" + Game.Players.Count() + "\n\n\n\n");

        StartButton.Accept += async (_, __) => await StartGame();

        Add(StartButton);

        StartButton.SetFocus();
    }

    private async Task StartGame()
    {
        RemoveAll();

        var loadingDialog = new Dialog()
        {
            Width = 18,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Starting game...",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        loadingDialog.Add(loadingText);

        Add(loadingDialog);

        var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, __, ___, ____) => true
        };

        var httpClient = new HttpClient(httpHandler)
        {
            BaseAddress = new Uri($"{WssConfig.WebApiServerScheme}://{WssConfig.WebApiServerDomain}:{WssConfig.WebApiServerPort}"),
        };

        var request = httpClient.PostAsJsonAsync($"/games/{Game.Id}/start", new { });
        var response = await request;

        if (!response.IsSuccessStatusCode)
        {
            await Refresh(Game);
        }
        else
        {
            OnStart(null, new HandledEventArgs());
        }
    }
}

public class CurrentGameCompanyView : CurrentGameView
{
    private GameOverview Game;
    private PlayerOverview CurrentPlayer;
    private readonly string PlayerName;
    public EventHandler<CurrentGameActionList.Action> OnRoundAction = (_, __) => { };

    private View? Header;
    private View? Body;
    private View? LeftBody;
    private View? RightBody;

    private FrameView? Player;
    private FrameView? Company;
    private FrameView? Treasury;
    private FrameView? Rounds;
    private FrameView? Employees;
    private FrameView? Consultants;
    private FrameView? CallForTenders;
    private FrameView? Actions;

    public CurrentGameCompanyView(GameOverview game, string playerName)
    {
        Game = game;
        PlayerName = playerName;
        CurrentPlayer = Game.Players.First(p => p.Name == PlayerName);

        Width = Dim.Auto(DimAutoStyle.Auto);
        Height = Dim.Auto(DimAutoStyle.Auto);

        SetupHeader();
        SetupBody();
    }

    public override Task Refresh(GameOverview game)
    {
        Game = game;
        CurrentPlayer = Game.Players.First(p => p.Name == PlayerName);

        RemoveHeader();
        RemoveBody();

        SetupHeader();
        SetupBody();

        return Task.CompletedTask;
    }

    private void SetupHeader()
    {
        Header = new()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Auto(DimAutoStyle.Content)
        };

        SetupPlayer();
        SetupCompany();
        SetupTreasury();
        SetupRounds();

        Add(Header);
    }

    private void SetupBody()
    {
        Body = new()
        {
            X = 0,
            Y = Pos.Bottom(Header!) + 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        SetupLeftBody();
        SetupRightBody();

        Add(Body);
    }

    private void SetupLeftBody()
    {
        LeftBody = new()
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(80),
            Height = Dim.Fill()
        };

        SetupEmployees();
        SetupConsultants();
        SetupCallForTenders();

        Body!.Add(LeftBody);
    }

    private void SetupRightBody()
    {
        RightBody = new()
        {
            X = Pos.Right(LeftBody!),
            Y = Pos.Top(LeftBody!),
            Width = Dim.Percent(20),
            Height = Dim.Fill()
        };

        SetupActions();

        Body!.Add(RightBody);
    }

    private void SetupPlayer()
    {
        Player = new()
        {
            Title = "Player",
            Width = Dim.Percent(25),
            Height = Dim.Auto(DimAutoStyle.Content),
            X = 0,
            Y = 0
        };

        Player.Add(new Label { Text = CurrentPlayer.Name });

        Header!.Add(Player);
    }

    private void SetupCompany()
    {
        Company = new()
        {
            Title = "Company",
            Width = Dim.Percent(25),
            Height = Dim.Auto(DimAutoStyle.Content),
            X = Pos.Right(Player!),
            Y = 0
        };

        Company.Add(new Label { Text = CurrentPlayer.Company.Name });

        Header!.Add(Company);
    }

    private void SetupTreasury()
    {
        Treasury = new()
        {
            Title = "Treasury",
            Width = Dim.Percent(25),
            Height = Dim.Auto(DimAutoStyle.Content),
            X = Pos.Right(Company!),
            Y = 0
        };

        Treasury.Add(new Label { Text = $"{CurrentPlayer.Company.Treasury} $" });

        Header!.Add(Treasury);
    }

    private void SetupRounds()
    {
        Rounds = new()
        {
            Title = "Rounds",
            Width = Dim.Percent(25),
            Height = Dim.Auto(DimAutoStyle.Content),
            X = Pos.Right(Treasury!),
            Y = 0
        };

        Rounds.Add(new Label { Text = $"{Game.CurrentRound}/{Game.MaximumRounds}" });

        Header!.Add(Rounds);
    }

    private void SetupEmployees()
    {
        Employees = new()
        {
            Title = "Employees",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Percent(40)
        };

        var employeesTree = new TreeView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.Dotted
        };

        var employeesData = new List<TreeNode>();
        int selectedEmployeeId = 0; // Variable pour stocker l'employé sélectionné

        foreach (var employee in CurrentPlayer.Company.Employees.ToList())
        {
            // Ajoute un préfixe [ ] ou [x] pour simuler la sélection avec des "boutons radio"
            var isSelected = employee.Id == selectedEmployeeId ? "[x]" : "[ ]";
            var isoccuped = employee.enformation == true ? "[statue : in training]" : "[Statue : Free]";
            var HowmuchTurn = employee.dureeformation >= 1 ? "[Duration : " + employee.dureeformation + " Turn]" : "";
            if (isoccuped == "[statue : in training]")
            {
                //on ne fait rien
                Console.WriteLine("\n\nil est en formation\n\n");
            }
            else
            {
                //cependant s'il n'est pas en formation on vérifie s'il est en projet
                isoccuped = employee.enprojet == true ? "[statue : in project]" : "[Statue : Free ]";
            }
            var node = new TreeNode($"{isSelected} {employee.Name} | {employee.Salary} $ {isoccuped} {HowmuchTurn}")
            {
                Tag = employee.Id // Ajoute l'ID au nœud comme Tag
            };

            var skills = employee.Skills.ToList();

            foreach (var skill in skills)
            {
                node.Children.Add(new TreeNode($"{skill.Name} | {skill.Level}"));
            }

            employeesData.Add(node);
        }

        employeesTree.SelectionChanged += (sender, args) =>
        {
            if (args.NewValue is TreeNode selectedNode && selectedNode.Tag is int EmployeeId)
            {
                SelectedItemId = EmployeeId;
                // Stocke l'ID de l'employé sélectionné
                selectedEmployeeId = EmployeeId;

                // Met à jour la sélection en changeant le préfixe des employés
                foreach (var node in employeesData)
                {
                    node.Text = node.Tag.Equals(EmployeeId)
                        ? node.Text.Replace("[ ]", "[x]")
                        : node.Text.Replace("[x]", "[ ]");
                }

                //on appelle setupconsultant afin d'enlever la sélection du consultant
                SetupConsultants();
                // SetupCallForTenders();
                // Rafraîchit l'affichage pour refléter les changements
                employeesTree.SetNeedsDisplay();
            }
        };

        employeesTree.BorderStyle = LineStyle.None;
        employeesTree.AddObjects(employeesData);
        employeesTree.ExpandAll();

        Employees.Add(employeesTree);

        LeftBody!.Add(Employees);
    }

    private void SetupConsultants()
    {
        Consultants = new()
        {
            Title = "Consultants",
            X = Pos.Left(Employees!),
            Y = Pos.Bottom(Employees!) + 1,
            Width = Dim.Fill(),
            Height = Dim.Percent(30)
        };

        var consultantsTree = new TreeView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.Dotted
        };

        var consultantsData = new List<TreeNode>();
        int selectedConsultantId = 0; // Variable pour stocker le consultant sélectionné

        foreach (var consultant in Game.Consultants.ToList()) // Pour chaque consultant disponible.
        {
            var isSelected = consultant.Id == selectedConsultantId ? "[x]" : "[ ]";
            var node = new TreeNode($"{isSelected} {consultant.Name} | {consultant.SalaryRequirement} $")
            {
                Tag = consultant.Id // Ajoute l'ID au nœud comme Tag
            };
            var skills = consultant.Skills.ToList(); // Récupère les compétences du consultant.

            foreach (var skill in skills) // Pour chaque compétence.
            {
                node.Children.Add(new TreeNode($"{skill.Name} | {skill.Level}")); // Ajoute la compétence au nœud.
            }

            consultantsData.Add(node); // Ajoute le nœud à la liste des consultants.
        }

        consultantsTree.SelectionChanged += (sender, args) =>
        {
            if (args.NewValue is TreeNode selectedNode && selectedNode.Tag is int consultantId)
            {
                SelectedItemId = consultantId;

                // Stocke l'ID de l'employé sélectionné
                selectedConsultantId = consultantId;

                // Met à jour la sélection en changeant le préfixe des employés
                foreach (var node in consultantsData)
                {
                    node.Text = node.Tag.Equals(consultantId)
                        ? node.Text.Replace("[ ]", "[x]")
                        : node.Text.Replace("[x]", "[ ]");
                }

                //on appelle setupemployee afin d'enlever la sélection de l'employée
                SetupEmployees();
                // SetupCallForTenders();
                // Rafraîchit l'affichage pour refléter les changements
                consultantsTree.SetNeedsDisplay();
            }
        };

        consultantsTree.BorderStyle = LineStyle.None; // Supprime le style de bordure.
        consultantsTree.AddObjects(consultantsData); // Ajoute les nœuds au TreeView.
        consultantsTree.ExpandAll(); // Développe tous les nœuds.



        Consultants.Add(consultantsTree);

        LeftBody!.Add(Consultants);
    }

    private void SetupCallForTenders()
    {
        CallForTenders = new()
        {
            Title = "Call For Tenders",
            X = Pos.Left(Consultants!),
            Y = Pos.Bottom(Consultants!) + 1,
            Width = Dim.Fill(),
            Height = Dim.Percent(30)
        };

        var callfortendersTree = new TreeView()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            BorderStyle = LineStyle.Dotted
        };

        //     //Ci-dessous il suffit d'appeler Game.project.ToList() et de faire la boucle foreach avec les infos de projets

        //     var callfortendersData = new List<TreeNode>();
        //     int selectedCallFortendersId = 0; // Variable pour stocker le projet sélectionné

        //     foreach (var project in Game.Projects.ToList())
        //     {
        //         var isSelected = project.Id == selectedCallFortendersId ? "[x]" : "[ ]";
        //         var node = new TreeNode($"{isSelected} {project.Name} | {project.Revenu} $")
        //         {
        //             Tag = project.Id // Ajoute l'ID au nœud comme Tag
        //         };
        //         var skills = project.Skills.ToList(); // Récupère les compétences nécessaire au projet.

        //         foreach (var skill in skills) // Pour chaque compétence.
        //         {
        //             node.Children.Add(new TreeNode($"{skill.Name} | {skill.Level}")); // Ajoute la compétence au nœud.
        //         }

        //         callfortendersData.Add(node); // Ajoute le nœud à la liste des projets.
        //     }

        //     callfortendersTree.SelectionChanged += (sender, args) =>
        //     {
        //         if (args.NewValue is TreeNode selectedNode && selectedNode.Tag is int callfortendersId)
        //         {
        //             SelectedItemId = callfortendersId;

        //             // Stocke l'ID de l'employé sélectionné
        //             selectedCallFortendersId = callfortendersId;

        //             // Met à jour la sélection en changeant le préfixe des employés
        //             foreach (var node in callfortendersData)
        //             {
        //                 node.Text = node.Tag.Equals(callfortendersId)
        //                     ? node.Text.Replace("[ ]", "[x]")
        //                     : node.Text.Replace("[x]", "[ ]");
        //             }

        //             //on appelle setupemployee et setupconsultant afin d'enlever la sélection de l'employée
        //             SetupEmployees();
        //             SetupConsultants();
        //             // Rafraîchit l'affichage pour refléter les changements
        //             callfortendersTree.SetNeedsDisplay();
        //         }
        //     };

        //     callfortendersTree.BorderStyle = LineStyle.None; // Supprime le style de bordure.
        //     callfortendersTree.AddObjects(callfortendersData); // Ajoute les nœuds au TreeView.
        //     callfortendersTree.ExpandAll(); // Développe tous les nœuds.



        //          CallForTenders.Add(callfortendersTree);

        LeftBody!.Add(CallForTenders);
    }

    private void SetupActions()
    {
        Actions = new()
        {
            Title = "Actions",
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        var actionList = new CurrentGameActionList()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        actionList.CanFocus = true;
        actionList.OpenSelectedItem += (_, selected) =>
        {
            var selectedAction = (CurrentGameActionList.Action) selected.Value;

            if (selectedAction == CurrentGameActionList.Action.FireAnEmployee)
            {
                var selectedEmployee = CurrentPlayer.Company.Employees.FirstOrDefault(e => e.Id == SelectedItemId);

                if (selectedEmployee == null)
                {
                    MessageBox.ErrorQuery("Action Unavailable", "You must select an employee to fire.", "OK");
                    return;
                }
            }

            if (selectedAction == CurrentGameActionList.Action.RecruitAConsultant)
            {
                var selectedConsultant = Game.Consultants.FirstOrDefault(c => c.Id == SelectedItemId);

                if (selectedConsultant == null)
                {
                    MessageBox.ErrorQuery("Action Unavailable", "You must select a consultant to recruit.", "OK");
                    return;
                }
            }

            // Désactiver "SendEmployeeForTraining" si un employé n'est pas sélectionné
            if (selectedAction == CurrentGameActionList.Action.SendEmployeeForTraining)
            {
                var selectedEmployee = CurrentPlayer.Company.Employees.FirstOrDefault(e => e.Id == SelectedItemId);

                if (selectedEmployee == null)
                {
                    MessageBox.ErrorQuery("Action Unavailable", "You must select an employee to send for training.", "OK");
                    return;
                }
            }

            OnRoundAction(null, selectedAction);
        };

        Actions.Add(actionList);
        actionList.SetFocus();
        actionList.MoveHome();

        RightBody!.Add(Actions);
    }





    private void RemoveHeader()
    {
        Header!.Remove(Player);
        Header!.Remove(Company);
        Header!.Remove(Treasury);
        Header!.Remove(Rounds);

        Remove(Header);
    }

    private void RemoveBody()
    {
        LeftBody!.Remove(Employees);
        LeftBody!.Remove(Consultants);
        LeftBody!.Remove(CallForTenders);

        RightBody!.Remove(Actions);

        Body!.Remove(LeftBody);
        Body!.Remove(RightBody);

        Remove(Body);
    }
}

public class CurrentGameActionList : ListView
{
    public enum Action
    {
        SendEmployeeForTraining,
        ParticipateInCallForTenders,
        RecruitAConsultant,
        FireAnEmployee,
        PassMyTurn
    }

    private readonly CurrentGameActionListDataSource Actions = [
        Action.SendEmployeeForTraining,
        Action.ParticipateInCallForTenders,
        Action.RecruitAConsultant,
        Action.FireAnEmployee,
        Action.PassMyTurn
    ];

    public CurrentGameActionList()
    {
        Source = Actions;
    }
}

public class CurrentGameActionListDataSource : List<CurrentGameActionList.Action>, IListDataSource
{
    public int Length => Count; // Propriété pour obtenir le nombre d'actions.
    private HashSet<int> markedItems = new HashSet<int>(); // Pour garder une trace des éléments marqués.


    public bool SuspendCollectionChangedEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event NotifyCollectionChangedEventHandler CollectionChanged = (_, __) => { };

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public bool IsMarked(int item)
    {
        Console.WriteLine(item);
        return markedItems.Contains(item); // Vérifie si l'élément est marqué.
    }

    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
    {
        switch (item)
        {
            case (int) CurrentGameActionList.Action.SendEmployeeForTraining:
                driver.AddStr("Send Employee For Training");
                break;
            case (int) CurrentGameActionList.Action.ParticipateInCallForTenders:
                driver.AddStr("Participate In Call For Tenders");
                break;
            case (int) CurrentGameActionList.Action.RecruitAConsultant:
                driver.AddStr("Recruit A Consultant");
                break;
            case (int) CurrentGameActionList.Action.FireAnEmployee:
                driver.AddStr("Fire An Employee");
                break;
            case (int) CurrentGameActionList.Action.PassMyTurn:
                driver.AddStr("Pass My Turn");
                break;
        }
    }

    public void SetMark(int item, bool value) { }

    public IList ToList()
    {
        return this;
    }
}
