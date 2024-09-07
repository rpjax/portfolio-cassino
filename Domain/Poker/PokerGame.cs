using Aidan.Core.Patterns;

namespace Domain.Poker;

public class PokerGame
{
    public Guid Id { get; }
    public Table Table { get; }
    public Dealer Dealer { get; }
        
    public PokerGame(
        Guid id,
        Table table,
        Dealer dealer)
    {
        Id = id;
        Table = table;
        Dealer = dealer;
    }

    public IReadOnlyList<Player> Players => Table.Players;

    public static PokerGameBuilder Create()
    {
        return new PokerGameBuilder();
    }

    public bool IsPlayerSeated(Guid playerId)
    {
        var player = FindPlayer(playerId);

        if(player is null)
        {
            return false;
        }

        return Table.IsPlayerSeated(player);
    }

    /*
     * Behavior
     */

    public void EndGame()
    {

    }

    public void StartRound()
    {
        Dealer.StartRound(Table);
    }

    public void AbortRound()
    {
        Dealer.AbortRound(Table);
    }

    /* Player actions */

    public void Join(Guid id, int seat, string name, decimal bankroll = 0)
    {
        var player = Player.CreateBuilder()
            .WithId(id)
            .WithName(name)
            .WithBankrollBalance(bankroll)
            .Build();

        Dealer.AcceptPlayerJoin(player, Table);
        Table.SitPlayer(seat, player);
    }

    public QuitResult Quit(Guid playerId)
    {
        var player = GetPlayer(playerId);

        Dealer.AcceptPlayerQuit(player, Table);
        Table.StandPlayer(player);

        return QuitResult.Create()
            .WithPlayer(player)
            .WithBankrollBalance(player.BankrollBalance)
            .Build();
    }

    public void Rebuy(Guid playerId, decimal amount)
    {
        Dealer.AcceptPlayerRebuy(GetPlayer(playerId), Table, amount);
    }

    /* Player betting actions */

    public void Check(Guid playerId)
    {
        Dealer.AcceptPlayerCheck(GetPlayer(playerId), Table);
    }

    public void Bet(Guid playerId, decimal amount)
    {
        Dealer.AcceptPlayerBet(GetPlayer(playerId), Table, amount);
    }

    public void Fold(Guid playerId)
    {
        Dealer.AcceptPlayerFold(GetPlayer(playerId), Table);
    }

    public void Call(Guid playerId)
    {
        Dealer.AcceptPlayerCall(GetPlayer(playerId), Table);
    }

    public void Raise(Guid playerId, decimal amount)
    {
        Dealer.AcceptPlayerRaise(GetPlayer(playerId), Table, amount);
    }

    /* Helpers */

    public Player? FindPlayer(Guid playerId)
    {
        return Table.FindPlayer(playerId);
    }

    public Player GetPlayer(Guid playerId)
    {
        var player = Table.FindPlayer(playerId);

        if(player is null)
        {
            throw new InvalidOperationException("Player not found");
        }

        return player;
    }
        
}

public class PokerGameBuilder : IBuilder<PokerGame>
{
    private Guid Id { get; set; }
    private Table? Table { get; set; }
    private Dealer? Dealer { get; set; }

    public PokerGameBuilder()
    {
        Id = Guid.NewGuid();
    }

    public PokerGameBuilder WithId(Guid id)
    {
        Id = id;
        return this;
    }

    public PokerGameBuilder WithTable(Table table)
    {
        Table = table;
        return this;
    }

    public PokerGameBuilder WithDealer(Dealer dealer)
    {
        Dealer = dealer;
        return this;
    }

    public PokerGame Build()
    {
        if (Id == Guid.Empty)
        {
            throw new InvalidOperationException("Id is required");
        }

        if (Table is null)
        {
            throw new InvalidOperationException("Table is required");
        }

        if (Dealer is null)
        {
            throw new InvalidOperationException("Dealer is required");
        }

        return new PokerGame(Id, Table, Dealer);
    }

}