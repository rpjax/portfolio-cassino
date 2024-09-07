namespace Domain.Poker;

public interface IRankingStrategy
{
    HandRankType ComputeHandRank(IReadOnlyList<Card> cards);
    int ComputeHandStrength(IReadOnlyList<Card> cards);

    RankedHand ComputeRankedHand(IReadOnlyList<Card> cards);
}

public class TexasHoldemRankingStrategy : IRankingStrategy
{   
    public HandRankType ComputeHandRank(IReadOnlyList<Card> cards)
    {
        if (IsRoyalFlush(cards))
        {
            return HandRankType.RoyalFlush;
        }

        if (IsStraightFlush(cards))
        {
            return HandRankType.StraightFlush;
        }

        if (IsFourOfAKind(cards))
        {
            return HandRankType.FourOfAKind;
        }

        if (IsFullHouse(cards))
        {
            return HandRankType.FullHouse;
        }

        if (IsFlush(cards))
        {
            return HandRankType.Flush;
        }

        if (IsStraight(cards))
        {
            return HandRankType.Straight;
        }

        if (IsThreeOfAKind(cards))
        {
            return HandRankType.ThreeOfAKind;
        }

        if (IsTwoPair(cards))
        {
            return HandRankType.TwoPair;
        }

        if (IsPair(cards))
        {
            return HandRankType.Pair;
        }

        return HandRankType.HighCard;
    }

    private bool AreAllSameSuit(IReadOnlyList<Card> cards)
    {
        return cards.All(c => c.Suit == cards[0].Suit);
    }

    private bool IsRoyalFlush(IReadOnlyList<Card> cards)
    {
        if (cards.Count < 5)
        {
            return false;
        }

        if (!AreAllSameSuit(cards))
        {
            return false;
        }

        var ordered = cards
            .OrderBy(c => c.Rank)
            .ToArray();

        if (ordered[0].Rank != CardRank.Ace)
        {
            return false;
        }
        if (ordered[1].Rank != CardRank.Ten)
        {
            return false;
        }
        if (ordered[2].Rank != CardRank.Jack)
        {
            return false;
        }
        if (ordered[3].Rank != CardRank.Queen)
        {
            return false;
        }
        if (ordered[4].Rank != CardRank.King)
        {
            return false;
        }

        return true;
    }

    private bool IsStraightFlush(IReadOnlyList<Card> cards)
    {
        if (cards.Count < 5)
        {
            return false;
        }

        return AreAllSameSuit(cards) && IsStraight(cards);
    }

    private bool IsFourOfAKind(IReadOnlyList<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Rank)
            .ToArray();

        return groups.Any(g => g.Count() == 4);
    }

    private bool IsFullHouse(IReadOnlyList<Card> cards)
    {
        return IsThreeOfAKind(cards) && IsPair(cards);
    }

    private bool IsFlush(IReadOnlyList<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Suit)
            .ToArray();

        return groups.Length == 1;
    }

    private bool IsStraight(IReadOnlyList<Card> cards)
    {
        if (cards.Count < 5)
        {
            return false;
        }

        var orderedCards = cards
            .OrderBy(c => c.Rank)
            .ToArray();

        var orderedRanks = orderedCards
            .Select(c => (int)c.Rank)
            .ToArray();

        var normalizationValue = orderedRanks[0] - 1;

        for (var i = 0; i < orderedRanks.Length; i++)
        {
            orderedRanks[i] -= normalizationValue;
        }

        var expected = new[] { 1, 2, 3, 4, 5 };

        return orderedRanks.SequenceEqual(expected);
    }

    private bool IsThreeOfAKind(IReadOnlyList<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Rank)
            .ToArray();

        return groups.Any(g => g.Count() == 3);
    }

    private bool IsTwoPair(IReadOnlyList<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Rank)
            .ToArray();

        return groups.Count(g => g.Count() == 2) == 2;
    }

    private bool IsPair(IReadOnlyList<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Rank)
            .ToArray();

        return groups.Any(g => g.Count() == 2);
    }

    public int ComputeHandStrength(IReadOnlyList<Card> cards)
    {
        var rank = ComputeHandRank(cards);

        var ordered = cards
            .OrderBy(c => c.Rank)
            .ToArray();

        switch (rank)
        {
            case HandRankType.RoyalFlush:
                return 0;

            case HandRankType.StraightFlush:
                return (int)ordered[4].Rank;

            case HandRankType.FourOfAKind:
                return (int)ordered[2].Rank;

            case HandRankType.FullHouse:
                return (int)ordered[2].Rank;

            case HandRankType.Flush:
                return (int)ordered[4].Rank;

            case HandRankType.Straight:
                return (int)ordered[4].Rank;

            case HandRankType.ThreeOfAKind:
                return (int)ordered[2].Rank;

            case HandRankType.TwoPair:
                return (int)ordered[3].Rank;

            case HandRankType.Pair:
                return (int)ordered[3].Rank;

            case HandRankType.HighCard:
                return (int)ordered[4].Rank;

            default:
                throw new ArgumentOutOfRangeException(nameof(rank));
        }
    }



    public RankedHand ComputeRankedHand(IReadOnlyList<Card> cards)
    {
        var rank = ComputeHandRank(cards);
        var strength = ComputeHandStrength(cards);

        return new RankedHand(cards, rank, strength);
    }

}
