namespace Domain.Poker;

public class PokerRules
{
    public int MinPlayers { get; }
    public int MaxPlayers { get; }
    public decimal SmallBlindAmount { get; }
    public decimal BigBlindAmount { get; }
    public IShufflingStrategy ShufflingStrategy { get; }
    public bool AllowRebuys { get; }

    public PokerRules(
        int minPlayers, 
        int maxPlayers,
        decimal smallBlindValue,
        decimal bigBlindValue,
        IShufflingStrategy shufflingStrategy,
        bool allowRebuys)
    {
        MinPlayers = minPlayers;
        MaxPlayers = maxPlayers;
        SmallBlindAmount = smallBlindValue;
        BigBlindAmount = bigBlindValue;
        ShufflingStrategy = shufflingStrategy;
        AllowRebuys = allowRebuys;

        if (MinPlayers < 2)
        {
            throw new InvalidOperationException("Minimum players must be at least 2.");
        }
        if (MaxPlayers < MinPlayers)
        {
            throw new InvalidOperationException("Maximum players must be greater than or equal to minimum players.");
        }

        if (SmallBlindAmount <= 0)
        {
            throw new InvalidOperationException("Small blind value must be greater than 0.");
        }
        if (BigBlindAmount <= 0)
        {
            throw new InvalidOperationException("Big blind value must be greater than 0.");
        }
        if (SmallBlindAmount >= BigBlindAmount)
        {
            throw new InvalidOperationException("Small blind value must be less than big blind value.");
        }
    }
}
