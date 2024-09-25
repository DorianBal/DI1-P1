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

public sealed record RemoveConsultantParams(Consultant consultant, int? GameId = null, Game? Game = null);

public class RemoveConsultantValidator : AbstractValidator<RemoveConsultantParams>
{
    public RemoveConsultantValidator()
    {
        RuleFor(p => p.consultant).NotEmpty();
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}

public class RemoveConsultant(
    IConsultantsRepository consultantsRepository
) : IAction<RemoveConsultantParams, Result<Consultant>>
{
    public async Task<Result<Consultant>> PerformAsync(RemoveConsultantParams actionParams)
    {
        var consultant = actionParams.consultant;

        try
        {
            await consultantsRepository.DeleteConsultant(consultant);

            return Result.Ok(consultant);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to remove consultant: {ex.Message}");
        }

    }
}