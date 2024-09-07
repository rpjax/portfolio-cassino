namespace Application.Infra.Database.Poker;

public class PokerGameDbModel
{
    public long Id { get; set; }
    public Guid DomainId { get; set; }
    public TableDbModel Table { get; set; } = null!;
    public DealerDbModel Dealer { get; set; } = null!;
}

public class TableDbModel
{
    public long Id { get; set; }
    public long GameId { get; set; }
}

public class DealerDbModel
{
    public long Id { get; set; }
    public long GameId { get; set; }
    public RulesDbModel Rules { get; set; } = null!;
}

public class RulesDbModel
{
    public long Id { get; set; }
    public long DealerId { get; set; }

    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }

    public decimal SmallBlindAmount { get; set; }
    public decimal BigBlindAmount { get; set; }
}