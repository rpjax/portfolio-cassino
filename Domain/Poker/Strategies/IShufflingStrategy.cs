namespace Domain.Poker;

public interface IShufflingStrategy
{
    void Shuffle(IList<Card> cards);
}

public class RandomShufflingStrategy : IShufflingStrategy
{
    public void Shuffle(IList<Card> cards)
    {
        for (int i = 0; i < 50; i++)
        {
            var n = cards.Count;
            while (n > 1)
            {
                n--;
                var k = new Random().Next(n + 1);
                var value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
        }
    }
}


