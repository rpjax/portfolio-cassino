namespace Domain.Poker;

public class PlayerHand : CardCollection, IValueObject
{
    public bool IsFolded { get; private set; }

    public PlayerHand(
        IEnumerable<Card> cards, 
        bool isFolded) : base(cards)
    {
        IsFolded = isFolded;
    }

    public bool IsEmpty => Cards.Count == 0;
    public bool IsNotEmpty => !IsEmpty;
    public bool IsPlayable => !IsFolded && IsNotEmpty;
    public IReadOnlyList<Card> Cards => CardList;

    public override string ToString()
    {
        return string.Join(", ", Cards);
    }

    public void AddCard(Card card)
    {
        if(IsFolded)
        {
            throw new InvalidOperationException("Player is folded.");
        }

        CardList.Add(card);
    }

    public IReadOnlyList<Card> RemoveAllCards()
    {
        var cards = new List<Card>(Cards);
        CardList.Clear();
        return cards;
    }

    public void Fold()
    {
        if(IsFolded)
        {
            throw new InvalidOperationException("Player is already folded.");
        }

        IsFolded = true;
    }

}
