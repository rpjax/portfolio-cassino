using Aidan.Core.Extensions;

namespace CassinoDemo.Poker;

public class PlayerSeats : PlayerCollection
{
    public PlayerSeats(IEnumerable<Player> players) : base(players)
    {

    }

    /*
     * Behavior helpers
     */

    public void AddPlayer(Player player)
    {
        if(PlayerList.Any(x => x.Id == player.Id))
        {
            throw new InvalidOperationException("Player already added.");
        }

        PlayerList.Add(player);
    }

    public void RemovePlayer(Player player)
    {
        if(!PlayerList.Remove(player))
        {
            throw new InvalidOperationException("Player not found.");
        }
    }

    /*
     * Internal helpers
     */

}
