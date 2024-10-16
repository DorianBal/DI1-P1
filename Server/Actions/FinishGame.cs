
using FluentResults;

using FluentValidation;

using Microsoft.AspNetCore.SignalR;

using Server.Actions.Contracts;
using Server.Hubs;
using Server.Hubs.Contracts;
using Server.Models;
using Server.Persistence.Contracts;

namespace Server.Actions;

public sealed record FinishGameParams(int? GameId = null, Game? Game = null);

public class FinishGameValidator : AbstractValidator<FinishGameParams>
{
    public FinishGameValidator()
    {
        RuleFor(p => p.GameId).NotEmpty().When(p => p.Game is null);
        RuleFor(p => p.Game).NotEmpty().When(p => p.GameId is null);
    }
}


public class FinishGame(IGameHubService gameHubService, IGamesRepository gamesRepository) : IAction<FinishGameParams, Result<Game>>
{
    public async Task<Result<Game>> PerformAsync(FinishGameParams actionParams)
    {
        var game = actionParams.Game;
        if (game == null)
        {
            return Result.Fail("Le jeu n'a pas pu être trouvé.");
        }
        game.Status = GameStatus.Finished;
        await gamesRepository.SaveGame(game);
        await gameHubService.UpdateCurrentGame(gameId: game.Id!.Value);
        return Result.Ok(game);
    }
}