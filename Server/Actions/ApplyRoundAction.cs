
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

            await gameHubService.UpdateCurrentGame(gameId: gameId);

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

            await gameHubService.UpdateCurrentGame(gameId: gameId);
        }

        else if (action is FireAnEmployeeRoundAction Fire)
        {
            Console.WriteLine("FIRE EMPLOYEE");
            var employee = await employeesRepository.GetEmployeetById(Fire.Payload.EmployeeId);

            if(employee.enformation==false)
            {
                await employeesRepository.DeleteEmployeeById(employee.Id);
                await gameHubService.UpdateCurrentGame(gameId: gameId);
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
