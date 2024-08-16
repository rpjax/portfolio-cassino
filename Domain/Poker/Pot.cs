using System.Collections;

namespace CassinoDemo.Poker;

public class Pot : IReadOnlyList<Bet>
{
    private List<Bet> Bets { get; }

    public Pot(IEnumerable<Bet> bets)
    {
        Bets = new List<Bet>(bets);
    }

    public Bet this[int index] => Bets[index];

    public int Count => Bets.Count;

    public IEnumerator<Bet> GetEnumerator()
    {
        return Bets.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void AddBet(Bet bet)
    {
        Bets.Add(bet);
    }

    public void Clear()
    {
        Bets.Clear();
    }

    public decimal GetTotal()
    {
        return Bets.Sum(b => b.Amount);
    }
}