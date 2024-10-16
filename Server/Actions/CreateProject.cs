
using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record CreateProjectParams(string ProjectName, int? GameId = null, Game? Game = null);

public class CreateProjectValidator : AbstractValidator<CreateProjectParams>
{
    public CreateProjectValidator()
    {
        RuleFor(p => p.ProjectName).NotEmpty();
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class CreateProject(
    IProjectsRepository projectsRepository,
    IGamesRepository gamesRepository,
    ISkillsRepository skillsRepository,
    IGameHubService gameHubService
) : IAction<CreateProjectParams, Result<Project>>
{
    public async Task<Result<Project>> PerformAsync(CreateProjectParams actionParams)
    {
        var rnd = new Random();

        var actionValidator = new CreateProjectValidator();
        var actionValidationResult = await actionValidator.ValidateAsync(actionParams);

        if (actionValidationResult.Errors.Count != 0)
        {
            return Result.Fail(actionValidationResult.Errors.Select(e => e.ErrorMessage));
        }

        var (projectName, gameId, game) = actionParams;

        game ??= await gamesRepository.GetById((int) gameId!);

        if (game is null)
        {
            Result.Fail($"Company with Id \"{gameId}\" not found.");
        }

        IEnumerable<int> revenu = [];

        revenu = revenu.Append(1500); // Salaire de base (hors skills)

        var randomSalary = revenu.ToList()[rnd.Next(revenu.Count() - 1)];

        var nombreTour = rnd.Next(1, 5);

        var project = new Project(projectName, (int) game!.Id!, randomSalary, nombreTour);

        var randomSkills = await skillsRepository.GetRandomSkills(5);

        var totalSkillsLevel = 0;

        foreach (var randomSkill in randomSkills)
        {
            var leveledSkill = rnd.Next(3); // Mettre le niveau de départ à 3 au maximum
            project.Skills.Add(new LeveledSkill(randomSkill.Name, leveledSkill));
            totalSkillsLevel += leveledSkill;
        }

        project.Revenu += nombreTour * totalSkillsLevel * 100; // Nouveau salaire, en fonction des niveaux des skills

        await projectsRepository.SaveProject(project);

        await gameHubService.UpdateCurrentGame(gameId: gameId);

        return Result.Ok(project);
    }
}
