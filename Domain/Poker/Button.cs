namespace CassinoDemo.Poker;

public enum ButtonType
{
    Turn,
    Dealer,
    SmallBlind,
    BigBlind,
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

