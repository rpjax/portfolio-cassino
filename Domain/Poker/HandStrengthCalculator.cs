namespace Domain.Poker;

public class HandRankCalculator
{
    public HandRankType RankType { get; }
    public IReadOnlyList<Card> RankCards { get; }

    private Card[]? FourOfAKind { get; }
    private Card[]? FullHouse { get; }
    private Card[]? Flush { get; }
    private Card[]? Straight { get; }
    private Card[]? ThreeOfAKind { get; }
    private Card[]? TwoPair { get; }
    private Card[]? Pair { get; }
    private Card[]? HighCard { get; }

    private IReadOnlyList<Card> Cards { get; }

    public HandRankCalculator(IReadOnlyList<Card> cards)
    {
        Cards = cards;
        RankType = ComputeRankType();
        RankCards = ComputeRankCards();
        FourOfAKind = GetFourOfAKind(Cards);
        FullHouse = GetFullHouse(Cards);
        Flush = GetFlush(Cards);
        Straight = GetStraight(Cards);
        ThreeOfAKind = GetThreeOfAKind(Cards);
        TwoPair = GetTwoPair(Cards);
        Pair = GetPair(Cards);
        HighCard = GetHighCard(Cards);
    }

    public bool IsFourOfAKind => FourOfAKind is not null;
    public bool IsFullHouse => FullHouse is not null;
    public bool IsFlush => Flush is not null;
    public bool IsStraight => Straight is not null;
    public bool IsThreeOfAKind => ThreeOfAKind is not null;
    public bool IsTwoPair => TwoPair is not null;
    public bool IsPair => Pair is not null;
    public bool IsHighCard => HighCard is not null;

    private HandRankType ComputeRankType()
    {
        if (IsRoyalFlush)
        {
            return HandRankType.RoyalFlush;
        }

        if (IsStraightFlush)
        {
            return HandRankType.StraightFlush;
        }

        if (IsFourOfAKind)
        {
            return HandRankType.FourOfAKind;
        }

        if (IsFullHouse)
        {
            return HandRankType.FullHouse;
        }

        if (IsFlush)
        {
            return HandRankType.Flush;
        }

        if (IsStraight)
        {
            return HandRankType.Straight;
        }

        if (IsThreeOfAKind)
        {
            return HandRankType.ThreeOfAKind;
        }

        if (IsTwoPair)
        {
            return HandRankType.TwoPair;
        }

        if (IsPair)
        {
            return HandRankType.Pair;
        }

        return HandRankType.HighCard;
    }

    private Card[]? GetFourOfAKind(IEnumerable<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Rank)
            .OrderByDescending(g => g.Key)
            .ToArray();

        var fourOfAKind = groups
            .FirstOrDefault(g => g.Count() >= 4);

        if (fourOfAKind == null)
        {
            return null;
        }

