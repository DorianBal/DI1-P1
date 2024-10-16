
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
using System.Text.Json;
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
    IProjectsRepository projectsRepository,
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

        if (action.ActionType == "SendEmployeeForTraining")
        {
            Console.WriteLine("TRAINING");

            if (!string.IsNullOrEmpty(action.Payload))
            {
                var payloadDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(action.Payload);

                if (payloadDictionary != null && payloadDictionary.ContainsKey("EmployeeId"))
                {
                    int EmployeeId = Convert.ToInt32(payloadDictionary["EmployeeId"].ToString());
                    int numberofleveltoimproveskill = Convert.ToInt32(payloadDictionary["numberofleveltoimproveskill"].ToString());
                    string nameofskillupgrade = payloadDictionary["nameofskillupgrade"].ToString();
                    var employee = await employeesRepository.GetEmployeetById(EmployeeId);

                    //on vérifie si l'employée n'est pas déjà en formation
                    if (employee.enformation == false)
                    {
                        Console.WriteLine("\n\n\n\n" + employee.dureeformation + "\n\n\n\n");
                        //on met à true la variable en formation afin de savoir qu'il l'est
                        employee.enformation = true;

                        //on met son nombre de tour à 1 pour qu'au prochain tour cela tombe à 0 avec la fonction dans finishround et que sa formation se terminer automatiquement grâce à la fonction après applyroundaction dans finishround
                        employee.dureeformation = numberofleveltoimproveskill;

                        Console.WriteLine("\n\n\n\nNom : " + employee.Name + " true : " + employee.enformation + " la durée de sa formation devrais être à 1 : " + employee.dureeformation + "\n\n\n\n");
                        //on rajoute ensuite son nouveau niveau de skill en vérifiant quel skill à été choisis
                        foreach (var skill in employee.Skills)
                        {
                            if (skill.Name == nameofskillupgrade)
                            {
                                skill.Level += numberofleveltoimproveskill;
                            }
                            else
                            {
                                Console.WriteLine("\n\nle skill " + skill.Name + " n'est pas égale au skill " + numberofleveltoimproveskill + ".\n\n");
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("l'employée est en formation, il ne peut pas faire une seconde formation avant d'avoir terminer celle-là");
            }
            //l'employee sera en formation pendant un tour donc on ne pourras pas l'envoyer faire un projet ni faire une autre formation
            //ni le virer lors du prochain tour, lors du début du prochain tour on remettra plus haut dans le code tout les bool enformation à false
        }
        if (action.ActionType == "ParticipateInCallForTenders")
            Console.WriteLine("TENDERS");

        else if (action.ActionType == "RecruitAConsultant")
        {
            Console.WriteLine("RECRUIT");

            var payloadDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(action.Payload);

            if (payloadDictionary != null && payloadDictionary.ContainsKey("ConsultantId"))
            {
                int consultantId = Convert.ToInt32(payloadDictionary["ConsultantId"].ToString());

                int nonNullableInt = action.PlayerId!.Value - 2; // Le moins 2 car y'a un bug dans la bdd
                var consultant = await consultantsRepository.GetConsultantById(consultantId);
                await consultantsRepository.DeleteConsultantById(consultantId);
                await employeesRepository.SaveEmployeeFromConsultant(consultant!, nonNullableInt);
            }
        }

        else if (action.ActionType == "FireAnEmployee")
        {
            Console.WriteLine("FIRE EMPLOYEE");

            if (!string.IsNullOrEmpty(action.Payload))
            {
                var payloadDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(action.Payload);

                if (payloadDictionary != null && payloadDictionary.ContainsKey("EmployeeId"))
                {
                    int EmployeeId = Convert.ToInt32(payloadDictionary["EmployeeId"].ToString());
                    var employee = await employeesRepository.GetEmployeetById(EmployeeId);

                    // if (employee.enformation == false)
                    // {
                    await employeesRepository.DeleteEmployeeById(EmployeeId);
                    // }
                }
            }
        }
        else if (action.ActionType == "PassMyTurn")
            Console.WriteLine("PASS TURN");
        else
        {
            Console.WriteLine("AUTRE"); Console.WriteLine(action.ToString());
        }

        await gameHubService.UpdateCurrentGame(gameId: gameId);

        return Result.Ok();
    }
}
