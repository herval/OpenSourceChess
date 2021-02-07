using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// pick a random piece and move it to any available position
public class RandomMoves : TurnManager
{
    public override PieceCommand ActOn(Player player, Player opponent, Board board)
    {
        var cmd = base.ActOn(player, opponent, board);
        if (cmd != null)
        {
            return cmd;
        }
        
        var allMoves = player.UnblockedMoves();

        // if no piece can move, declare defeat
        if (allMoves.Count == 0)
        {
            return new LoseGame(player);
        }

        var m = allMoves[Random.Range(0, allMoves.Count - 1)];
        return new Movement(m);
    }
}
