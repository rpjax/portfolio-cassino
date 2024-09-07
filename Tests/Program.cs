using Aidan.Core.Helpers;
using Application.Infra.Database.Poker;
using Application.Infra.Database.Services;
using Domain.Core.Services;
using Domain.Poker;
using Xunit;

namespace Tests;

public class Program
{
    private IRepositoryProvider RepositoryProvider { get; }
    private IPokerService PokerService { get; }
    private Guid JohnId { get; }
    private Guid AliceId { get; }
    private Guid BobId { get; }
    private Guid CharlieId { get; }
    private Guid DavidId { get; }

    public Program()
    {
        var dbFile = LocalStorage.GetFileInfo("cassino.db");
        var dbContext = new PokerDbContext(dbFile);
        var repositoryProvider = new SqliteRepositoryProvider(dbContext);
        var repository = new PokerGameRepository(repositoryProvider);

        RepositoryProvider = repositoryProvider;
        PokerService = new PokerService(repository);

        JohnId = Guid.NewGuid();
        AliceId = Guid.NewGuid();
        BobId = Guid.NewGuid();
        CharlieId = Guid.NewGuid(); 
        DavidId = Guid.NewGuid();
    }

    private async Task<PokerGame> CreateGameAsync(bool addPlayers = false)
    {
        var game = await PokerService.StartGameAsync(GetRules());
        
        if (addPlayers)
        {
            AddPlayers(game);
        }

        await RepositoryProvider.CommitAsync();

        return game;
    }

    private PokerRules GetRules()
    {
        return new PokerRules(
            minPlayers: 2,
            maxPlayers: 5,
            smallBlindValue: 5,
            bigBlindValue: 10,
            shufflingStrategy: new RandomShufflingStrategy(),
            allowRebuys: false);
    }

    private void AddPlayers(PokerGame game)
    {
        game.Join(seat: 0, id: JohnId, name: "John", bankroll: 1000);
        game.Join(seat: 1, id: AliceId, name: "Alice", bankroll: 1000);
        game.Join(seat: 2, id: BobId, name: "Bob", bankroll: 10);
        game.Join(seat: 3, id: CharlieId, name: "Charlie", bankroll: 1000);
        game.Join(seat: 4, id: DavidId, name: "David", bankroll: 1000);
    }


    private Player GetJohn(PokerGame game)
    {
        return game.GetPlayer(JohnId);
    }

    private Player GetAlice(PokerGame game)
    {
        return game.GetPlayer(AliceId);
    }

    private Player GetBob(PokerGame game)
    {
        return game.GetPlayer(BobId);
    }

    private Player GetCharlie(PokerGame game)
    {
        return game.GetPlayer(CharlieId);
    }

    private Player GetDavid(PokerGame game)
    {
        return game.GetPlayer(DavidId);
    }


    [Fact(DisplayName = "PokerGame Start Mechanics")]
    private async Task TestStartMechanics()
    {
        var pokerGame = await CreateGameAsync(addPlayers: true);

        var john = GetJohn(pokerGame);
        var alice = GetAlice(pokerGame);
        var bob = GetBob(pokerGame);
        var charlie = GetCharlie(pokerGame);
        var david = GetDavid(pokerGame);

        pokerGame.StartRound();

        /* Assert players are seated */
        Assert.True(pokerGame.IsPlayerSeated(JohnId));
        Assert.True(pokerGame.IsPlayerSeated(AliceId));
        Assert.True(pokerGame.IsPlayerSeated(BobId));
        Assert.True(pokerGame.IsPlayerSeated(CharlieId));
        Assert.True(pokerGame.IsPlayerSeated(DavidId));

        /* Assert players bankroll */
        Assert.Equal(1000, john.BankrollBalance);
        Assert.Equal(995, alice.BankrollBalance);
        Assert.Equal(0, bob.BankrollBalance);
        Assert.Equal(1000, charlie.BankrollBalance);
        Assert.Equal(1000, david.BankrollBalance);

        /* Assert players have a hand with 2 cards */
        Assert.Equal(2, john.Hand?.Count);
        Assert.Equal(2, alice.Hand?.Count);
        Assert.Equal(2, bob.Hand?.Count);
        Assert.Equal(2, charlie.Hand?.Count);
        Assert.Equal(2, david.Hand?.Count);

        /* Assert players are seated */
        Assert.True(john.IsActive);
        Assert.True(alice.IsActive);
        Assert.True(bob.IsActive);
        Assert.True(charlie.IsActive);
        Assert.True(david.IsActive);

        /* Assert players are not sleeping */
        Assert.True(john.IsNotSleeping);
        Assert.True(alice.IsNotSleeping);
        Assert.True(bob.IsNotSleeping);
        Assert.True(charlie.IsNotSleeping);
        Assert.True(david.IsNotSleeping);
    }

    [Fact(DisplayName = "PokerGame Betting Mechanics")]
    private async Task TestBettingMechanics()
    {
        var pokerGame = await CreateGameAsync(addPlayers: true);

        var john = GetJohn(pokerGame);
        var alice = GetAlice(pokerGame);
        var bob = GetBob(pokerGame);
        var charlie = GetCharlie(pokerGame);
        var david = GetDavid(pokerGame);

        var dealer = john;
        var smallBlind = alice;
        var bigBlind = bob;

        pokerGame.StartRound();

        pokerGame.Call(charlie.Id);
        pokerGame.Call(david.Id);
        pokerGame.Call(john.Id);
        pokerGame.Call(alice.Id);
        pokerGame.Check(bob.Id);

        pokerGame.Check(alice.Id);
        pokerGame.Check(bob.Id);
        pokerGame.Check(charlie.Id);
        pokerGame.Check(david.Id);
        pokerGame.Check(john.Id);

        pokerGame.Check(alice.Id);
        pokerGame.Check(bob.Id);
        pokerGame.Raise(charlie.Id, 10);
        pokerGame.Call(david.Id);
        pokerGame.Call(john.Id);
        pokerGame.Call(alice.Id);
        pokerGame.Fold(bob.Id);

        pokerGame.Check(alice.Id);
        pokerGame.Check(charlie.Id);
        pokerGame.Check(david.Id);
        pokerGame.Check(john.Id);

    }

    [Fact(DisplayName = "PokerGame Quit Mechanics")]
    private async Task TestQuitMechanics()
    {
        var pokerGame = await CreateGameAsync(addPlayers: true);

        var john = GetJohn(pokerGame);
        var alice = GetAlice(pokerGame);
        var bob = GetBob(pokerGame);
        var charlie = GetCharlie(pokerGame);
        var david = GetDavid(pokerGame);

        pokerGame.StartRound();

        pokerGame.Quit(john.Id);
        pokerGame.Quit(alice.Id);
        pokerGame.Quit(bob.Id);
        pokerGame.Quit(charlie.Id);
        pokerGame.Quit(david.Id);

        Assert.False(john.IsActive);
        Assert.False(alice.IsActive);
        Assert.False(bob.IsActive);
        Assert.False(charlie.IsActive);
        Assert.False(david.IsActive);
    }

}