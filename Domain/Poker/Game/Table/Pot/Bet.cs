namespace Domain.Poker;

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

    public static BetBuilder Create()
    {
        return new BetBuilder();
    }

}

public class BetBuilder
{
    private Guid _playerId;
    private decimal _amount;

    public BetBuilder WithPlayerId(Guid playerId)
    {
        _playerId = playerId;
        return this;
    }

    public BetBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public Bet Build()
    {
        return new Bet(_playerId, _amount);
    }
}