namespace Domain.Poker;

public class CardWaste : CardCollection, IValueObject
{
    public CardWaste(IEnumerable<Card> cards) : base(cards)
    {
        
    }

    /*
     * Factories
     */

    public static CardWaste CreateEmpty()
    {
        return new CardWaste(cards: Array.Empty<Card>());
    }

    /*
     * Behavior
     */

    public void AddCard(Card card)
    {
        CardList.Add(card);
    }

    public void AddCards(IEnumerable<Card> cards)
    {
        CardList.AddRange(cards);
    }

    public IReadOnlyList<Card> RemoveAllCards()
    {
        var cards = new List<Card>(CardList);
        CardList.Clear();
        return cards;
    }

}

