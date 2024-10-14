using FluentResults;

using FluentValidation;

using Server.Actions.Contracts;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

using static Server.Models.RoundAction;

namespace Server.Actions;

public sealed record ActInRoundParams(
    RoundActionType ActionType,
    RoundActionPayload ActionPayload,
    int? RoundId = null,
    Round? Round = null,
    int? PlayerId = null,
    Player? Player = null
);

public class ActInRoundValidator : AbstractValidator<ActInRoundParams>
{
    public ActInRoundValidator()
    {
        RuleFor(p => p.ActionType).NotEmpty();
        RuleFor(p => p.ActionPayload).NotEmpty();
        RuleFor(p => p.RoundId).NotEmpty().When(p => p.Round is null);
        RuleFor(p => p.Round).NotEmpty().When(p => p.RoundId is null);
        RuleFor(p => p.PlayerId).NotEmpty().When(p => p.Player is null);
        RuleFor(p => p.Player).NotEmpty().When(p => p.PlayerId is null);
    }//permet de validé si on peut éxecuter ActInRound en vérifiant si toutes les variables dont on a besoin on une valeur
}

public class ActInRound(//ici on a en paramètre les interfaces I qui implémente obligatoirement des méthodes ou des variables
    IRoundsRepository roundsRepository,
    IPlayersRepository playersRepository,
    IAction<FinishRoundParams, Result<Round>> finishRoundAction,
    IGameHubService gameHubService
) : IAction<ActInRoundParams, Result<Round>>
{
    public async Task<Result<Round>> PerformAsync(ActInRoundParams actionParams)
    {
        var actionValidator = new ActInRoundValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        //on appelle la méthode permettant de vérifier si les variables sont bonne et si non on renvoie une erreur 
        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        //cela permet de séparer (explode) toutes les variables d'actionParams en variable individuelle en une seule ligne
        var (actionType, actionPayload, roundId, round, playerId, player) = actionParams;

        Console.WriteLine("\n\n\n" + actionType + ", " + playerId);

        //si la valeur round est null alors on exécute le code se trouvant après le =
        //cela permet de récupérer l'objet round à partir de son Id si on n'avais pas déjà cette objet
        round ??= await roundsRepository.GetById(roundId!.Value);

        if (round is null)
        {
            Result.Fail($"Round with Id \"{roundId}\" not found.");
        }

        player ??= await playersRepository.GetById(playerId!.Value);

        if (player is null)
        {
            Result.Fail($"Player with Id \"{playerId}\" not found.");
        }

        //si le joueur ne peut pas faire d'action dans ce tour on renvoie une erreur
        if (!round!.CanPlayerActIn(player!.Id!.Value))
        {
            return Result.Fail("Player cannot act in this round.");
        }

        var roundAction = CreateForType(actionType, player.Id.Value, actionPayload);

        round.Actions.Add(roundAction);

        Console.WriteLine(actionType + ", " + playerId + "\n\n\n");

        await roundsRepository.SaveRound(round);

        Console.WriteLine("REPOBUG \n\n\n" + round.Id);
        Console.WriteLine("REPOBUG \n\n\n" + roundsRepository);


        //grâce à une méthode on créer en fonction de l'action du tour une variable roundAction qui est ajouté puis on l'ajoute à notre classe round
        //cela est ensuite sauvegarder sur le repo
        if (round.EverybodyPlayed())
        {
            Console.WriteLine("\n\n\n" + round);
            var finishRoundParams = new FinishRoundParams(Round: round);
            var finishRoundResult = await finishRoundAction.PerformAsync(finishRoundParams);

            Console.WriteLine("\n\n\n" + finishRoundParams + ", " + finishRoundResult);

            if (finishRoundResult.IsFailed)
            {
                return Result.Fail(finishRoundResult.Errors);
            }
        }
        //si tout le monde à jouer son action, on appelle la classe finishRound qui va appeler la classe ApplyRoundAction
        //puis on vérifie si cela à réussie ou non

        // Mise à jour du jeux
        await gameHubService.UpdateCurrentGame(gameId: round.GameId);

        return Result.Ok(round);
    }
}
