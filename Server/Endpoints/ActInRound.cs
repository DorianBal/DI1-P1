using System.Text.Json;

using FluentResults;

using Microsoft.AspNetCore.Mvc;

using Server.Actions;
using Server.Actions.Contracts;
using Server.Endpoints.Contracts;
using Server.Models;
using Server.Persistence;

namespace Server.Endpoints;

public class ActInRound : IEndpoint
{
    public sealed record ActInRoundBody(string ActionType, string ActionPayload, int PlayerId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("rounds/{roundId}/act", Handler).WithTags("Rounds");
    }

    public static async Task<IResult> Handler(
        int roundId,
        [FromBody] ActInRoundBody body,
        WssDbContext context,
        IAction<ActInRoundParams, Result<Round>> actInRoundAction
    )
    {
        var actionTypeParsed = Enum.TryParse<RoundActionType>(body.ActionType, out var parsedActionType);

        if (!actionTypeParsed)
        {
            return Results.BadRequest(new { Errors = new[] { "Invalid action type" } });
        }

        var actionParams = new ActInRoundParams(
            body.ActionType,
            body.ActionPayload!,
            roundId,
            PlayerId: body.PlayerId
        );

        using var transaction = context.Database.BeginTransaction();

        var actionResult = await actInRoundAction.PerformAsync(actionParams);

        if (actionResult.IsFailed)
        {
            await transaction.RollbackAsync();
            return Results.BadRequest(new { Errors = actionResult.Errors.Select(e => e.Message) });
        }

        await transaction.CommitAsync();
        return Results.Ok();
    }
}