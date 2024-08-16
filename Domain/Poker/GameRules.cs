namespace CassinoDemo.Poker;

public class GameRules
{
    public int MinPlayers { get; }
    public int MaxPlayers { get; }
    public decimal SmallBlindValue { get; }
    public decimal BigBlindValue { get; }
    public IShufflingStrategy ShufflingStrategy { get; }

    public GameRules(
        int minPlayers, 
        int maxPlayers,
        decimal smallBlindValue,
        decimal bigBlindValue,
        IShufflingStrategy shufflingStrategy)
    {
        MinPlayers = minPlayers;
        MaxPlayers = maxPlayers;
        SmallBlindValue = smallBlindValue;
        BigBlindValue = bigBlindValue;
        ShufflingStrategy = shufflingStrategy;
    }
}
