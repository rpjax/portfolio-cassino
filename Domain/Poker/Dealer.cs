using Aidan.Core.Extensions;
using Aidan.Core.Patterns;

namespace CassinoDemo.Poker;

public class Dealer
{
    internal GameRules Rules { get; }

    public Dealer(GameRules rules)
    {
        Rules = rules;
    }

    private decimal MinBet => Rules.SmallBlindValue * 2;

    /*
     * Factories
     */

    public static DealerBuilder Create()
    {
        return new DealerBuilder();
    }

    /*
     * Behaviour
     */

    public void AddPlayer(Player player, Table table)
    {
        if (table.Players.Count >= Rules.MaxPlayers)
        {
            throw new InvalidOperationException("Max players reached");
        }
        if (table.Players.Any(p => p.Id == player.Id))
        {
            throw new InvalidOperationException("Player is already at the table");
        }

        table.Seats.AddPlayer(player);
    }

    public void RemovePlayer(Player player, Table table)
    {
        table.CardWaste.AddCards(player.RemoveAllCards());
        table.Seats.RemovePlayer(player);
    }

    public PlayerCollection GetEligiblePlayers(Table table)
    {
        var players = table.Players
            .Where(player => player.Bankroll.Balance > Rules.SmallBlindValue)
            .ToArray();

        return new PlayerCollection(players);
    }

    public Round GetRound(Table table)
    {
        var players = table.Players
            .Where(player => player.IsPlaying)
            .ToArray();

        return Round.Create()
            .WithPlayers(players)
            .Build();
    }

    /*
     * Round progression
     */

    public void StartRound(Table table)
    {
        AssignButtons(table);
        ShuffleCards(table);
        PlaceInitialBets(table);
        DealInitialHand(table);
    }

    public void DealFlop(Table table)
    {
        var seats = table.Seats;
        var deck = table.Deck;
        var communityCards = table.CommunityCards;

        var bigBlind = seats.GetBigBlind();
        var turnPlayer = seats.GetPlayerToTheLeftOf(bigBlind);

        for (var i = 0; i < 3; i++)
        {
            communityCards.AddCard(deck.DrawCard());
        }
    }

    /*
     * player actions
     */

    public void AcceptPlayerCheck(Player player, Table table)
    {
        EnsurePlayerTurn(player);
        ValidateCheck(player, GetRound(table));
        PassPlayerTurn(player, GetRound(table));
    }

    public void AcceptPlayerCall(Player player, Table table)
    {
        EnsurePlayerTurn(player);
        ValidateCall(player, GetRound(table));
        player.Call(amount: GetRound(table).GetHighestBetAmount());
        PassPlayerTurn(player, GetRound(table));
    }

    public void AcceptPlayerRaise(Player player, Table table, decimal amount)
    {
        EnsurePlayerTurn(player);
        ValidateRaise(player, GetRound(table), amount);
        player.Raise(amount);
        PassPlayerTurn(player, GetRound(table));
    }

    public void AcceptPlayerFold(Player player, Table table)
    {
        EnsurePlayerTurn(player);
        player.Fold();
        PassPlayerTurn(player, GetRound(table));
    }

    public void AcceptPlayerAllIn(Player player, Table table)
    {
        EnsurePlayerTurn(player);
        player.AllIn();
        PassPlayerTurn(player, GetRound(table));
    }

    /*
     * Round start helpers
     */

    private void AssignButtons(Table table)
    {
        var players = GetEligiblePlayers(table);
        var dealer = AssignDealer(players);
        var smallBlind = AssignSmallBlind(players);
        var bigBlind = AssignBigBlind(players);
        var turnPlayer = players.GetPlayerToTheLeftOf(bigBlind);

        foreach (var player in players)
        {
            player.RemoveAllButtons();
        }

        dealer.AddButton(type: ButtonType.Dealer);
        smallBlind.AddButton(type: ButtonType.SmallBlind);
        bigBlind.AddButton(type: ButtonType.BigBlind);
        turnPlayer.AddButton(type: ButtonType.Turn);
    }

    private int GetDealerIndex(PlayerCollection players)
    {
        var index = players.GetDealerIndex(defaultValue: 0);

        if (index == 0)
        {
            return 0;
        }

        index++;

        if (index >= players.Count)
        {
            index = 0;
        }

        return index;
    }

