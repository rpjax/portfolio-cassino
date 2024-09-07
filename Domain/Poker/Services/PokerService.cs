using Aidan.Core.Linq;
using Aidan.Core.Linq.Extensions;
using Domain.Core.Services;

namespace Domain.Poker;

public class PokerService : IPokerService
{   
    private IRepositoryService<PokerGame> Repository { get; }

    public PokerService(IRepositoryService<PokerGame> repository)
    {
        Repository = repository;
    }

    /* Query */

    public async Task<PokerGame> GetAsync(Guid gameId)
    {
        var game = await Repository.AsQueryable()
            .Where(x => x.Id == gameId)
            .FirstOrDefaultAsync();

        if (game == null)
        {
            throw new InvalidOperationException("Game not found");
        }

        return game;
    }

    public IAsyncQueryable<PokerGame> GetGames()
    {
        throw new NotImplementedException();
    }

    /* Start Game */

    public async Task<PokerGame> StartGameAsync(
        PokerRules rules)
    {
        var game = PokerGame.Create()
            .WithTable(CreateTable(seats: rules.MaxPlayers))
            .WithDealer(CreateDealer(rules))
            .Build();

        await Repository.CreateAsync(game);
        return game;
    }

    private Table CreateTable(int seats)
    {
        return Table.Create()
            .WithSeats(seats)
            .WithDeck(CreateDeck())
            .Build();
    }

    private CardDeck CreateDeck()
    {
        return CardDeckBuilder.Standard52Cards();
    }

    private Dealer CreateDealer(PokerRules rules)
    {
        return Dealer.Create()
            .WithRules(rules)
            .Build();   
    }

    /* Game Mechanics */

    public async Task JoinGameAsync(
        Guid gameId, 
        Guid playerId, 
        int seat, 
        string playerName, 
        decimal bankroll)
    {
        var game = await GetAsync(gameId);
        game.Join(playerId, seat, playerName, bankroll);
        await Repository.UpdateAsync(game);
    }

    public async Task QuitGameAsync(
        Guid gameId, 
        Guid playerId)
    {
        var game = await GetAsync(gameId);
        game.Quit(playerId);
        await Repository.UpdateAsync(game);
    }

    public Task RebuyAsync(
        Guid gameId, 
        Guid playerId, 
        decimal amount)
    {
        throw new NotImplementedException();
    }

    public Task CheckAsync(
        Guid gameId, 
        Guid playerId)
    {
        throw new NotImplementedException();
    }

    public Task BetAsync(
        Guid gameId, 
        Guid playerId, 
        decimal amount)
    {
        throw new NotImplementedException();
    }

    public Task FoldAsync(
        Guid gameId, 
        Guid playerId)
    {
        throw new NotImplementedException();
    }

    public Task CallAsync(
        Guid gameId, 
        Guid playerId)
    {
        throw new NotImplementedException();
    }

    public Task RaiseAsync(
        Guid gameId, 
        Guid playerId, 
        decimal amount)
    {
        throw new NotImplementedException();
    }
}
