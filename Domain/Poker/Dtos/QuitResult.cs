using Aidan.Core.Patterns;

namespace Domain.Poker;

public class QuitResult
{
    public Player Player { get; }
    public decimal BankrollBalance { get; }

    public QuitResult(Player player, decimal bankrollBalance)
    {
        Player = player;
        BankrollBalance = bankrollBalance;
    }

    public static QuitResultBuilder Create()
    {
        return new QuitResultBuilder();
    }

    public override string ToString()
    {
        return $"Player: {Player.Name}, Bankroll: {BankrollBalance}";
    }

}

public class QuitResultBuilder : IBuilder<QuitResult>
{
    private Player? Player { get; set; }
    private decimal? BankrollBalance { get; set; }

    public QuitResultBuilder WithPlayer(Player player)
    {
        Player = player;
        return this;
    }

    public QuitResultBuilder WithBankrollBalance(decimal bankrollBalance)
    {
        BankrollBalance = bankrollBalance;
        return this;
    }

    public QuitResult Build()
    {
        return new QuitResult(
            Player ?? throw new InvalidOperationException("Player is required"),
            BankrollBalance ?? throw new InvalidOperationException("Bankroll balance is required")
        );
    }
}