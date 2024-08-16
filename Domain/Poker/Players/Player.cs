using Aidan.Core.Patterns;

namespace CassinoDemo.Poker;

public class Player : IEntity
{
    public Guid Id { get; }
    public string Name { get; }
    public IReadOnlyList<Button> Buttons { get; }
    public PlayerBankroll Bankroll { get; }
    public PlayerHand? Hand { get; internal set; }
    public Bet? Bet { get; internal set; }

    public Player(
        Guid id, 
        string name, 
        IEnumerable<Button> buttons,
        PlayerBankroll bankroll, 
        PlayerHand? hand)
    {
        Id = id;
        Name = name;
        Buttons = new List<Button>(buttons);
        Bankroll = bankroll;
        Hand = hand;
    }

    public bool IsPlaying => Hand is not null && Hand.IsPlayable;
    public bool HasTurnButton => HasButton(ButtonType.Turn);
    public bool IsDealer => HasButton(ButtonType.Dealer);
    public bool IsSmallBlind => HasButton(ButtonType.SmallBlind);
    public bool IsBigBlind => HasButton(ButtonType.BigBlind);

    internal List<Button> ButtonsList => (List<Button>)Buttons;

    /*
     * Factories
     */

    public static PlayerBuilder CreateBuilder()
    {
        return new PlayerBuilder();
    }

    /*
     * Technical methods
     */

    public override string ToString()
    {
        var buttons = string.Join(", ", ButtonsList.Select(x => x.ToString()));
        var bankroll = Bankroll.ToString();
        var bet = Bet?.ToString() ?? "No bet";
        var hand = Hand?.ToString() ?? "No hand";

        return $"{Name} [{buttons}] bankroll: {bankroll}, bet: {bet}, hand: {hand}";
    }

    public bool HasButton(ButtonType type)
    {
        return ButtonsList.Any(b => b.Type == type);
    }

    /*
     * Button behavior
     */

    public void AddButton(ButtonType type)
    {
        if (ButtonsList.Any(x => x.Type == type))
        {
            throw new ArgumentException("Button already exists");
        }

        ButtonsList.Add(new Button(type: type));
    }

    public void AddButtons(IEnumerable<ButtonType> buttons)
    {
        foreach (var button in buttons)
        {
            AddButton(button);
        }
    }

    public Button RemoveButton(ButtonType type)
    {
        var index = ButtonsList.FindIndex(b => b.Type == type);
        
        if(index == -1)
        {
            throw new ArgumentException("Button not found");
        }

        var button = ButtonsList[index];

        ButtonsList.RemoveAt(index);
        return button;
    }

    public IReadOnlyList<Button> RemoveAllButtons()
    {
        var buttons = new List<Button>(ButtonsList);
        ButtonsList.Clear();
        return buttons;
    }

    /*
     * Bankroll behavior
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
        if (amount <= 0)
        {
            throw new ArgumentException("Invalid small blind amount");
        }

        if (Bankroll.Balance < amount)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        Bankroll.Debit(amount);
        Bet = new Bet(Id, amount);
    }

    public void PlaceBigBlindBet(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Invalid big blind amount");
        }

        if (Bankroll.Balance < amount)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        Bankroll.Debit(amount);
        Bet = new Bet(Id, amount);
    }

    public void Check()
    {
        return;
    }

    public void Call(decimal amount)
    {
        var callAmount = amount - (Bet?.Amount ?? 0);

        if(callAmount < 0)
        {
            throw new InvalidOperationException("Invalid call amount");
        }
        if(callAmount > Bankroll.Balance)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        Bankroll.Debit(callAmount);
        Bet = new Bet(Id, amount);
    }

    public void Raise(decimal amount)
    {
        if(amount <= 0)
        {
            throw new ArgumentException("Invalid raise amount");
        }
        if(amount > Bankroll.Balance)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        var total = amount + (Bet?.Amount ?? 0);

        Bankroll.Debit(amount);
        Bet = new Bet(Id, total);
    }

    public void Fold()
    {
        Hand.Fold();
    }

    public void AllIn()
    {
        if(Bankroll.Balance == 0)
        {
            throw new InvalidOperationException("Player is already all-in");
        }

        var allInAmount = (Bet?.Amount ?? 0) + Bankroll.Balance;
        Bankroll.Debit(Bankroll.Balance);
        Bet = new Bet(Id, allInAmount);
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
