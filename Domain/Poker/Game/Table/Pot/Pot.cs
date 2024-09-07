namespace Domain.Poker;

public class Pot 
{
    private Dictionary<Guid, decimal> Bets { get; }

    public Pot(Dictionary<Guid, decimal> bets)
    {
        Bets = bets;
    }

    public bool IsEmpty => TotalAmount == 0;
    public bool IsNotEmpty => !IsEmpty;
    public decimal TotalAmount => GetTotalAmount();
    public decimal MaxBetAmount => GetMaxBetAmount();

    public decimal GetTotalAmount()
    {
        if (Bets.Count == 0)
        {
            return 0m;
        }

        return Bets.Sum(x => x.Value);
    }

    public decimal GetMaxBetAmount()
    {
        if(Bets.Count == 0)
        {
            return 0m;
        }

        return Bets.Values.Max();
    }

    public decimal GetPlayerBetAmount(Player player)
    {
        if (Bets.ContainsKey(player.Id))
        {
            return Bets[player.Id];
        }

        return 0m;
    }

    public void PlaceBet(Player player, decimal amount)
    {
        var currentBet = GetPlayerBetAmount(player);
        var newBet = currentBet + amount;
        Bets[player.Id] = newBet;
    }

    public decimal CollectBets()
    {
        var totalAmount = TotalAmount;
        Bets.Clear();
        return totalAmount;
    }

}