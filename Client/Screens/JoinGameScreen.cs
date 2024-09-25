using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net.Http.Json;

using Client.Records;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

using Terminal.Gui;

namespace Client.Screens;

public class JoinGameScreen(Window target)
{
    private Window Target { get; } = target; // Permet de stocker l'écran souhaité
    private readonly ListView GamesList = new(); // Stock la liste des parties créé
    private readonly Button ListReturnButton = new() { Text = "Return" }; // Bouton de retour
    private readonly JoinGameForm Form = new(); // Formulaire de création d'une partie
    private readonly Label textviewerrorlist = new ()
    {
        Text = "No games available at the moment.",
        X = Pos.Center(),  // Position centrale dans la fenêtre
        Y = Pos.Center(),
        Visible = false    // On cache l'élément par défaut
    };

    private ICollection<JoinableGame> _joinableGames = []; // Listes des parties joinable
    private ICollection<JoinableGame> JoinableGames
    {
        get => _joinableGames;
        set { _joinableGames = value; ReloadListData(); }
    }
    private int? GameId = null;

    private bool Loading = true;
    private bool Errored = false;
    private bool ListReturned = false;
    private bool FormReturned = false;
    private bool FormSubmitted = false;

    public async Task Show() // Permet d'afficher toute les informations
    {
        await BeforeShow(); // Attend que toute ces actions soient fini pour passer aux lignes suivantes

        await LoadGames();

        await SelectGame();

        if (ListReturned)
        {
            await Return();
            return;
        }

        await DisplayForm();

        if (FormReturned)
        {
            await SelectGame();
            return;
        }

        await JoinGame();

        if (Errored) // Gestion des erreurs
        {
            FormSubmitted = false;
            FormReturned = false;

            await DisplayForm(true);

            if (FormReturned)
            {
                await SelectGame();
                return;
            }

            return;
        }

        var currentGameScreen = new CurrentGameScreen(Target, (int) GameId!, Form.PlayerNameField.Text.ToString()!); // Création et Stockage de la fenetre de la partie actuel

        await currentGameScreen.Show(); // Affichage de la fenetre
    }

    private Task BeforeShow() // Nettoyage des anciennes fenetres, et affichage du titre
    {
        Target.RemoveAll();
        Target.Title = $"{MainWindow.Title} - [Join Game]";

        Target.Add(textviewerrorlist);

        return Task.CompletedTask;
    }

    private async Task Return() // Gestion de l'affichage du menu d'accueil quand on faire retour
    {
        var mainMenuScreen = new MainMenuScreen(Target);
        await mainMenuScreen.Show();
    }

    private void ReloadListData() // Permet d'actualiser les listes des parties joinable
    {
        var dataSource = new JoinGameChoiceListDataSource();
        dataSource.AddRange(JoinableGames);
        GamesList.Source = dataSource;

        if (JoinableGames.Count == 0)
        {
            textviewerrorlist.Visible = true;
            GamesList.Visible = false;
        }
        else
        {
            textviewerrorlist.Visible = false;
            GamesList.Visible = true;
        }
    }

    private async Task LoadGames() // Chargement de la liste des parties joinable
    {
        var hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri($"{WssConfig.WebSocketServerScheme}://{WssConfig.WebSocketServerDomain}:{WssConfig.WebSocketServerPort}/main"), opts =>
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

        hubConnection.On<ICollection<JoinableGame>>("JoinableGamesListUpdated", data =>
        {
            JoinableGames = data ?? new List<JoinableGame>();
            Loading = false;
            
            if (JoinableGames.Count == 0)
            {
                textviewerrorlist.Visible = true;
                GamesList.Visible = false;  // Masquer la liste
            }
            else
            {
                textviewerrorlist.Visible = false;
                GamesList.Visible = true;   // Afficher la liste si des jeux sont présents
            }
        });

        await hubConnection.StartAsync();

        var loadingDialog = new Dialog()
        {
            Width = 18,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Loading games...",
            X = Pos.Center(),
            Y = Pos.Center()
        };

        loadingDialog.Add(loadingText);

        Target.Add(loadingDialog);

        while (Loading) { await Task.Delay(100); }

        Target.Remove(loadingDialog);
    }

    private async Task SelectGame() // Gestion de la sélection d'une partie
    {
        GamesList.X = GamesList.Y = Pos.Center();
        GamesList.Width = JoinableGames.Max(g => g.Name.Length);
        GamesList.Height = Math.Min(JoinableGames.Count, 20);

        ListReturnButton.X = Pos.Center();
        ListReturnButton.Y = Pos.Bottom(GamesList) + 1;
        ListReturnButton.Accept += (_, __) => ListReturned = true;

        //textviewerrorlist.X = Pos.Center();
        //textviewerrorlist.Y = Pos.Bottom(ListReturnButton) + 1;

        Target.Add(GamesList);
        Target.Add(ListReturnButton);
        //Target.Add(textviewerrorlist);

        GamesList.OpenSelectedItem += (_, selected) => GameId = ((JoinableGame) selected.Value).Id;
        GamesList.SetFocus();
        GamesList.MoveHome();

        while (GameId is null && !ListReturned) { await Task.Delay(100); };
    }

