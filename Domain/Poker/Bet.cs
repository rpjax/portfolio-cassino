namespace CassinoDemo.Poker;

public class Bet
{
    public Guid PlayerId { get; }
    public decimal Amount { get; }

    public Bet(Guid playerId, decimal value)
    {
        PlayerId = playerId;
        Amount = value;
    }

    public override string ToString()
    {
        return $"${Amount}";
    }

}
