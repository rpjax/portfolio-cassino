using Aidan.Core.Extensions;
using Aidan.Core.Patterns;

namespace Domain.Poker;

public class Dealer
{
    public PokerRules Rules { get; }
    public DealerMemory Memory { get; }
    public IPokerLog? GameLog { get; }

    public Dealer(
        PokerRules rules,
        DealerMemory memory,
        IPokerLog? gameLog)
    {
        Rules = rules;
        Memory = memory;
        GameLog = gameLog;
    }

    private decimal MinBet => Rules.BigBlindAmount;

    /* Factories */

    public static DealerBuilder Create()
    {
        return new DealerBuilder();
    }

    /*
     * Commands
     */

    /* Round start  */

    internal void StartRound(Table table)
    {
        if (!IsRoundStartable(table))
        {
            throw new InvalidOperationException("Not enough players to start the round.");
        }

        var players = GetEligiblePlayers(table);

        RotateButtons(table, players);
        PlaceInitialBets(table);
        DealHands(table, players);
        StartInitialRoundOfBetting(table, players);
    }

    private bool IsRoundStartable(Table table)
    {
        var eligiblePlayersCount = GetEligiblePlayers(table).Count;
        return eligiblePlayersCount >= Rules.MinPlayers;
    }

    private void RotateButtons(Table table, PlayerCollection players)
    {
        var dealer = AssignDealer(table, players);
        var smallBlind = AssignSmallBlind(table, players);
        var bigBlind = AssignBigBlind(table, players);

        table.RemoveAllButtons();
        table.AssignDealerButtonTo(dealer);
        table.AssignSmallBlindButtonTo(smallBlind);
        table.AssignBigBlindButtonTo(bigBlind);
    }

    private Player AssignDealer(Table table, PlayerCollection players)
    {
        var dealerSeat = table.FindDealerSeat();

        if (dealerSeat == null)
        {
            return players.First();
        }

        var dealer = table.GetNextPlayer(dealerSeat, player =>
            players.Contains(player));

        return dealer;
    }

    private Player AssignSmallBlind(Table table, PlayerCollection players)
    {
        return GetEligiblePlayers(table)
            .GetPlayerToTheLeftOf(AssignDealer(table, players));
    }

    private Player AssignBigBlind(Table table, PlayerCollection players)
    {
        return GetEligiblePlayers(table)
            .GetPlayerToTheLeftOf(AssignSmallBlind(table, players));
    }

    private void PlaceInitialBets(Table table)
    {
        var smallBlind = table.GetSmallBlind();
        var bigBlind = table.GetBigBlind();

        smallBlind.Bet(Rules.SmallBlindAmount);
        bigBlind.Bet(Rules.BigBlindAmount);

        table.PlaceBet(smallBlind, Rules.SmallBlindAmount);
        table.PlaceBet(bigBlind, Rules.BigBlindAmount);
    }

    private void DealHands(Table table, PlayerCollection players)
    {
        var deck = table.Deck;

        ShuffleCards(table);

        foreach (var player in players)
        {
            for (var i = 0; i < 2; i++)
            {
                player.TakeCard(deck.DrawCard());
            }
        }
    }

    private void ShuffleCards(Table table)
    {
        var deck = table.Deck;
        var cardWaste = table.CardWaste;
        var strategy = Rules.ShufflingStrategy;

        deck.AddCards(cardWaste.RemoveAllCards());
        deck.Shuffle(strategy);
    }

    private void StartInitialRoundOfBetting(Table table, PlayerCollection players)
    {
        var bigBlind = table.GetBigBlind();
        var turnPlayer = players.GetPlayerToTheLeftOf(bigBlind);

        table.AssignTurnButtonTo(turnPlayer);
        Memory.StartRound();
    }

    /* Round end */

    internal void AbortRound(Table table)
    {
        CollectWinnings(table);
        CollectAllCards(table);
    }

    private void CollectWinnings(Table table)
    {
        var players = table.ActivePlayers;
        var potAmount = table.CollectBets();
        var winningPlayers = DetermineWinningPlayers(table);
        var winningsPerPlayer = potAmount / winningPlayers.Length;

        foreach (var player in winningPlayers)
        {
            player.CollectWinnings(winningsPerPlayer);
        }
    }

    private Player[] DetermineWinningPlayers(Table table)
    {
        var strengthGroups = table.ActivePlayers
            .GroupBy(player => DetermineHandStrength(player, table))
            .OrderByDescending(group => group.Key)
            .ToArray();

        var winningPlayers = strengthGroups
            .First()
            .ToArray();

        return winningPlayers;
    }

    private int DetermineHandStrength(Player player, Table table)
    {
        if (player.Hand == null)
        {
            throw new InvalidOperationException("Player must have a hand.");
        }
        if (player.Hand.Count != 2)
        {
            throw new InvalidOperationException("Player must have 2 cards in hand.");
        }

        var hand = player.Hand;
        var communityCards = table.CommunityCards;

        var allCards = hand
            .Concat(communityCards)
            .ToArray();

        var calculator = new HandStrengthCalculator(allCards);

        return calculator.ComputeStrength();
    }

