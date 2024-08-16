using Aidan.Core.Patterns;

namespace CassinoDemo.Poker;

public interface IPokerService
{
    /*
     * Description of business rules
     * 
     * 1. The game is played with a standard deck of 52 cards.
     * 2. The deck is shuffled before the game starts.
     * 3. The game starts with the player and the dealer each receiving two cards.
     * 4. The player can see their own cards and one of the dealer's cards.
     * 5. The player can choose to hit or stand.
     */

    PokerGame StartGame();
}

public class PokerService : IPokerService
{
    public PokerGame StartGame()
    {
        return new PokerGame(
            id: Guid.NewGuid(),
            table: CreateTable(),
            dealer: CreateDealer()
        );
    }

    /*
     * private helpers
     */

    private Table CreateTable()
    {
        return Table.Create()
            .WithDeck(CreateDeck())
            .Build();
    }

    private CardDeck CreateDeck()
    {
        var builder = CardDeck.CreateBuilder();

        foreach (var suit in Enum.GetValues<CardSuit>())
        {
            foreach (var rank in Enum.GetValues<CardRank>())
            {
                builder.AddCard(new Card(suit: suit, rank: rank, orientation: CardOrientation.FaceDown));
            }
        }

        return builder.Build();
    }

    private Dealer CreateDealer()
    {
        return Dealer.Create()
            .WithRules(CreateRules())
            .Build();   
    }

    private GameRules CreateRules()
    {
        return new GameRules(
            minPlayers: 2,
            maxPlayers: 5,
            smallBlindValue: 5,
            bigBlindValue: 10,
            shufflingStrategy: new RandomShufflingStrategy());
    }

}