        return fourOfAKind
            .Take(4)
            .ToArray();
    }

    private Card[]? GetFullHouse(IEnumerable<Card> cards)
    {
        var threeOfAKind = GetThreeOfAKind(cards);

        if (threeOfAKind == null)
        {
            return null;
        }

        var pair = GetPair(cards.Except(threeOfAKind));

        if (pair == null)
        {
            return null;
        }

        return threeOfAKind
            .Concat(pair)
            .ToArray();
    }

    private Card[]? GetFlush(IEnumerable<Card> cards)
    {
        if (Cards.Count < 5)
        {
            return null;
        }

        var groups = cards
            .GroupBy(c => c.Suit)
            .Where(g => g.Count() >= 5)
            .OrderByDescending(g => g.Max(c => Card.GetRankValue(c)))
            .ToArray();

        if (groups.Length == 0)
        {
            return null;
        }

        var flush = groups
            .First()
            .Take(5)
            .ToArray();
        return flush;
    }

    private Card[]? GetStraight(IEnumerable<Card> cards)
    {
        var cardArray = cards.ToArray();

        if (cardArray.Length < 5)
        {
            return null;
        }

        var orderedCards = cardArray
            .OrderBy(c => c.Rank)
            .ToArray();

        foreach (var card in orderedCards)
        {
            var normalizationValue = Card.GetRankValue(card) - 1;
            var normalizedRanks = orderedCards
                .Select(card => Card.GetRankValue(card))
                .Select(rank => rank - normalizationValue)
                .ToArray();
            var rankSequence = normalizedRanks
                .Take(5)
                .ToArray();

            var expected = new[] { 1, 2, 3, 4, 5 };

            if (rankSequence.SequenceEqual(expected))
            {
                var cardSequence = orderedCards
                    .Take(5)
                    .ToArray();
                return cardSequence;
            }

            // TODO: add the case for high ace straight and any other edge cases
        }

        return null;
    }

    private Card[]? GetThreeOfAKind(IEnumerable<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Rank)
            .OrderByDescending(g => g.Key)
            .ToArray();

        var threeOfAKind = groups
            .FirstOrDefault(g => g.Count() == 3);

        if (threeOfAKind == null)
        {
            return null;
        }

        return threeOfAKind.ToArray();
    }

    private Card[]? GetTwoPair(IEnumerable<Card> cards)
    {
        var firstPair = GetPair(cards);

        if (firstPair == null)
        {
            return null;
        }

        var secondPair = GetPair(cards.Except(firstPair));

        if (secondPair == null)
        {
            return null;
        }

        return firstPair
            .Concat(secondPair)
            .ToArray();
    }

    private Card[]? GetPair(IEnumerable<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Rank)
            .OrderByDescending(g => g.Key)
            .ToArray();

        var pair = groups
            .FirstOrDefault(g => g.Count() == 2);

        if (pair == null)
        {
            return null;
        }

        return pair.ToArray();
    }

    private Card[]? GetHighCard(IEnumerable<Card> cards)
    {
        return cards
            .OrderByDescending(c => Card.GetRankValue(c))
            .Take(1)
            .ToArray();
    }
}

public class HandStrengthCalculator
{
    private IReadOnlyList<Card> Cards { get; }
    private HandRankType HandRankType { get; }

    public HandStrengthCalculator(IReadOnlyList<Card> cards)
    {
        Cards = cards;
        HandRankType = ComputeRankType();
    }

    public int ComputeStrength()
    {
        // defines the weight of each term.
        int rankCoefficient = 1000;
        int tiebreakerCoefficient = 100;
        int kickerCoefficient = 10;

        // computes the base value for each term.
        int rank = ComputeRankValue();
        int tiebreaker = ComputeTiebreakerValue();
        int kicker = ComputeKickerValue();

        // computes the terms, spaced by the coefficient.
        int rankTerm = rank * rankCoefficient;
        int tiebreakerTerm = tiebreaker * tiebreakerCoefficient;
        int kickerTerm = kicker * kickerCoefficient;

        return rankTerm + tiebreakerTerm + kickerTerm;
    }

    /* Compute Rank */

    private int ComputeRankValue()
    {
        return (int)HandRankType;
    }

    private HandRankType ComputeRankType()
    {
        if (IsRoyalFlush())
        {
            return HandRankType.RoyalFlush;
        }

        if (IsStraightFlush())
        {
            return HandRankType.StraightFlush;
        }

        if (IsFourOfAKind())
        {
            return HandRankType.FourOfAKind;
        }

        if (IsFullHouse())
        {
            return HandRankType.FullHouse;
        }

        if (IsFlush())
        {
            return HandRankType.Flush;
        }

        if (IsStraight())
        {
            return HandRankType.Straight;
        }

        if (IsThreeOfAKind())
        {
            return HandRankType.ThreeOfAKind;
        }

        if (IsTwoPair())
        {
            return HandRankType.TwoPair;
        }

        if (IsPair())
        {
            return HandRankType.Pair;
        }

        return HandRankType.HighCard;
    }

    private bool AreAllSameSuit()
    {
        return Cards.All(c => c.Suit == Cards[0].Suit);
    }

