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

public sealed record RemoveProjectParams(Project project, int? GameId = null, Game? Game = null);

public class RemoveProjectValidator : AbstractValidator<RemoveProjectParams>
{
    public RemoveProjectValidator()
    {
        RuleFor(p => p.project).NotEmpty();
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class RemoveProject(
    IProjectsRepository consultantsRepository
) : IAction<RemoveProjectParams, Result<Project>>
{
    public async Task<Result<Project>> PerformAsync(RemoveProjectParams actionParams)
    {
        var project = actionParams.project;

        try
        {
            await consultantsRepository.DeleteProjectById(project.Id);

            return Result.Ok(project);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to remove project: {ex.Message}");
        }

    }
}