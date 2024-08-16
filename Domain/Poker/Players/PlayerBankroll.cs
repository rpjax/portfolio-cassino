namespace CassinoDemo.Poker;

public class PlayerBankroll
{
    public decimal Balance { get; private set; }

    public PlayerBankroll(decimal bankroll)
    {
        Balance = bankroll;
    }

    public override string ToString()
    {
        return $"${Balance}";
    }

    public void Credit(decimal amount)
    {
        Balance += amount;
    }

    public void Debit(decimal amount)
    {
        if(amount <= 0)
        {
            throw new ArgumentException("Invalid debit amount");
        }
        if(amount > Balance)
        {
            throw new InvalidOperationException("Insufficient funds");
        }

        Balance -= amount;
    }
}
