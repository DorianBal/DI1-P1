
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;
namespace Server.Actions;

public sealed record ApplyRoundActionParams(
    RoundAction RoundAction,
    int? GameId = null,
    Game? Game = null
);

public class ApplyRoundActionValidator : AbstractValidator<ApplyRoundActionParams>
{
    public ApplyRoundActionValidator()
    {
        RuleFor(p => p.RoundAction).NotEmpty();
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class ApplyRoundAction(
    IGamesRepository gamesRepository,
    IGameHubService gameHubService,
    IEmployeesRepository employeesRepository,
    IConsultantsRepository consultantsRepository
) : IAction<ApplyRoundActionParams, Result>
{
    public async Task<Result> PerformAsync(ApplyRoundActionParams actionParams)
    {
        var actionValidator = new ApplyRoundActionValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (action, gameId, game) = actionParams;

        game ??= await gamesRepository.GetById(gameId!.Value);

        if (game is null)
        {
            return Result.Fail($"Game with Id \"{gameId}\" not found.");
        }

        Console.WriteLine("\n\n\n\n");

        if (action is SendEmployeeForTrainingRoundAction)
        {


            Console.WriteLine("TRAINING");
        }
        else if (action is ParticipateInCallForTendersRoundAction)
            Console.WriteLine("TENDERS");

        else if (action is RecruitAConsultantRoundAction recruit)
        {
            Console.WriteLine("RECRUIT");
            Console.WriteLine(action.PlayerId!.Value);

            int nonNullableInt = action.PlayerId!.Value -2; // Le moins 1 c'est parcequ'il y a eu un conflit dans la bdd, a corrigé
            Console.WriteLine(nonNullableInt);
            Console.WriteLine(recruit);
            Console.WriteLine(recruit.Payload.ConsultantId);

            var consultant = await consultantsRepository.GetConsultantById(recruit.Payload.ConsultantId);
            Console.WriteLine("\n\n idConsultant : " + consultant!.Id + "\n\n");


            Console.WriteLine("\n\n ici");
            await consultantsRepository.DeleteConsultantById(consultant.Id);
            // Console.WriteLine("\n\n delete consultant : " + consultant!.Id + "\n\n");
            Console.WriteLine("\n\n la");
            await employeesRepository.SaveEmployeeFromConsultant(consultant!, nonNullableInt);
            // Console.WriteLine("\n\n save consultant : " + consultant!.Id + "\n\n");
            Console.WriteLine("\n\n da");
            // Console.WriteLine("\n\nRECRUIT" + action.PlayerId + " / " + consultant!.Name);
            await gameHubService.UpdateCurrentGame(gameId: gameId);
            Console.WriteLine("\n\n pu");
        }

        else if (action is FireAnEmployeeRoundAction)
            Console.WriteLine("FIRE EMPLOYEE");
        else if (action is PassMyTurnRoundAction)
            Console.WriteLine("PASS TURN");
        else
            Console.WriteLine("AUTRE");


        // @todo: Implement the logic for applying the round action

        await gameHubService.UpdateCurrentGame(gameId: gameId);

        return Result.Ok();
    }

    /*
    public void ImplementeRoundAction(FireAnEmployeeRoundAction action, Game game)
     //on doit modifier le modèle de l'action sinon on ne peut pas récupérer les données comme par exemple action.payload.employeeid
     //donc il faut détecter le type d'action avant ou avec un switch ou une autre solution ?
    {
        if (action is RecruitAConsultantRoundAction)
        {
            //on écris ici le code associé
        }
        else if (action is FireAnEmployeeRoundAction)//donc cela ne sert plus à rien
        {
            action.Payload.EmployeeId //grâce à ça on doit récupérer l'employé qui va être virée afin de faie fonctionner le code ci-dessous
            Consultant unconsultant = new Consultant(employee.Name, employee.Salary, employee.GameId);
            consultantsRepository.SaveConsultant(unconsultant);

            employeesRepository.FireEmployee(employee);
            //il faudrait pouvoir appeler le repository pour mettre à jour cela, supprimer l'employée et le rajouter en tant que consultant
        }
        else
        {
            //on balance une erreur
        }
    }*/
}
