using Aidan.Core.Patterns;

namespace Domain.Poker;

public class Table
{
    public SeatCollection Seats { get; }
    public CardDeck Deck { get; }
    public CardWaste CardWaste { get; }
    public Pot Pot { get; }
    public CommunityCards CommunityCards { get; }

    public Table(
        IEnumerable<Seat> seats,
        CardDeck deck,
        CardWaste cardWaste,
        Pot pot, 
        CommunityCards communityCards)
    {
        Seats = new SeatCollection(seats);
        Deck = deck;
        CardWaste = cardWaste;
        Pot = pot;
        CommunityCards = communityCards;

        //* Invariant checks
        if(Seats.Count < 2)
        {
            throw new InvalidOperationException("Table must have at least 2 seats.");
        }
    }

    public int AvailableSeatsCount => GetAvailableSeatCount();
    public int[] AvailableSeatIndexes => GetAvailableSeatIndexes();
    public SeatCollection PlayerSeats => GetPlayerSeats();
    public PlayerCollection Players => GetPlayers();
    public PlayerCollection ActivePlayers => GetActivePlayers();

    /* Factories */

    public static TableBuilder Create()
    {
        return new TableBuilder();
    }

    /* Queries */

    public bool IsPlayerSeated(Player player)
    {
        return Players.Contains(player);
    }

    public bool IsPlayerTurn(Player player)
    {
        return GetSeat(player).HasTurnButton;
    }

    public bool IsDealer(Player player)
    {
        return GetSeat(player).HasDealerButton;
    }

    public bool IsSmallBlind(Player player)
    {
        return GetSeat(player).HasSmallBlindButton;
    }

    public bool IsBigBlind(Player player)
    {
        return GetSeat(player).HasBigBlindButton;
    }

    /**/

    public Seat GetSeat(Player player)
    {
        return Seats
            .FirstOrDefault(seat => seat.Player?.Id == player.Id)
            ?? throw new InvalidOperationException("Player is not seated.");
    }

    public Seat GetNextSeat(Seat seat, Func<Seat, bool>? predicate = null)
    {
        var seatIndex = Seats.ToList().IndexOf(seat);
        var nextSeatIndex = seatIndex + 1;

        while (true)
        {
            if (nextSeatIndex >= Seats.Count)
            {
                nextSeatIndex = 0;
            }

            var nextSeat = Seats[nextSeatIndex];

            if (nextSeat == seat)
            {
                throw new InvalidOperationException("No other seats found.");
            }

            if (predicate is null || predicate(nextSeat))
            {
                return nextSeat;
            }

            nextSeatIndex++;
        }
    }

    public Seat GetNextSeat(Player player, Func<Seat, bool>? predicate = null)
    {
        return GetNextSeat(GetSeat(player), predicate);
    }

    public Seat? FindDealerSeat()
    {
        return Seats
            .FirstOrDefault(seat => seat.HasDealerButton);
    }

    public Seat GetDealerSeat()
    {
        return Seats
            .FirstOrDefault(seat => seat.HasDealerButton) 
            ?? throw new InvalidOperationException("No dealer found.");
    }

    /**/

    public Player? FindPlayer(Guid id)
    {
        return Players.FirstOrDefault(p => p.Id == id);
    }

    public Player GetTurnPlayer()
    {
        var players = Players
            .Where(p => GetSeat(p).HasTurnButton)
            .ToArray();

        if (players.Length == 0)
        {
            throw new InvalidOperationException("No player has the turn button.");
        }
        if (players.Length > 1)
        {
            throw new InvalidOperationException("More than one player has the turn button.");
        }

        return players[0];
    }

    public Player GetDealer()
    {
        var players = Players
            .Where(p => GetSeat(p).HasDealerButton)
            .ToArray();

        if (players.Length == 0)
        {
            throw new InvalidOperationException("No player has the dealer button.");
        }
        if (players.Length > 1)
        {
            throw new InvalidOperationException("More than one player has the dealer button.");
        }

        return players[0];
    }

    public Player GetSmallBlind()
    {
        var players = Players
            .Where(p => GetSeat(p).HasSmallBlindButton)
            .ToArray();

        if (players.Length == 0)
        {
            throw new InvalidOperationException("No player has the small blind button.");
        }
        if (players.Length > 1)
        {
            throw new InvalidOperationException("More than one player has the small blind button.");
        }

        return players[0];
    }

    public Player GetBigBlind()
    {
        var players = Players
            .Where(p => GetSeat(p).HasBigBlindButton)
            .ToArray();

        if (players.Length == 0)
        {
            throw new InvalidOperationException("No player has the big blind button.");
        }
        if (players.Length > 1)
        {
            throw new InvalidOperationException("More than one player has the big blind button.");
        }

        return players[0];
    }

    public Player GetNextPlayer(Seat seat, Func<Player, bool>? predicate = null)
    {
        var currentSeat = seat;

        while (true)
        {
            var nextSeat = GetNextSeat(currentSeat);

            if (nextSeat == seat)
            {
                throw new InvalidOperationException("No other players found.");
            }

            if (nextSeat.Player is not null && (predicate is null || predicate(nextSeat.Player)))
            {
                return nextSeat.Player;
            }

            currentSeat = nextSeat;
        }
    }

