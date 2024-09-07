using Aidan.Core.Linq;

namespace Domain.Poker;

public interface IPokerService
{
    /* Query */

    IAsyncQueryable<PokerGame> GetGames();

    /* Game Lifecycle */

    Task<PokerGame> StartGameAsync(PokerRules rules);

    /* Game Mechanics */

    Task JoinGameAsync(Guid gameId, Guid playerId, int seat, string playerName, decimal bankroll);

    Task QuitGameAsync(Guid gameId, Guid playerId);

    Task RebuyAsync(Guid gameId, Guid playerId, decimal amount);

    Task CheckAsync(Guid gameId, Guid playerId);

    Task BetAsync(Guid gameId, Guid playerId, decimal amount);

    Task FoldAsync(Guid gameId, Guid playerId);

    Task CallAsync(Guid gameId, Guid playerId);

    Task RaiseAsync(Guid gameId, Guid playerId, decimal amount);
}
