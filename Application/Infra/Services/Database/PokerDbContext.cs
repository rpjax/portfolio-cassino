using Aidan.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Application.Infra.Database.Poker;

public class PokerDbContext : EFCoreSqliteContext
{
    public PokerDbContext(FileInfo fileInfo)
        : base(fileInfo)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurePokerGame(modelBuilder);
        ConfigureTable(modelBuilder);
        ConfigureDealer(modelBuilder);
    }

    private void ConfigurePokerGame(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PokerGameDbModel>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DomainId).IsRequired();

            entity
                .HasOne(x => x.Table)
                .WithOne()
                .HasForeignKey<TableDbModel>(x => x.GameId)
                .IsRequired()
                ;

            entity
                .HasOne(x => x.Dealer)
                .WithOne()
                .HasForeignKey<DealerDbModel>(x => x.GameId)
                .IsRequired()
                ;
        });
    }

    private void ConfigureTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TableDbModel>(entity =>
        {
            entity.HasKey(x => x.Id);
        });
    }

    private void ConfigureDealer(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DealerDbModel>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity
                .HasOne(x => x.Rules)
                .WithOne()
                .HasForeignKey<RulesDbModel>(x => x.DealerId)
                .IsRequired()
                ;
        });
    }
}