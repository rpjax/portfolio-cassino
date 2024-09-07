using Aidan.Core.Patterns;

namespace Domain.Poker;

public class CardDeck : CardCollection, IEnumerable<Card>, IValueObject
{
    public CardDeck(IEnumerable<Card> cards) : base(cards)
    {
        
    }

    /*
     * Factories
     */

    public static CardDeckBuilder CreateBuilder()
    {
        return new CardDeckBuilder();
    }

    /*
     * Behavior
     */

    public void Shuffle(IShufflingStrategy strategy)
    {
        strategy.Shuffle(CardList);
    }

    public Card DrawCard()
    {
        var card = CardList.First();
        CardList.RemoveAt(0);
        return card;
    }

    public void AddCard(Card card)
    {
        CardList.Add(card);
    }

    public void AddCards(IEnumerable<Card> cards)
    {
        CardList.AddRange(cards);
    }

    /*
     * Other methods
     */

}

public class CardDeckBuilder : IBuilder<CardDeck>
{
    private List<Card> Cards { get; } = new();

    public static CardDeck Standard52Cards()
    {
        var builder = new CardDeckBuilder();

        foreach (var suit in Enum.GetValues<CardSuit>())
        {
            foreach (var rank in Enum.GetValues<CardRank>())
            {
                builder.AddCard(new Card(suit: suit, rank: rank, orientation: CardOrientation.FaceDown));
            }
        }

        return builder.Build();
    }

    public CardDeck Build()
    {
        return new CardDeck(cards: Cards);
    }

    public CardDeckBuilder AddCard(Card card)
    {
        Cards.Add(card);
        return this;
    }

    public CardDeckBuilder AddCards(IEnumerable<Card> cards)
    {
        Cards.AddRange(cards);
        return this;
    }
}
