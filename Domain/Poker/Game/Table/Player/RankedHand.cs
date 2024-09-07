using Aidan.Core.Extensions;

namespace Domain.Poker;

public class RankedHand
{
    public IList<Card> Cards { get; }
    public HandRankType Rank { get; }
    public float Strength { get; }

    public RankedHand(IEnumerable<Card> cards, HandRankType rank, float strength)
    {
        if(cards.IsEmpty())
        {
            throw new ArgumentException("Cards cannot be empty.");
        }

        Cards = new List<Card>(cards);
        Rank = rank;
        Strength = strength;
    }
}