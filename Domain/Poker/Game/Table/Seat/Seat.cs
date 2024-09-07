using System.Collections;

namespace Domain.Poker;

public class Seat
{
    public Player? Player { get; private set; }
    public IReadOnlyList<Button> Buttons { get; }

    public Seat(
        Player? player,
        IEnumerable<Button> buttons)
    {
        Player = player;
        Buttons = new List<Button>(buttons);
    }

    public bool IsTaken => Player is not null;
    public bool IsNotTaken => !IsTaken;
    public bool HasDealerButton => HasButton(ButtonType.Dealer);
    public bool HasSmallBlindButton => HasButton(ButtonType.SmallBlind);
    public bool HasBigBlindButton => HasButton(ButtonType.BigBlind);
    public bool HasTurnButton => HasButton(ButtonType.Turn);

    internal List<Button> ButtonsList => (List<Button>)Buttons;

    /* Factories */

    public static Seat CreateEmpty()
    {
        return new Seat(
            player: null,
            buttons: new List<Button>()
        );
    }

    public override string ToString()
    {
        return $"Seat: {Player?.Name ?? "Empty"}";
    }

    /* Queries */

    public bool HasButton(ButtonType type)
    {
        return Buttons
            .Any(x => x.Type == type);
    }

    /* Commands */

    public void AcceptPlayer(Player player)
    {
        if (IsTaken)
        {
            throw new InvalidOperationException("Seat is already taken.");
        }

        Player = player;
    }

    public void RemovePlayer(Player player)
    {
        if (Player is null)
        {
            throw new InvalidOperationException("Seat is already empty.");
        }
        if (Player.Id != player.Id)
        {
            throw new InvalidOperationException("Player is not seated at this seat.");
        }

        Player = null;
    }

    /* Buttons */

    public void AddButton(Button button)
    {
        if (ButtonsList.Any(x => x.Type == button.Type))
        {
            throw new InvalidOperationException("This button is already assigned to the player");
        }

        ButtonsList.Add(button);
    }

    public void AddButtons(IEnumerable<Button> buttons)
    {
        foreach (var button in buttons)
        {
            AddButton(button);
        }
    }

    public void RemoveButton(ButtonType type)
    {
        var buttons = Buttons
            .Where(x => x.Type != type)
            .ToArray();

        ButtonsList.Clear();
        ButtonsList.AddRange(buttons);
    }

    public IReadOnlyList<Button> RemoveAllButtons()
    {
        var buttons = new List<Button>(ButtonsList);
        ButtonsList.Clear();
        return buttons;
    }


}

public class SeatCollection : IReadOnlyList<Seat>
{
    private List<Seat> Seats { get; }

    public SeatCollection(IEnumerable<Seat> seats)
    {
        Seats = new List<Seat>(seats);
    }

    public int Count => Seats.Count;

    public Seat this[int index]
    {
        get => Seats[index];
    }

    public IEnumerator<Seat> GetEnumerator()
    {
        return Seats.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int GetSeatIndex(Seat seat)
    {
        return Seats.IndexOf(seat);
    }

    public Seat? FindDealerSeat()
    {
        return Seats
            .FirstOrDefault(x => x.HasDealerButton);
    }

    public Seat? FindSmallBlindSeat()
    {
        return Seats
            .FirstOrDefault(x => x.HasSmallBlindButton);
    }

    public Seat? FindBigBlindSeat()
    {
        return Seats
            .FirstOrDefault(x => x.HasBigBlindButton);
    }

    public Seat GetNextSeat(Seat seat)
    {
        var seatIndex = GetSeatIndex(seat);
        var nextSeatIndex = seatIndex + 1;

        if (nextSeatIndex >= Seats.Count)
        {
            nextSeatIndex = 0;
        }

        return Seats[nextSeatIndex];
    }

    public Seat GetSeatToTheLeftOf(Seat seat)
    {
        if(Seats.Count < 2)
        {
            throw new InvalidOperationException("There are not enough seats to rotate.");
        }

        var seatIndex = GetSeatIndex(seat);
        var nextSeatIndex = seatIndex + 1;

        if (nextSeatIndex >= Seats.Count)
        {
            nextSeatIndex = 0;
        }

        return Seats[nextSeatIndex];
    }

}