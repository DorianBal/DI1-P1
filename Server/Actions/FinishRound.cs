using System.Text.Json;
using System.Text.Json.Nodes;

using FluentResults;

using FluentValidation;

using Server.Actions.Contracts;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

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

public class FinishRound(
    IRoundsRepository roundsRepository,
    IAction<ApplyRoundActionParams, Result> applyRoundActionAction,
    IAction<StartRoundParams, Result<Round>> startRoundAction,
    IAction<FinishGameParams, Result<Game>> finishGameAction,
    IGameHubService gameHubService,
    ICompaniesRepository companiesRepository,
    IEmployeesRepository employeesRepository
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

        var (roundId, round) = actionParams;

        round ??= await roundsRepository.GetById(roundId!.Value);

        if (round is null)
        {
            return Result.Fail($"Round with Id \"{roundId}\" not found.");
        }

        var rnd = new Random();

        var newConsultantShouldBeGenerated = rnd.Next(2) == 1;

        if (newConsultantShouldBeGenerated)
        {
            var payload = new { round.GameId };
            var jsonPayload = JsonSerializer.Serialize<dynamic>(payload);

            var action = new RoundAction
            {
                PlayerId = 0,
                ActionType = "GenerateNewConsultant",
                Payload = jsonPayload
            };

            round.Actions.Add(action);

            await roundsRepository.SaveRound(round);
        }

        //si on créer de nouveau consultant on créer une action de tour pour le type GenerateNewConsultant
        //on ajoute cette action au round puis on sauvegarde cela dans le repos

        //à chaque nouveau tour on diminue de 1 la durée de leur entrainement
        await employeesRepository.dureetrainingreduceeachturn();


        foreach (var action in round.Actions)
        {
            var applyRoundActionParams = new ApplyRoundActionParams(RoundAction: action, Game: round.Game);
            var applyRoundActionResult = await applyRoundActionAction.PerformAsync(applyRoundActionParams);

            if (applyRoundActionResult.IsFailed)
            {
                return Result.Fail(applyRoundActionResult.Errors);
            }
        }

        await employeesRepository.EndOfTraining();

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

            await gameHubService.UpdateCurrentGame(gameId: round.GameId);

            return Result.Ok(newRound);
        }
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
        }
    }
}