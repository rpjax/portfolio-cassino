using Aidan.Core.Patterns;
using System.Collections;

namespace Domain.Poker;

public class Player : IEntity
{
    public Guid Id { get; }
    public string Name { get; }
    public PlayerBankroll Bankroll { get; }
    public PlayerHand? Hand { get; internal set; }

    public bool IsSleeping { get; internal set; }

    public Player(
        Guid id,
        string name,
        IEnumerable<Button> buttons,
        PlayerBankroll bankroll,
        PlayerHand? hand)
    {
        Id = id;
        Name = name;
        Bankroll = bankroll;
        Hand = hand;

    }

    public bool HasHand => Hand is not null;
    public bool HandIsFolded => Hand?.IsFolded == true;
    public bool HandIsNotFolded => !HandIsFolded;
    public bool IsActive => HasHand && HandIsNotFolded;
    public bool IsNotSleeping => !IsSleeping;

    public decimal BankrollBalance => Bankroll.Balance;

    /*
     * Factories
     */

    public static PlayerBuilder CreateBuilder()
    {
        return new PlayerBuilder();
    }

    /*
     * Helpers
     */

    public override string ToString()
    {
        var bankroll = Bankroll.ToString();
        var hand = Hand?.ToString() ?? "No hand";

        return $"{Name}; bankroll: {bankroll}; hand: [{hand}]";
    }

    /*
     * Betting behavior
     */

    public void CollectWinnings(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Invalid winnings amount");
        }

        Bankroll.Credit(amount);
    }

    public void Rebuy(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Invalid rebuy amount");
        }

        Bankroll.Credit(amount);
    }

    /*
     * Hand behavior
     */

    public void TakeCard(Card card)
    {
        Hand ??= new PlayerHand(cards: Array.Empty<Card>(), isFolded: false);
        Hand.AddCard(card);
    }

    public IReadOnlyList<Card> RemoveAllCards()
    {
        var cards = Hand?.RemoveAllCards() ?? new List<Card>();
        Hand = null;
        return cards;
    }

    /*
     * Play actions
     */

    public void PlaceSmallBlindBet(decimal amount)
    {
        Bankroll.Debit(amount);
    }

    public void PlaceBigBlindBet(decimal amount)
    {
        Bankroll.Debit(amount);
    }

    public void Check()
    {
        return;
    }

    public void Bet(decimal amount)
    {
        Bankroll.Debit(amount);
    }

    public void Fold()
    {
        if (Hand is null)
        {
            throw new InvalidOperationException("Player has no hand.");
        }

        Hand.Fold();
    }

    public void Call(decimal calledAmount)
    {
        Bankroll.Debit(calledAmount);
    }

    public void Raise(decimal raisedAmount)
    {
        Bankroll.Debit(raisedAmount);
    }

    public void AllIn()
    {
        Bankroll.Debit(Bankroll.Balance);
    }

}

public class PlayerBuilder : IBuilder<Player>
{
    private Guid Id { get; set; }
    private string? Name { get; set; }
    private decimal BankrollBalance { get; set; }

    public Player Build()
    {
        return new Player(
            id: Id,
            name: Name ?? throw new InvalidOperationException("Name is required"),
            buttons: new List<Button>(),
            bankroll: new PlayerBankroll(BankrollBalance),
            hand: null
        );
    }

    public PlayerBuilder WithId(Guid id)
    {
        Id = id;
        return this;
    }

    public PlayerBuilder WithName(string name)
    {
        Name = name;
        return this;
    }

    public PlayerBuilder WithBankrollBalance(decimal bankroll)
    {
        BankrollBalance = bankroll;
        return this;
    }
}

public class PlayerCollection : IReadOnlyList<Player>
{
    protected List<Player> PlayerList { get; }

    public PlayerCollection(IEnumerable<Player> players)
    {
        PlayerList = new List<Player>(players);
    }

    public Player this[int index] => PlayerList[index];
    public int Count => PlayerList.Count;

    public IEnumerator<Player> GetEnumerator()
    {
        return PlayerList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /*
     * Query helpers
     */

    public Player? FindPlayer(Guid id)
    {
        return PlayerList.FirstOrDefault(p => p.Id == id);
    }

    public Player GetPlayer(Guid id)
    {
        var player = FindPlayer(id);

        if (player is null)
        {
            throw new InvalidOperationException("Player not found.");
        }

        return player;
    }

    public Player GetPlayerToTheLeftOf(Player player)
    {
        var index = PlayerList.IndexOf(player);

        if (index == -1)
        {
            throw new InvalidOperationException("Player not found");
        }

        var leftIndex = index + 1;

        if (leftIndex >= PlayerList.Count)
        {
            leftIndex = 0;
        }

        return PlayerList[leftIndex];
    }

}
