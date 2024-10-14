
using System.Numerics;
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
    IConsultantsRepository consultantsRepository,
    ICompaniesRepository companiesRepository
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

        if (action is SendEmployeeForTrainingRoundAction sendemployee)
        {
            var employee = await employeesRepository.GetEmployeetById(sendemployee.Payload.EmployeeId);

            //on vérifie si l'employée n'est pas déjà en formation
            if (employee.enformation == false)
            {
                Console.WriteLine("\n\n\n\n"+employee.dureeformation+"\n\n\n\n");
                //on met à true la variable en formation afin de savoir qu'il l'est
                employee.enformation = true;

                //on met son nombre de tour à 1 pour qu'au prochain tour cela tombe à 0 avec la fonction dans finishround et que sa formation se terminer automatiquement grâce à la fonction après applyroundaction dans finishround
                employee.dureeformation = 1;

                Console.WriteLine("\n\n\n\nNom : "+employee.Name+" true : "+employee.enformation+" la durée de sa formation devrais être à 1 : "+employee.dureeformation+"\n\n\n\n");
                //on rajoute ensuite son nouveau niveau de skill en vérifiant quel skill à été choisis
                foreach (var skill in employee.Skills)
                {
                    if (skill.Name == sendemployee.Payload.nameofskillupgrade)
                    {
                        skill.Level += 1;
                    }
                    else
                    {
                        Console.WriteLine("\n\nle skill " + skill.Name + " n'est pas égale au skill " + sendemployee.Payload.nameofskillupgrade + ".\n\n");
                    }
                }
            }
            else
            {
                Console.WriteLine("l'employée est en formation, il ne peut pas faire une seconde formation avant d'avoir terminer celle-là");
            }
            //l'employee sera en formation pendant un tour donc on ne pourras pas l'envoyer faire un projet ni faire une autre formation
            //ni le virer lors du prochain tour, lors du début du prochain tour on remettra plus haut dans le code tout les bool enformation à false

            Console.WriteLine("TRAINING");
        }
        else if (action is ParticipateInCallForTendersRoundAction)
            Console.WriteLine("TENDERS");

        else if (action is RecruitAConsultantRoundAction recruit)
        {
            Console.WriteLine("RECRUIT");

            int nonNullableInt = action.PlayerId!.Value -2; // Le moins 2 car y'a un bug dans la bdd
            var consultant = await consultantsRepository.GetConsultantById(recruit.Payload.ConsultantId);
            await consultantsRepository.DeleteConsultantById(consultant.Id);
            await employeesRepository.SaveEmployeeFromConsultant(consultant!, nonNullableInt);
        }

        else if (action is FireAnEmployeeRoundAction Fire)
        {
            Console.WriteLine("FIRE EMPLOYEE");
            var employee = await employeesRepository.GetEmployeetById(Fire.Payload.EmployeeId);

            if(employee.enformation==false)
            {
                await employeesRepository.DeleteEmployeeById(employee.Id);
            }
            else
            {
                Console.WriteLine("l'employée est en formation, il ne peut pas être virée");
            }
        }
        else if (action is PassMyTurnRoundAction)
            Console.WriteLine("PASS TURN");
        else
        {
            Console.WriteLine("AUTRE"); Console.WriteLine(action.ToString());
        }

        await gameHubService.UpdateCurrentGame(gameId: gameId);

        return Result.Ok();
    }
}