    public Player GetNextPlayer(Player player, Func<Player, bool>? predicate = null)
    {
        return GetNextPlayer(GetSeat(player), predicate);
    }

    /**/

    public decimal GetHighestBetAmount()
    {
        return Pot.MaxBetAmount;
    }

    public decimal GetPlayerBetAmount(Player player)
    {
        return Pot.GetPlayerBetAmount(player);
    }

    public decimal GetCallAmount(Player player)
    {
        var highestBet = GetHighestBetAmount();
        var playerBet = GetPlayerBetAmount(player);
        return highestBet - playerBet;
    }

    /* Mechanics */

    /* Sit player */

    internal void SitPlayer(int seatIndex, Player player)
    {
        if (!SeatExists(seatIndex))
        {
            throw new InvalidOperationException($"Seat {seatIndex} does not exist.");
        }
        if (!IsSeatAvailable(seatIndex))
        {
            throw new InvalidOperationException("Seat is already taken.");
        }
        if (IsPlayerSeated(player))
        {
            throw new InvalidOperationException("Player is already seated.");
        }

        Seats[seatIndex].AcceptPlayer(player);
    }

    private bool SeatExists(int seatIndex)
    {
        return seatIndex >= 0 && seatIndex < Seats.Count;
    }

    private bool IsSeatAvailable(int seatIndex)
    {
        if (seatIndex < 0 || seatIndex >= Seats.Count)
        {
            return false;
        }

        return Seats[seatIndex].IsNotTaken;
    }

    /* Stand player */

    internal void StandPlayer(Player player)
    {
        var seat = GetSeat(player);
        seat.RemovePlayer(player);
    }

    /* Assign buttons */

    internal void AssignDealerButtonTo(Player player)
    {
        AssignButton(player, ButtonType.Dealer);
    }

    internal void AssignSmallBlindButtonTo(Player player)
    {
        AssignButton(player, ButtonType.SmallBlind);
    }

    internal void AssignBigBlindButtonTo(Player player)
    {
        AssignButton(player, ButtonType.BigBlind);
    }

    internal void AssignTurnButtonTo(Player player)
    {
        AssignButton(player, ButtonType.Turn);
    }

    internal void AssignButton(Player player, ButtonType type)
    {
        GetSeat(player).AddButton(new Button(type));
    }

    /* Remove buttons */

    internal void RemoveAllButtons()
    {
        foreach (var seat in Seats)
        {
            seat.RemoveAllButtons();
        }
    }

    internal void RemoveButton(Player player, ButtonType type)
    {
        GetSeat(player)
            .RemoveButton(type);
    }

    internal void RemoveTurnButtonFrom(Player player)
    {
        GetSeat(player)
            .RemoveButton(ButtonType.Turn);
    }

    /* Betting */

    internal void PlaceBet(Player player, decimal amount)
    {
        Pot.PlaceBet(player, amount);
    }

    internal decimal CollectBets()
    {
        return Pot.CollectBets();
    }

    /* Private helpers */

    private int GetAvailableSeatCount()
    {
        return Seats
            .Count(seat => seat.IsNotTaken);
    }

    private int[] GetAvailableSeatIndexes()
    {
        return Seats
            .Select((seat, index) => (seat, index))
            .Where(x => x.seat.IsNotTaken)
            .Select(x => x.index)
            .ToArray();
    }

    private SeatCollection GetPlayerSeats()
    {
        return new SeatCollection(Seats.Where(seat => seat.Player is not null)); ;
    }

    private PlayerCollection GetPlayers()
    {
        var players = Seats
            .Where(seat => seat.Player is not null)
            .Select(seat => seat.Player!)
            .ToArray();
        return new PlayerCollection(players);
    }

    private PlayerCollection GetActivePlayers()
    {
        var players = Seats
            .Where(seat => seat.Player is not null && seat.Player.IsActive)
            .Select(seat => seat.Player!)
            .ToArray();
        return new PlayerCollection(players);
    }

}

public class TableBuilder : IBuilder<Table>
{
    private List<Seat> Seats { get; } = new();
    private CardDeck? Deck { get; set; } = null;
    private Pot? Pot { get; set; } = null;
    private CommunityCards? CommunityCards { get; set; } = null;

    public Table Build()
    {
        return new Table(
            seats: Seats,
            deck: Deck ?? throw new InvalidOperationException("Deck is required"),
            cardWaste: new CardWaste(cards: Array.Empty<Card>()),
            pot: Pot ?? new Pot(bets: new Dictionary<Guid, decimal>()),
            communityCards: CommunityCards ?? new CommunityCards(cards: Array.Empty<Card>())
        );
    }

    public TableBuilder WithSeats(int seatCount)
    {
        for (var i = 0; i < seatCount; i++)
        {
            Seats.Add(Seat.CreateEmpty());
        }

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
