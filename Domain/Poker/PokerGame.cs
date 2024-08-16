namespace CassinoDemo.Poker;

public enum GameLifeCycleStage
{
    NotStarted,
    InProgress,
    Ended
}

public class PokerGame
{
    public Guid Id { get; }

    private Table Table { get; }
    private Dealer Dealer { get; }
        
    public PokerGame(
        Guid id,
        Table table,
        Dealer dealer)
    {
        Id = id;
        Table = table;
        Dealer = dealer;
    }

    public IReadOnlyList<Player> Players => Table.Seats;

    /*
     * Behavior
     */

    public void EndGame()
    {

    }

    public void AddPlayer(Guid id, string name, decimal bankroll = 0)
    {
        var player = Player.CreateBuilder()
            .WithId(id)
            .WithName(name)
            .WithBankrollBalance(bankroll)
            .Build();

        Dealer.AddPlayer(player, Table);
    }

    public void RemovePlayer(Guid playerId)
    {
        Dealer.RemovePlayer(GetPlayer(playerId), Table);
    }

    public void StartRound()
    {
        Dealer.StartRound(Table);
    }

    public void EndRound()
    {

    }

    /*
     * Player actions
     */

    public void Check(Guid playerId)
    {
        Dealer.AcceptPlayerCheck(GetPlayer(playerId), Table);
    }

    public void Call(Guid playerId)
    {
        Dealer.AcceptPlayerCall(GetPlayer(playerId), Table);
    }

    /*
     * Private helper methods
     */

    private Player GetPlayer(Guid playerId)
    {
        var player = Players.FirstOrDefault(p => p.Id == playerId);

        if(player is null)
        {
            throw new InvalidOperationException("Player not found");
        }

        return player;
    }
        
}
