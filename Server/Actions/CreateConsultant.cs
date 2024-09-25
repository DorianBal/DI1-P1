
using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record CreateConsultantParams(string ConsultantName, int? GameId = null, Game? Game = null);

public class CreateConsultantValidator : AbstractValidator<CreateConsultantParams>
{
    public CreateConsultantValidator()
    {
        RuleFor(p => p.ConsultantName).NotEmpty();
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class CreateConsultant(
    IConsultantsRepository consultantsRepository,
    IGamesRepository gamesRepository,
    ISkillsRepository skillsRepository,
    IGameHubService gameHubService
) : IAction<CreateConsultantParams, Result<Consultant>>
{
    public async Task<Result<Consultant>> PerformAsync(CreateConsultantParams actionParams)
    {
        var rnd = new Random();

        var actionValidator = new CreateConsultantValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (consultantName, gameId, game) = actionParams;

        game ??= await gamesRepository.GetById((int) gameId!);

        if (game is null)
        {
            return Result.Fail($"Game with Id \"{gameId}\" not found.");
        }

        IEnumerable<int> salaryRequirements = [];

        salaryRequirements = salaryRequirements.Append(1500); // Salaire de base (hors skills)

        var randomSalaryRequirement = salaryRequirements.ToList()[rnd.Next(salaryRequirements.Count() - 1)];

        var consultant = new Consultant(consultantName, randomSalaryRequirement, (int) game!.Id!);

        var randomSkills = await skillsRepository.GetRandomSkills(5);

        var totalSkillsLevel = 0;

        foreach (var randomSkill in randomSkills)
        {
            var leveledSkill = rnd.Next(3); // Mettre le niveau de départ à 3 au maximum
            consultant.Skills.Add(new LeveledSkill(randomSkill.Name, leveledSkill));
            totalSkillsLevel += leveledSkill;
        }

        consultant.SalaryRequirement += totalSkillsLevel * 100; // Nouveau salaire, en fonction des niveaux des skills

        await consultantsRepository.SaveConsultant(consultant);

        await gameHubService.UpdateCurrentGame(gameId: gameId);

        return Result.Ok(consultant);
    }
}
