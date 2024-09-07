using Aidan.Core.Linq;
using Aidan.Core.Linq.Extensions;
using Domain.Core.Services;
using Domain.Poker;
using System.Linq.Expressions;

namespace Application.Infra.Database.Poker;

public class PokerGameRepository : IRepositoryService<PokerGame>
{
    private IRepositoryService<PokerGameDbModel> DbModelRepository { get; }

    public PokerGameRepository(IRepositoryService<PokerGameDbModel> repository)
    {
        DbModelRepository = repository;
    }

    public PokerGameRepository(IRepositoryProvider repositoryProvider)
    {
        DbModelRepository = repositoryProvider.GetRepository<PokerGameDbModel>();
    }

    public IAsyncQueryable<PokerGame> AsQueryable()
    {
        return DbModelRepository.AsQueryable()
            .Select(GetDbModelToEntityProjection());
    }

    public Task CreateAsync(PokerGame entity)
    {
        return DbModelRepository.CreateAsync(MapToDbModel(entity));
    }

    public Task UpdateAsync(PokerGame entity)
    {
        return DbModelRepository.UpdateAsync(MapToDbModel(entity));
    }

    public Task DeleteAsync(PokerGame entity)
    {
        return DbModelRepository.DeleteAsync(MapToDbModel(entity));
    }

    private static Expression<Func<PokerGameDbModel, PokerGame>> GetDbModelToEntityProjection()
    {
        return x => new PokerGame(
            x.DomainId,
            new Table(
                new List<Seat>(),
                new CardDeck(
                    new List<Card>()
                ),
                new CardWaste(
                    new List<Card>()
                ),
                new Pot(
                    new Dictionary<Guid, decimal>()
                ),
                new CommunityCards(
                    new List<Card>()
                )
            ),
            new Dealer(
                new PokerRules(
                    x.Dealer.Rules.MinPlayers,
                    x.Dealer.Rules.MaxPlayers,
                    x.Dealer.Rules.SmallBlindAmount,
                    x.Dealer.Rules.BigBlindAmount,
                    new RandomShufflingStrategy(),
                    false
                ),
                new DealerMemory(
                    new List<Guid>()
                ),
                null
            )
        );
    }

    private static PokerGameDbModel MapToDbModel(PokerGame entity)
    {
        return new PokerGameDbModel
        {
            DomainId = entity.Id,
            Table = new TableDbModel(),
            Dealer = new DealerDbModel
            {
                Rules = new RulesDbModel
                {
                    MinPlayers = entity.Dealer.Rules.MinPlayers,
                    MaxPlayers = entity.Dealer.Rules.MaxPlayers,
                    SmallBlindAmount = entity.Dealer.Rules.SmallBlindAmount,
                    BigBlindAmount = entity.Dealer.Rules.BigBlindAmount
                }
            }
        };
    }
}
