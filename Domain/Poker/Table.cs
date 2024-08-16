using Aidan.Core.Patterns;

namespace CassinoDemo.Poker;

public class Table
{
    public PlayerSeats Seats { get; }
    public CardDeck Deck { get; }
    public CardWaste CardWaste { get; }
    public Pot Pot { get; }
    public CommunityCards CommunityCards { get; }

    public Table(
        PlayerSeats seats,
        CardDeck deck,
        CardWaste cardWaste,
        Pot pot, 
        CommunityCards communityCards)
    {
        Seats = seats;
        Deck = deck;
        CardWaste = cardWaste;
        Pot = pot;
        CommunityCards = communityCards;
    }

    public IReadOnlyList<Player> Players => Seats;

    public static TableBuilder Create()
    {
        return new TableBuilder();
    }

    public void AddPlayer(Player player)
    {
        throw new NotImplementedException();
    }

    public void RemovePlayer(Player player)
    {
        throw new NotImplementedException();
    }

}

public class TableBuilder : IBuilder<Table>
{
    private List<Player> Players { get; } = new();
    private CardDeck? Deck { get; set; } = null;
    private Pot? Pot { get; set; } = null;
    private CommunityCards? CommunityCards { get; set; } = null;

    public Table Build()
    {
        return new Table(
            seats: new PlayerSeats(players: Players),
            deck: Deck ?? throw new InvalidOperationException("Deck is required"),
            cardWaste: new CardWaste(cards: Array.Empty<Card>()),
            pot: Pot ?? new Pot(bets: Array.Empty<Bet>()),
            communityCards: CommunityCards ?? new CommunityCards(cards: Array.Empty<Card>())
        );
    }

    public TableBuilder WithPlayer(Player player)
    {
        Players.Add(player);
        return this;
    }

    public TableBuilder WithPlayers(IEnumerable<Player> players)
    {
        Players.AddRange(players);
        return this;
    }

    public TableBuilder WithDeck(CardDeck deck)
    {
        Deck = deck;
        return this;
    }

    public TableBuilder WithPot(Pot pot)
    {
        Pot = pot;
        return this;
    }

    public TableBuilder WithCommunityCards(CommunityCards communityCards)
    {
        CommunityCards = communityCards;
        return this;
    }

    public TableBuilder WithCommunityCard(Card card)
    {
        CommunityCards ??= new CommunityCards(cards: Array.Empty<Card>());
        CommunityCards.AddCard(card);
        return this;
    }
}
