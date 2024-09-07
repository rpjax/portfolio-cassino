using System.Collections;

namespace Domain.Poker;

public enum CardSuit
{
    Clubs,
    Diamonds,
    Hearts,
    Spades
}

public enum CardRank
{
    Ace,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King
}

public enum CardOrientation
{
    FaceDown,
    FaceUp
}

public class Card
{
    public CardSuit Suit { get; }
    public CardRank Rank { get; }
    public CardOrientation Orientation { get; internal set; }

    public Card(
        CardSuit suit, 
        CardRank rank, 
        CardOrientation orientation)
    {
        Suit = suit;
        Rank = rank;
        Orientation = orientation;
    }

    public override string ToString()
    {
        return $"{Rank} of {Suit}";
    }

    public static int GetRankValue(CardRank rank)
    {
        return rank switch
        {
            CardRank.Ace => 1,
            CardRank.Two => 2,
            CardRank.Three => 3,
            CardRank.Four => 4,
            CardRank.Five => 5,
            CardRank.Six => 6,
            CardRank.Seven => 7,
            CardRank.Eight => 8,
            CardRank.Nine => 9,
            CardRank.Ten => 10,
            CardRank.Jack => 11,
            CardRank.Queen => 12,
            CardRank.King => 13,
            _ => throw new InvalidOperationException("Invalid card rank.")
        };
    }

    public static int GetRankValue(Card card)
    {
        return GetRankValue(card.Rank);
    }
}

public class CardCollection : IReadOnlyList<Card>
{
    protected List<Card> CardList { get; }

    public CardCollection(IEnumerable<Card> cards)
    {
        CardList = new List<Card>(cards);
    }

    public Card this[int index] => CardList[index];

    public int Count => CardList.Count;

    public IEnumerator<Card> GetEnumerator()
    {
        return CardList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
