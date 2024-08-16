namespace CassinoDemo.Poker;

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

public struct Card
{
    public CardSuit Suit { get; }
    public CardRank Value { get; }
    public CardOrientation Orientation { get; internal set; }

    public Card(
        CardSuit suit, 
        CardRank rank, 
        CardOrientation orientation)
    {
        Suit = suit;
        Value = rank;
        Orientation = orientation;
    }

    public override string ToString()
    {
        return $"{Value} of {Suit} ({Orientation})";
    }
}