    private async Task DisplayForm(bool errored = false) // Affiche le formulaire de création d'une partie
    {
        Target.RemoveAll();

        Form.OnReturn = (_, __) => FormReturned = true;
        Form.OnSubmit = (_, __) => FormSubmitted = true;

        Form.FormView.X = Form.FormView.Y = Pos.Center();
        Form.FormView.Width = 50;
        Form.FormView.Height = 9;

        Target.Add(Form.FormView);

        while (!FormReturned && !FormSubmitted)
        {
            await Task.Delay(100);
        }
    }

    private async Task JoinGame() // Gestion pour rejoindre une game
    {
        Target.Remove(Form.FormView);

        var loadingDialog = new Dialog()
        {
            Width = 17,
            Height = 3
        };

        var loadingText = new Label()
        {
            Text = "Joining game...",
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

        var playerName = Form.PlayerNameField.Text.ToString();
        var companyName = Form.CompanyNameField.Text.ToString();

        var requestBody = new { playerName, companyName };
        var request = httpClient.PostAsJsonAsync($"/games/{GameId}/join", requestBody);
        var response = await request;

        if (!response.IsSuccessStatusCode)
        {
            Errored = true;
        }
    }
}

public class JoinGameChoiceListDataSource : List<JoinableGame>, IListDataSource
{
    public int Length => Count;

    public bool SuspendCollectionChangedEvent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public event NotifyCollectionChangedEventHandler CollectionChanged = (_, __) => { };

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public bool IsMarked(int item)
    {
        return false;
    }

    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
    {
        var game = this[item];
        driver.AddStr($"{game.Name} ({game.PlayersCount}/{game.MaximumPlayersCount})");
    }

    public void SetMark(int item, bool value) { }

    public IList ToList()
    {
        return this;
    }
}

public class JoinGameForm
{
    private EventHandler<HandledEventArgs> _onSubmit = (_, __) => { };
    private EventHandler<HandledEventArgs> _onReturn = (_, __) => { };

    public EventHandler<HandledEventArgs> OnSubmit // Gestion des actions du bouton "submit"
    {
        get => _onSubmit;
        set
        {
            SubmitButton.Accept -= _onSubmit;
            SubmitButton.Accept += value;
            _onSubmit = value;
        }
    }
    public EventHandler<HandledEventArgs> OnReturn // Gestion des actions du bouton "retour"
    {
        get => _onReturn;
        set
        {
            ReturnButton.Accept -= _onReturn;
            ReturnButton.Accept += value;
            _onReturn = value;
        }
    }

    // Elements de l'affichage
    public View FormView { get; }
    public View ButtonsView { get; }
    public Button SubmitButton { get; }
    public Button ReturnButton { get; }
    public Label PlayerNameLabel { get; }
    public Label CompanyNameLabel { get; }
    public TextField PlayerNameField { get; }
    public TextField CompanyNameField { get; }

    public JoinGameForm() // Formulaire de création d'une game
    {
        PlayerNameLabel = new Label() // Texte d'information
        {
            X = 0,
            Y = 0,
            Width = 20,
            Text = "Player name :"
        };

        CompanyNameLabel = new Label() // Texte d'information
        {
            X = Pos.Left(PlayerNameLabel),
            Y = Pos.Bottom(PlayerNameLabel) + 1,
            Width = 20,
            Text = "Company name :"
        };

        PlayerNameField = new TextField() // Champs remplissable du nom du joueur
        {
            X = Pos.Right(PlayerNameLabel),
            Y = Pos.Top(PlayerNameLabel),
            Width = Dim.Fill(),
            Text = ""
        };

        CompanyNameField = new TextField() // Champs remplissable du nom de la compagnie du joueur
        {
            X = Pos.Right(CompanyNameLabel),
            Y = Pos.Top(CompanyNameLabel),
            Width = Dim.Fill(),
            Text = ""
        };

        ButtonsView = new View() // Modifie la position des boutons
        {
            Width = 1,
            Height = 1,
            X = Pos.Center(),
            Y = Pos.Bottom(CompanyNameLabel) + 1
        };

        SubmitButton = new Button() // Bouton de validation pour créer une partie
        {
            Text = "Submit",
            IsDefault = true
        };

        ReturnButton = new Button() // Bouton retour (pour revenir au menu d'accueil)
        {
            Text = "Return",
            IsDefault = false,
            X = Pos.Right(SubmitButton) + 1
        };

        SubmitButton.Accept += OnSubmit;
        ReturnButton.Accept += OnReturn;

        ButtonsView.Add(SubmitButton, ReturnButton); // Ajoute les boutons à l'affichage

        var submitButtonWidth = SubmitButton.Width;
        var returnButtonWidth = ReturnButton.Width;

        ButtonsView.Width = submitButtonWidth + returnButtonWidth + 1;

        FormView = new View()
        {
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        FormView.Add( // Permet d'afficher tout les éléments au joueur
            PlayerNameLabel, CompanyNameLabel,
            PlayerNameField, CompanyNameField,
            ButtonsView
        );
    }
}
