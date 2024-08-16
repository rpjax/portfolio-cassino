namespace CassinoDemo.Poker;

public class CommunityCards : CardCollection
{
    public CommunityCards(IEnumerable<Card> cards) : base(cards)
    {
    }

    public static CommunityCards CreateEmpty()
    {
        return new CommunityCards(new List<Card>());
    }

    public void AddCard(Card card)
    {
        CardList.Add(card);
    }

    public IReadOnlyList<Card> RemoveAllCards()
    {
        var cards = CardList.ToList();
        CardList.Clear();
        return cards;
    }
}