    private int GetSmallBlindIndex(PlayerCollection players)
    {
        var index = GetDealerIndex(players) + 1;

        if (index >= players.Count)
        {
            index = 0;
        }

        return index;
    }

    private int GetBigBlindIndex(PlayerCollection players)
    {
        var index = GetSmallBlindIndex(players) + 1;

        if (index >= players.Count)
        {
            index = 0;
        }

        return index;
    }

    private Player AssignDealer(PlayerCollection players)
    {
        return players[GetDealerIndex(players)];
    }

    private Player AssignSmallBlind(PlayerCollection players)
    {
        return players[GetSmallBlindIndex(players)];
    }

    private Player AssignBigBlind(PlayerCollection players)
    {
        return players[GetBigBlindIndex(players)];
    }

    private void ShuffleCards(Table table)
    {
        var deck = table.Deck;
        var cardWaste = table.CardWaste;
        var strategy = Rules.ShufflingStrategy;

        deck.AddCards(cardWaste.RemoveAllCards());
        deck.Shuffle(strategy);
    }

    private void PlaceInitialBets(Table table)
    {
        var players = GetEligiblePlayers(table);
        var smallBlind = players.GetSmallBlind();
        var bigBlind = players.GetBigBlind();

        smallBlind.PlaceSmallBlindBet(Rules.SmallBlindValue);
        bigBlind.PlaceBigBlindBet(Rules.SmallBlindValue * 2);
    }

    private void DealInitialHand(Table table)
    {
        var deck = table.Deck;

        foreach (var player in GetEligiblePlayers(table))
        {
            for (var i = 0; i < 2; i++)
            {
                player.TakeCard(deck.DrawCard());
            }
        }
    }

    /*
     * Player actions validation
     */

    private void ValidateCheck(Player player, Round players)
    {
        var playerBetAmount = player.Bet?.Amount ?? 0;
        var highestBet = players.GetHighestBetAmount();

        if (playerBetAmount < highestBet)
        {
            throw new InvalidOperationException("Player must match the highest bet");
        }
    }

    private void ValidateCall(Player player, Round players)
    {
        var highestBet = players.GetHighestBet();

        if (highestBet is null)
        {
            throw new InvalidOperationException("No bets to call");
        }

        var playerBetAmount = player.Bet?.Amount ?? 0m;
        var callAmount = highestBet.Amount - playerBetAmount;

        if (highestBet is null)
        {
            throw new InvalidOperationException("No bets to call");
        }
        if (highestBet.PlayerId == player.Id)
        {
            throw new InvalidOperationException("Player is already the highest bet");
        }
        if (callAmount > player.Bankroll.Balance)
        {
            throw new InvalidOperationException("Player does not have enough funds to call");
        }
    }

    private void ValidateRaise(Player player, Round players, decimal amount)
    {
        var highestBet = players.GetHighestBetAmount();

        if (amount < MinBet)
        {
            throw new InvalidOperationException("Bet is less than the minimum bet");
        }

        if (amount < highestBet)
        {
            throw new InvalidOperationException("Bet is less than the highest bet");
        }
    }

    /*
     * Helpers
     */

    private void EnsurePlayerTurn(Player player)
    {
        if (!player.HasTurnButton)
        {
            throw new InvalidOperationException("It's not the player's turn");
        }
    }

    private void PassPlayerTurn(Player player, Round players)
    {
        var playerIndex = players.ToList().IndexOf(player);
        var nextPlayerIndex = (playerIndex + 1);
        if (nextPlayerIndex >= players.Count)
        {
            nextPlayerIndex = 0;
        }

        var nextPlayer = players[nextPlayerIndex];

        player.RemoveButton(ButtonType.Turn);

        if (nextPlayer.IsSmallBlind)
        {
            return;
        }

        nextPlayer.AddButton(ButtonType.Turn);
    }

}

public class DealerBuilder : IBuilder<Dealer>
{
    private GameRules? Rules { get; set; } = null;

    public Dealer Build()
    {
        return new Dealer(
            rules: Rules ?? throw new InvalidOperationException("Rules are required")
        );
    }

    public DealerBuilder WithRules(GameRules rules)
    {
        Rules = rules;
        return this;
    }

}
