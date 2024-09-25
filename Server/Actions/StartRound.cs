

using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record StartRoundParams(int? GameId = null, Game? Game = null);

public class StartRoundValidator : AbstractValidator<StartRoundParams>
{
    public StartRoundValidator()
    {
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class StartRound(
    IGamesRepository gamesRepository,
    IRoundsRepository roundsRepository,
    IGameHubService gameHubService,
    IAction<CreateConsultantParams, Result<Consultant>> consultant, // Nécessaire pour créer un consultant 
    IAction<RemoveConsultantParams, Result<Consultant>> removeConsultant // Nécessaire pour supprimer un consultant 
) : IAction<StartRoundParams, Result<Round>>
{
    public async Task<Result<Round>> PerformAsync(StartRoundParams actionParams)
    {
        var actionValidator = new StartRoundValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (gameId, game) = actionParams;

        game ??= await gamesRepository.GetById(gameId!.Value);

        if (game is null)
        {
            return Result.Fail($"Game with Id \"{gameId}\" not found.");
        }

        if (!game!.CanStartANewRound())
        {
            return Result.Fail("Game cannot start a new round.");
        }

        var round = new Round(game.Id!.Value, game.RoundsCollection.Count + 1);

        try
        {
            // Récupère la liste des consultants pour le jeu actuel
            var NbConsultants = await gamesRepository.GetConsultant(game.Id!.Value);
            var consultantsList = NbConsultants!.Consultants.ToList();

            foreach (var consultant in consultantsList)
            {
                var removeConsultantParams = new RemoveConsultantParams(consultant, gameId, game);
                var removeConsultantResult = await removeConsultant.PerformAsync(removeConsultantParams);

                if (removeConsultantResult.IsFailed)
                {
                    Console.WriteLine($"Impossible de supprimer le consultant : {consultant.Id}: {string.Join(", ", removeConsultantResult.Errors.Select(e => e.Message))}");
                    continue;
                }
                await gameHubService.UpdateCurrentGame(gameId: round.GameId);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur lors de la suppression des consultants : " + ex.Message);
        }

        // Génère un nombre de consultants aléatoire en fonction du nombre de joueurs dans la partie
        for (var i = 0; i <= game.Players.Count; i++)
        {
            List<String> nameList = ["Mr.Martin", "Mme.Jeanne", "Mr.Bob", "Mme.Lucie", "Mr.Paul", "Mr.Jacques", "Mme.Catherine", "Mr.Jean", "Mme.Sophie", "Mr.Pierre", "Mme.Claire", "Mr.Francois", "Mr.Eric", "Mme.Charlotte", "Mr.Louis", "Mme.Marie", "Mr.Thierry", "Mme.Valerie", "Mr.Arthur", "Mme.Elise", "Mr.Thomas", "Mme.Laure"];
            int randomName = new Random().Next(0, nameList.Count);

            var createConsultantParams = new CreateConsultantParams(nameList[randomName], gameId, game);
            var createConsultantResult = await consultant.PerformAsync(createConsultantParams);
        }

        // Enregistre et actualise le tour
        await roundsRepository.SaveRound(round);
        await gameHubService.UpdateCurrentGame(gameId: round.GameId);

        return Result.Ok(round);
    }
}
