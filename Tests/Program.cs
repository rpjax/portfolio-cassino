using CassinoDemo.Poker;
using Xunit;

namespace CassinoDemo.Tests;

public class Program
{
    [Fact()]
    public static void StartPokerGame()
    {
        var service = new PokerService();

        var pokerGame = service.StartGame();

        pokerGame.AddPlayer(id: Guid.NewGuid(), name: "John", bankroll: 1000);
        pokerGame.AddPlayer(id: Guid.NewGuid(), name: "Alice", bankroll: 1000);
        pokerGame.AddPlayer(id: Guid.NewGuid(), name: "Bob", bankroll: 4);
        pokerGame.StartRound();

        var turnPlayer = pokerGame.Players.First(x => x.HasTurnButton);

        pokerGame.Call(turnPlayer.Id);

        Console.WriteLine();
    }
}