    private void CollectAllCards(Table table)
    {
        var cardWaste = table.CardWaste;
        var players = table.Players;
        var communityCards = table.CommunityCards;

        foreach (var player in players)
        {
            cardWaste.AddCards(player.RemoveAllCards());
        }

        cardWaste.AddCards(communityCards.RemoveAllCards());
    }

    /* Player actions */

    /* Join */

    internal void AcceptPlayerJoin(Player player, Table table)
    {
        if (player.BankrollBalance < MinBet)
        {
            throw new InvalidOperationException("Player does not have enough funds to join the table.");
        }
    }

    /* Quit */

    internal void AcceptPlayerQuit(Player player, Table table)
    {
        RemovePlayerCards(player, table);
        PassTurn(player, table);
    }

    private void RemovePlayerCards(Player player, Table table)
    {
        table.CardWaste.AddCards(player.RemoveAllCards());
    }

    private void PassTurn(Player player, Table table)
    {
        if (table.IsPlayerTurn(player))
        {
            OnTurnTaken(player, table);
        }
    }

    /* Rebuy */

    internal void AcceptPlayerRebuy(Player player, Table table, decimal amount)
    {
        ValidatePlayerRebuy(player, table, amount);
        player.Rebuy(amount);
    }

    private void ValidatePlayerRebuy(Player player, Table table, decimal amount)
    {
        if (player.IsActive)
        {
            throw new InvalidOperationException("Cannot rebuy while active in the round.");
        }
    }

    /* Betting actions */

    /* Check */

    internal void AcceptPlayerCheck(Player player, Table table)
    {
        EnsurePlayerTurn(player, table);
        ValidateCheck(player, table);
        OnTurnTaken(player, table);
    }

    private void ValidateCheck(Player player, Table table)
    {
        var highestBet = table.GetHighestBetAmount();
        var playerBetAmount = table.GetPlayerBetAmount(player);

        if (playerBetAmount < highestBet)
        {
            throw new InvalidOperationException($"Player {player.Name} must call or raise to check. Highest bet is {highestBet}");
        }
    }

    /* Bet */

    internal void AcceptPlayerBet(Player player, Table table, decimal amount)
    {
        EnsurePlayerTurn(player, table);
        ValidateBet(player, table, amount);
        player.Bet(amount);
        table.PlaceBet(player, amount);
        OnTurnTaken(player, table);
    }

    private void ValidateBet(Player player, Table table, decimal amount)
    {
        var highestBet = table.GetHighestBetAmount();
        var playerBetAmount = table.GetPlayerBetAmount(player);
        var totalBet = playerBetAmount + amount;

        if (totalBet < MinBet)
        {
            throw new InvalidOperationException("Bet is less than the minimum bet");
        }
        if (totalBet < highestBet)
        {
            throw new InvalidOperationException("Bet is less than the highest bet");
        }
    }

    /* Fold */

    internal void AcceptPlayerFold(Player player, Table table)
    {
        EnsurePlayerTurn(player, table);
        ValidateFold(player, table);
        player.Fold();
        OnTurnTaken(player, table);
    }

    private void ValidateFold(Player player, Table table)
    {

    }

    /* Call */

    internal void AcceptPlayerCall(Player player, Table table)
    {
        var callAmount = table.GetCallAmount(player);

        EnsurePlayerTurn(player, table);
        ValidateCall(player, table);
        player.Call(callAmount);
        table.PlaceBet(player, callAmount);
        OnTurnTaken(player, table);
    }

    private void ValidateCall(Player player, Table table)
    {
        var highestBet = table.GetHighestBetAmount();
        var playerBet = table.GetPlayerBetAmount(player);
        var callAmount = highestBet - playerBet;

        if (callAmount == 0)
        {
            throw new InvalidOperationException("No bet to call. Check instead.");
        }
    }

    /* Raise */

    internal void AcceptPlayerRaise(Player player, Table table, decimal amount)
    {
        EnsurePlayerTurn(player, table);
        ValidateRaise(player, table, amount);
        player.Raise(amount);
        table.PlaceBet(player, amount);
        OnTurnTaken(player, table);
    }

    private void ValidateRaise(Player player, Table table, decimal amount)
    {
        var highestBet = table.GetHighestBetAmount();
        var currentBet = table.GetPlayerBetAmount(player);
        var totalBet = currentBet + amount;

        if (totalBet < MinBet)
        {
            throw new InvalidOperationException("Bet is less than the minimum bet");
        }
        if (totalBet < highestBet)
        {
            throw new InvalidOperationException("Bet is less than the highest bet");
        }
    }

    /* On-Turn-Taken Hook */

