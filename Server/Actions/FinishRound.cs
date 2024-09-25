using FluentResults;

using FluentValidation;

using Server.Actions.Contracts;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence;
using Server.Persistence.Contracts;

using static Server.Models.GenerateNewConsultantRoundAction;

namespace Server.Actions;

public sealed record FinishRoundParams(int? RoundId = null, Round? Round = null);

public class FinishRoundValidator : AbstractValidator<FinishRoundParams>
{
    public FinishRoundValidator()
    {
        RuleFor(p => p.RoundId).NotEmpty().When(p => p.Round is null);
        RuleFor(p => p.Round).NotEmpty().When(p => p.RoundId is null);
    }
}
//méthode permettant de valider si les variables répondent au condition du bon fonctionnement de la classe

public class FinishRound(
    IRoundsRepository roundsRepository,
    ICompaniesRepository companiesRepository,
    IAction<ApplyRoundActionParams, Result> applyRoundActionAction,
    IAction<StartRoundParams, Result<Round>> startRoundAction,
    IAction<FinishGameParams, Result<Game>> finishGameAction,
    IGameHubService gameHubService
) : IAction<FinishRoundParams, Result<Round>>
{
    public async Task<Result<Round>> PerformAsync(FinishRoundParams actionParams)
    {
        var actionValidator = new FinishRoundValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }
        //on vérifie grâce à la méthode créer plus haut si les variables sont correcte et si non on renvoie une erreur

        var (roundId, round) = actionParams;
        //on initialise toutes les variables se trouvant dans actionParams afin de les utiliser individuellement

        round ??= await roundsRepository.GetById(roundId!.Value);
        //si la valeur round est null alors on exécute le code se trouvant après le =
        //cela permet de récupérer l'objet round à partir de son Id si on n'avais pas déjà cette objet

        if (round is null)
        {
            return Result.Fail($"Round with Id \"{roundId}\" not found.");
        }
        
        var rnd = new Random();

        var newConsultantShouldBeGenerated = rnd.Next(2) == 1;
        //on génère au hasard de nouveau consultant

        if (newConsultantShouldBeGenerated)
        {
            var action = RoundAction.CreateForType(
                RoundActionType.GenerateNewConsultant,
                0,
                new GenerateNewConsultantPayload { GameId = round.GameId }
            );

            round.Actions.Add(action);

            await roundsRepository.SaveRound(round);
        }
        //si on créer de nouveau consultant on créer une action de tour pour le type GenerateNewConsultant
        //on ajoute cette action au round puis on sauvegarde cela dans le repos

        foreach (var action in round.Actions)
        {
            var applyRoundActionParams = new ApplyRoundActionParams(RoundAction: action, Game: round.Game);
            var applyRoundActionResult = await applyRoundActionAction.PerformAsync(applyRoundActionParams);

            if (applyRoundActionResult.IsFailed)
            {
                return Result.Fail(applyRoundActionResult.Errors);
            }
        }
        //pour chaque actions dans le round on va appeler ApplyRounAction afin d'appliquer chaque action une par une
        //puis on vérifie si elles ont échoué et si oui on renvoie une erreur

        if (round.Game.CanStartANewRound())
        {

            var startRoundActionParams = new StartRoundParams(Game: round.Game);
            var startRoundActionResult = await startRoundAction.PerformAsync(startRoundActionParams);
            var newRound = startRoundActionResult.Value;

            foreach (var unplayer in round.Game.Players)
            {
                unplayer.Company?.DebitSalary();

                await companiesRepository.SaveAllCompany();
            }
            //permet de débiter le salire de chaque joueur et de sauvegarder les companys juste après

            await gameHubService.UpdateCurrentGame(gameId: round.GameId);

            return Result.Ok(newRound);
        }//on vérifie si on peut démarrer un nouveau tour, si oui on le démarre avec la classe StartRound
         //puis on met à jour le jeu et on renvoie en return le newround
        else
        {
            var finishGameActionParams = new FinishGameParams(Game: round.Game);
            var finishGameActionResult = await finishGameAction.PerformAsync(finishGameActionParams);

            if (finishGameActionResult.IsFailed)
            {
                return Result.Fail(finishGameActionResult.Errors);
            }

            await gameHubService.UpdateCurrentGame(gameId: round.GameId);

            return Result.Ok(round);
        }//sinon on en conclue que c'est la fin du jeu et on appelle la classe FinishGame, on envoie une erreur en cas de non réussite
        //puis on met à jour le statut du jeu avec UpdateCurrentGame et on renvoie un résultat avec en paramètre le round
    }
}
