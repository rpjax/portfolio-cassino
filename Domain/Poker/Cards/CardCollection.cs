using System.Collections;

namespace CassinoDemo.Poker;

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

