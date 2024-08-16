using System.Collections;

namespace CassinoDemo.Poker;

public class PlayerCollection : IReadOnlyList<Player>
{
    protected List<Player> PlayerList { get; }

    public PlayerCollection(IEnumerable<Player> players)
    {
        PlayerList = new List<Player>(players);
    }

    public Player this[int index] => PlayerList[index];
    public int Count => PlayerList.Count;

    public IEnumerator<Player> GetEnumerator()
    {
        return PlayerList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /*
     * Query helpers
     */

    public int GetDealerIndex(int defaultValue)
    {
        return ComputeDealerIndex(PlayerList) ?? defaultValue;
    }

    public int GetSmallBlindIndex(int defaultValue)
    {
        return ComputeSmallBlindIndex(PlayerList) ?? defaultValue;
    }

    public int GetBigBlindIndex(int defaultValue)
    {
        return ComputeBigBlindIndex(PlayerList) ?? defaultValue;
    }

    public Player? FindPlayer(Guid id)
    {
        return PlayerList.FirstOrDefault(p => p.Id == id);
    }

    public Player GetPlayer(Guid id)
    {
        var player = FindPlayer(id);

        if (player is null)
        {
            throw new InvalidOperationException("Player not found");
        }

        return player;
    }

    public Player GetDealer()
    {
        return PlayerList[ComputeDealerIndex(PlayerList) ?? throw new InvalidOperationException("Dealer not set")];
    }

    public Player GetSmallBlind()
    {
        return PlayerList[ComputeSmallBlindIndex(PlayerList) ?? throw new InvalidOperationException("Small blind not set")];
    }

    public Player GetBigBlind()
    {
        return PlayerList[ComputeBigBlindIndex(PlayerList) ?? throw new InvalidOperationException("Big blind not set")];
    }

    public Player GetPlayerToTheLeftOf(Player player)
    {
        var index = PlayerList.IndexOf(player);

        if (index == -1)
        {
            throw new InvalidOperationException("Player not found");
        }

        var leftIndex = index - 1;

        if (leftIndex < 0)
        {
            leftIndex = PlayerList.Count - 1;
        }

        return PlayerList[leftIndex];
    }

    /*
     * Bet queries
     */

    public Bet? GetHighestBet()
    {
        return PlayerList
            .Select(p => p.Bet)
            .Where(b => b is not null)
            .OrderByDescending(b => b!.Amount)
            .FirstOrDefault();
    }

    public decimal GetHighestBetAmount()
    {
        return PlayerList.Max(p => p.Bet?.Amount ?? 0);
    }

    /*
     * Internal helpers
     */

    protected int? ComputeDealerIndex(List<Player> players)
    {
        var index = players
            .FindIndex(p => p.IsDealer);

        return index == -1 ? null : index;
    }

    protected int? ComputeSmallBlindIndex(List<Player> players)
    {
        var dealerIndex = ComputeDealerIndex(players);

        if (dealerIndex is null)
        {
            return null;
        }

        var index = (int)dealerIndex + 1;

        if (index >= PlayerList.Count)
        {
            index = 0;
        }

        return index;
    }

    protected int? ComputeBigBlindIndex(List<Player> players)
    {
        var smallBlindIndex = ComputeSmallBlindIndex(players);

        if (smallBlindIndex is null)
        {
            return null;
        }

        var index = (int)smallBlindIndex + 1;

        if (index >= PlayerList.Count)
        {
            index = 0;
        }

        return index;
    }

}