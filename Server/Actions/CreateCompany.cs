using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record CreateCompanyParams(string CompanyName, int? PlayerId = null, Player? Player = null);

public class CreateCompanyValidator : AbstractValidator<CreateCompanyParams>
{
    public CreateCompanyValidator()
    {
        RuleFor(p => p.CompanyName).NotEmpty();
        RuleFor(p => p.PlayerId).NotEmpty().When(p => p.Player is null);
        RuleFor(p => p.Player).NotEmpty().When(p => p.PlayerId is null);
    }
}

public class CreateCompany(
  ICompaniesRepository companiesRepository,
  IPlayersRepository playersRepository,
  IAction<CreateEmployeeParams, Result<Employee>> createEmployeeAction,
  IGameHubService gameHubService
) : IAction<CreateCompanyParams, Result<Company>>
{
    public async Task<Result<Company>> PerformAsync(CreateCompanyParams actionParams)
    {
        var actionValidator = new CreateCompanyValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (companyName, playerId, player) = actionParams;

        player ??= await playersRepository.GetById(playerId!.Value);

        if (player is null)
        {
            Result.Fail($"Player with Id \"{playerId}\" not found.");
        }

        if (player!.CompanyId is not null)
        {
            return Result.Fail("Player already has a company.");
        }

        var isCompanyNameAvailable = await companiesRepository.IsCompanyNameAvailable(companyName, player.GameId);

        if (!isCompanyNameAvailable)
        {
            return Result.Fail("'Company Name' is already in use.");
        }

        var company = new Company(companyName, player.Id!.Value);

        await companiesRepository.SaveCompany(company);

        foreach (var index in Enumerable.Range(1, 3))
        {
            // Génère un nombre de consultants aléatoire en fonction du nombre de joueurs dans la partie
            List<String> nameList = ["Mr.Dupont", "Mme.Lefevre", "Mr.Durand", "Mme.Lemoine", "Mr.Girard", "Mr.Bernard", "Mme.Fournier", "Mr.Rousseau", "Mme.Morin", "Mr.Moreau", "Mme.Perrier", "Mr.Blanc", "Mr.Philippe", "Mme.Gauthier", "Mr.Renaud", "Mme.Martineau", "Mr.Leclerc", "Mme.Laroche", "Mr.Bertin", "Mme.Fontaine", "Mr.Dumas", "Mme.Carrier"];
            int randomName = new Random().Next(0, nameList.Count);

            var createEmployeeParams = new CreateEmployeeParams(nameList[randomName], Company: company);
            var createEmployeeResult = await createEmployeeAction.PerformAsync(createEmployeeParams);

            if (createEmployeeResult.IsFailed)
            {
                return Result.Fail(createEmployeeResult.Errors);
            }
        }

        await gameHubService.UpdateCurrentGame(gameId: player.GameId);

        return Result.Ok(company);
    }
}