    private bool SameSuit(int count)
    {
        var groups = Cards
            .GroupBy(c => c.Suit)
            .ToArray();

        return groups.Any(g => g.Count() >= count);
    }

    private bool IsSameSuitSequence(int count)
    {
        var groups = Cards
            .GroupBy(c => c.Suit)
            .ToArray();

        return groups.Any(g => g.Count() >= count);
    }

    private Card[] GetGreatestSuitGroup(IEnumerable<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Suit)
            .OrderByDescending(g => g.Count())
            .ToArray();

        return groups
            .First()
            .ToArray();
    }

    private bool IsRoyalFlush()
    {
        var cards = Cards;

        var ordered = cards
            .OrderBy(c => c.Rank)
            .ToArray();

        var containsAce = ordered.Any(c => c.Rank == CardRank.Ace);
        var containsTen = ordered.Any(c => c.Rank == CardRank.Ten);
        var containsJack = ordered.Any(c => c.Rank == CardRank.Jack);
        var containsQueen = ordered.Any(c => c.Rank == CardRank.Queen);
        var containsKing = ordered.Any(c => c.Rank == CardRank.King);

        if (!containsAce)
        {
            return false;
        }
        if (!containsTen)
        {
            return false;
        }
        if (!containsJack)
        {
            return false;
        }
        if (!containsQueen)
        {
            return false;
        }
        if (!containsKing)
        {
            return false;
        }

        return true;
    }

    private bool IsStraightFlush()
    {
        if (!IsStraight())
        {
            return false;
        }

        return AreAllSameSuit() && IsStraight();
    }

    private bool IsFourOfAKind()
    {
        return GetFourOfAKind(Cards) is not null;
    }

    private bool IsFullHouse()
    {
        return GetFullHouse(Cards) is not null;
    }

    private bool IsFlush()
    {
        return GetFlush(Cards) is not null;
    }

    private bool IsStraight()
    {
        return GetStraight(Cards) is not null;
    }

    private bool IsThreeOfAKind()
    {
        return GetThreeOfAKind(Cards) is not null;
    }

    private bool IsTwoPair()
    {
        return GetTwoPair(Cards) is not null;
    }

    private bool IsPair()
    {
        return GetPair(Cards) is not null;
    }

    /* Compute Tiebreaker */

    private int ComputeTiebreakerValue()
    {
        return 0;
        //switch (ComputeRankType())
        //{
        //    case HandRankType.RoyalFlush:
        //        return 0;

        //    case HandRankType.StraightFlush:
        //        return ComputeStraightFlushTiebreakerValue();

        //    case HandRankType.FourOfAKind:
        //        return ComputeFourOfAKindTiebreakerValue();

        //    case HandRankType.FullHouse:
        //        return ComputeFullHouseTiebreakerValue();

        //    case HandRankType.Flush:
        //        return ComputeFlushTiebreakerValue();

        //    case HandRankType.Straight:
        //        return ComputeStraightTiebreakerValue();

        //    case HandRankType.ThreeOfAKind:
        //        return ComputeThreeOfAKindTiebreakerValue();

        //    case HandRankType.TwoPair:
        //        return ComputeTwoPairTiebreakerValue();

        //    case HandRankType.Pair:
        //        return ComputePairTiebreakerValue();

        //    case HandRankType.HighCard:
        //        return ComputeHighCardTiebreakerValue();

        //    default:
        //        throw new InvalidOperationException();
        //}
    }

    /* Compute Kicker */

    private int ComputeKickerValue()
    {
        return 0;
    }

    //private Card[]? GetRoyalStraightFlush(ref Card[] cards)
    //{
    //    var sameSuitCards = GetGreatestSuitGroup(cards);

    //    if (sameSuitCards.Length < 5)
    //    {
    //        return null;
    //    }

    //    var ace = sameSuitCards.FirstOrDefault(c => c.Rank == CardRank.Ace);
    //    var ten = sameSuitCards.FirstOrDefault(c => c.Rank == CardRank.Ten);    
    //    var jack = sameSuitCards.FirstOrDefault(c => c.Rank == CardRank.Jack);
    //    var queen = sameSuitCards.FirstOrDefault(c => c.Rank == CardRank.Queen);
    //    var king = sameSuitCards.FirstOrDefault(c => c.Rank == CardRank.King);

    //    var allPresent = ace != null 
    //        && ten != null 
    //        && jack != null 
    //        && queen != null 
    //        && king != null;

    //    if (!allPresent)
    //    {
    //        return null;
    //    }

    //    var hand = new Card[] { ace!, ten!, jack!, queen!, king! };
    //    var leftout = sameSuitCards
    //        .Except(hand)
    //        .ToArray();

    //    cards = leftout;

    //    return hand;
    //}

    private Card[]? GetFourOfAKind(IEnumerable<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Rank)
            .OrderByDescending(g => g.Key)
            .ToArray();

        var fourOfAKind = groups
            .FirstOrDefault(g => g.Count() >= 4);

        if (fourOfAKind == null)
        {
            return null;
        }

        return fourOfAKind
            .Take(4)
            .ToArray();
    }

    private Card[]? GetFullHouse(IEnumerable<Card> cards)
    {
        var threeOfAKind = GetThreeOfAKind(cards);

        if (threeOfAKind == null)
        {
            return null;
        }

        var pair = GetPair(cards.Except(threeOfAKind));

        if (pair == null)
        {
            return null;
        }

        return threeOfAKind
            .Concat(pair)
            .ToArray();
    }

    private Card[]? GetFlush(IEnumerable<Card> cards)
    {
        if(Cards.Count < 5)
        {
            return null;
        }

        var groups = cards
            .GroupBy(c => c.Suit)
            .Where(g => g.Count() >= 5)
            .OrderByDescending(g => g.Max(c => Card.GetRankValue(c)))
            .ToArray();

        if(groups.Length == 0)
        {
            return null;
        }

        var flush = groups
            .First()
            .Take(5)
            .ToArray();
        return flush;
    }

    private Card[]? GetStraight(IEnumerable<Card> cards)
    {
        var cardArray = cards.ToArray();

        if (cardArray.Length < 5)
        {
            return null;
        }

        var orderedCards = cardArray
            .OrderBy(c => c.Rank)
            .ToArray();

        foreach (var card in orderedCards)
        {
            var normalizationValue = Card.GetRankValue(card) - 1;
            var normalizedRanks = orderedCards
                .Select(card => Card.GetRankValue(card))
                .Select(rank => rank - normalizationValue)
                .ToArray();
            var rankSequence = normalizedRanks
                .Take(5)
                .ToArray();

            var expected = new[] { 1, 2, 3, 4, 5 };

            if (rankSequence.SequenceEqual(expected))
            {
                var cardSequence = orderedCards
                    .Take(5)
                    .ToArray();
                return cardSequence;
            }

            // TODO: add the case for high ace straight and any other edge cases
        }

        return null;
    }

    private Card[]? GetThreeOfAKind(IEnumerable<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Rank)
            .OrderByDescending(g => g.Key)
            .ToArray();

        var threeOfAKind = groups
            .FirstOrDefault(g => g.Count() == 3);

        if (threeOfAKind == null)
        {
            return null;
        }

        return threeOfAKind.ToArray();
    }

    private Card[]? GetTwoPair(IEnumerable<Card> cards)
    {
        var firstPair = GetPair(cards);

        if (firstPair == null)
        {
            return null;
        }

        var secondPair = GetPair(cards.Except(firstPair));

        if (secondPair == null)
        {
            return null;
        }

        return firstPair
            .Concat(secondPair)
            .ToArray();
    }

    private Card[]? GetPair(IEnumerable<Card> cards)
    {
        var groups = cards
            .GroupBy(c => c.Rank)
            .OrderByDescending(g => g.Key)
            .ToArray();

        var pair = groups
            .FirstOrDefault(g => g.Count() == 2);

        if (pair == null)
        {
            return null;
        }

        return pair.ToArray();
    }

}