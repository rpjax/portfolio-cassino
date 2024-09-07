using Aidan.Core.Extensions;
using Aidan.Core.Patterns;

namespace Domain.Poker;

public enum RoundPhase
{
    PreFlop,
    Flop,
    Turn,
    River
}

public class RoundPlayers : PlayerCollection
{
    public RoundPlayers(IEnumerable<Player> players) : base(players)
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

    public static RoundPlayersBuilder Create()
    {
        return new RoundPlayersBuilder();
    }

    /*
     * private helpers
     */

}

public class RoundPlayersBuilder : IBuilder<RoundPlayers>
{
    private List<Player> Players { get; } = new();

    public RoundPlayers Build()
    {
        return new RoundPlayers(players: Players);
    }

    public RoundPlayersBuilder AddPlayer(Player player)
    {
        Players.Add(player);
        return this;
    }

    public RoundPlayersBuilder WithPlayers(IEnumerable<Player> players)
    {
        Players.AddRange(players);
        return this;
    }
}