    private void OnTurnTaken(Player player, Table table)
    {
        EndTurn(player, table);

        if (!IsCurrentRoundPlayable(table))
        {
            AbortRound(table);
            return;
        }

        if (IsRoundOfBettingOver(table))
        {
            TransitionRound(table);
        }
        else
        {
            PassTurnToNextActivePlayer(player, table);
        }
    }

    private void EndTurn(Player player, Table table)
    {
        Memory.SaveTurn(player);
        table.RemoveButton(player, ButtonType.Turn);
    }

    private bool IsCurrentRoundPlayable(Table table)
    {
        return table.ActivePlayers.Count >= Rules.MinPlayers;
    }

    private bool IsRoundOfBettingOver(Table table)
    {
        var activePlayers = table.ActivePlayers;
        var highestBet = table.GetHighestBetAmount();

        var haveAllPlayersPlayed = activePlayers
            .All(player => Memory.HasPlayed(player));

        var haveAllPlayersCalled = activePlayers
            .All(player => table.GetPlayerBetAmount(player) == highestBet);

        return haveAllPlayersPlayed && haveAllPlayersCalled;
    }

    private void TransitionRound(Table table)
    {
        switch (DetermineRoundPhase(table))
        {
            case RoundPhase.PreFlop:
                DealFlop(table);
                break;

            case RoundPhase.Flop:
                DealTurn(table);
                break;

            case RoundPhase.Turn:
                DealRiver(table);
                break;

            case RoundPhase.River:
                EndRound(table);
                break;

            default:
                throw new InvalidOperationException("Invalid round phase.");
        }
    }

    private RoundPhase DetermineRoundPhase(Table table)
    {
        var communityCards = table.CommunityCards;

        if (communityCards.Count == 0)
        {
            return RoundPhase.PreFlop;
        }
        if (communityCards.Count == 3)
        {
            return RoundPhase.Flop;
        }
        if (communityCards.Count == 4)
        {
            return RoundPhase.Turn;
        }
        if (communityCards.Count == 5)
        {
            return RoundPhase.River;
        }

        throw new InvalidOperationException("Invalid round phase");
    }

    private void DealFlop(Table table)
    {
        var deck = table.Deck;
        var communityCards = table.CommunityCards;

        if (communityCards.Count != 0)
        {
            throw new InvalidOperationException("Flop has already been dealt");
        }

        for (var i = 0; i < 3; i++)
        {
            communityCards.AddCard(deck.DrawCard());
        }

        StartRoundOfBetting(table);
    }

    private void DealTurn(Table table)
    {
        var deck = table.Deck;
        var communityCards = table.CommunityCards;

        if (communityCards.Count != 3)
        {
            throw new InvalidOperationException("Flop must be dealt first");
        }

        communityCards.AddCard(deck.DrawCard());
        StartRoundOfBetting(table);
    }

    private void DealRiver(Table table)
    {
        var deck = table.Deck;
        var communityCards = table.CommunityCards;

        if (communityCards.Count != 4)
        {
            throw new InvalidOperationException("Turn must be dealt first");
        }

        communityCards.AddCard(deck.DrawCard());
        StartRoundOfBetting(table);
    }

    private void StartRoundOfBetting(Table table)
    {
        var dealerSeat = table.GetDealerSeat();

        var turnPlayer = table.GetNextPlayer(dealerSeat, player =>
            player.IsActive);

        table.AssignTurnButtonTo(turnPlayer);
        Memory.StartRound();
    }

    private void EndRound(Table table)
    {
        CollectWinnings(table);
        CollectAllCards(table);
    }

    private void PassTurnToNextActivePlayer(Player player, Table table)
    {
        var nextPlayer = table
                .GetNextPlayer(player, player => player.IsActive);

        table.AssignButton(nextPlayer, ButtonType.Turn);
    }

    /* General helpers */

    private void EnsurePlayerTurn(Player player, Table table)
    {
        if (!table.IsPlayerTurn(player))
        {
            throw new InvalidOperationException("It's not the player's turn.");
        }
    }

    private PlayerCollection GetEligiblePlayers(Table table)
    {
        var players = table.Players
            .Where(player => IsPlayerEligible(player))
            .ToArray();
        return new PlayerCollection(players);
    }

    private bool IsPlayerEligible(Player player)
    {
        if (player.IsSleeping)
        {
            return false;
        }
        if (player.BankrollBalance < MinBet)
        {
            return false;
        }

        return true;
    }

}

public class DealerBuilder : IBuilder<Dealer>
{
    private PokerRules? Rules { get; set; } = null;
    private DealerMemory? RoundTracker { get; set; } = null;

    public Dealer Build()
    {
        return new Dealer(
            rules: Rules ?? throw new InvalidOperationException("Rules are required"),
            memory: RoundTracker ?? DealerMemory.CreateEmpty(),
            gameLog: null
        );
    }

    public DealerBuilder WithRules(PokerRules rules)
    {
        Rules = rules;
        return this;
    }

    public DealerBuilder WithRoundTracker(DealerMemory roundTracker)
    {
        RoundTracker = roundTracker;
        return this;
    }

}
