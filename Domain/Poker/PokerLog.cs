using Aidan.Core.Extensions;

namespace Domain.Poker;

public enum PlayerActionType
{
    Check,
    Call,
    Raise,
    Fold,
    AllIn
}

public class Action
{
    public Guid PlayerId { get; }

    public Action(Guid playerId)
    {
        PlayerId = playerId;
    }
}

public class RoundOfBetting
{
    public IReadOnlyList<Action> ActionHistory { get; }

    public RoundOfBetting(IEnumerable<Action> actionHistory)
    {
        ActionHistory = new List<Action>(actionHistory);
    }

    public bool HasPlayerPlayed(Player nextPlayer)
    {
        return ActionHistory.Any(a => a.PlayerId == nextPlayer.Id);
    }

}

public class RoundLog
{
    public IReadOnlyList<RoundOfBetting> Rounds { get; }

    public RoundLog(IEnumerable<RoundOfBetting> rounds)
    {
        Rounds = new List<RoundOfBetting>(rounds);
    }
}

public interface IPokerLog
{
    
}

public class PokerLog : IPokerLog
{   
    public IReadOnlyList<Action> ActionRecords { get; }
    public IReadOnlyList<RoundOfBetting> RoundRecords { get; }
    public IReadOnlyList<RoundLog> MatchRecords { get; }
        
    public PokerLog(
        IEnumerable<Action> actionHistory,
        IEnumerable<RoundOfBetting> roundHistory,
        IEnumerable<RoundLog> matchHistory)
    {
        ActionRecords = new List<Action>(actionHistory);
        RoundRecords = new List<RoundOfBetting>(roundHistory);
        MatchRecords = new List<RoundLog>(matchHistory);
    }

    private List<Action> ActionList => (List<Action>)ActionRecords;
    private List<RoundOfBetting> RoundList => (List<RoundOfBetting>)RoundRecords;
    private List<RoundLog> MatchList => (List<RoundLog>)MatchRecords;

    public void CreateActionRecord(Guid playerId)
    {
        ActionList.Add(new Action(playerId));
    }

    public void EndRoundOfBetting()
    {
        if (ActionList.IsEmpty())
        {
            return;
        }

        RoundList.Add(new RoundOfBetting(actionHistory: ActionList));
        ActionList.Clear();
    }

    public void EndMatch()
    {
        if (RoundList.IsEmpty())
        {
            return;
        }

        MatchList.Add(new RoundLog(RoundList));
        RoundList.Clear();
    }

    public RoundOfBetting GetCurrentRound()
    {
        return new RoundOfBetting(ActionList);
    }

}