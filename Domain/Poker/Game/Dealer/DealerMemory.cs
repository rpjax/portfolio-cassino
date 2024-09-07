namespace Domain.Poker;

public class DealerMemory : IValueObject
{
    /* new API */
    private List<Guid> TurnMemory { get; }

    public DealerMemory(IEnumerable<Guid> turns)
    {
        TurnMemory = new List<Guid>(turns);
    }

    public static DealerMemory CreateEmpty()
    {
        return new DealerMemory(
            turns: Array.Empty<Guid>()
        );
    }

    /* Queries */

    public bool HasPlayed(Player player)
    {
        return TurnMemory.Contains(player.Id);
    }

    public bool HasNotPlayed(Player player)
    {
        return !HasPlayed(player);
    }
        
    /*
     * Commands
     */

    public void StartRound()
    {
        TurnMemory.Clear();
    }

    public void SaveTurn(Player player)
    {
        TurnMemory.Add(player.Id);
    }

}
