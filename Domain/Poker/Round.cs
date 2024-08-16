using Aidan.Core.Extensions;
using Aidan.Core.Patterns;

namespace CassinoDemo.Poker;

public enum RoundPhase
{
    NotStarted,
    PreFlop,
    Flop,
    Turn,
    River,
    Showdown
}

public class Round : PlayerCollection
{
    public Round(IEnumerable<Player> players) : base(players)
    {
        if (players.IsEmpty())
        {
            throw new ArgumentException("Players list is empty");
        }
    }

    public IReadOnlyList<Player> Players => PlayerList;

    /*
     * Factories
     */

    public static RoundBuilder Create()
    {
        return new RoundBuilder();
    }

    /*
     * private helpers
     */

}

public class RoundBuilder : IBuilder<Round>
{
    private List<Player> Players { get; } = new();

    public Round Build()
    {
        return new Round(players: Players);
    }

    public RoundBuilder AddPlayer(Player player)
    {
        Players.Add(player);
        return this;
    }

    public RoundBuilder WithPlayers(IEnumerable<Player> players)
    {
        Players.AddRange(players);
        return this;
    }
}