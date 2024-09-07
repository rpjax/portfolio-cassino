namespace Domain.Poker;

public enum ButtonType
{
    Dealer,
    SmallBlind,
    BigBlind,
    Turn
}

public struct Button : IValueObject
{
    public ButtonType Type { get; }

    public Button(ButtonType type)
    {
        Type = type;
    }

    public override string ToString()
    {
        return Type.ToString();
    }
}

