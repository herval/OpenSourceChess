using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// pick a random piece and move it to any available position
public class RandomMoves : TurnManager
{
    public override Play ActOn(Player player, Player opponent, Board boardView)
    {
        var cmd = base.ActOn(player, opponent, boardView);
        if (cmd != null)
        {
            return cmd;
        }
        
        var allMoves = player.UnblockedMoves();

        // if no piece can move, declare defeat
        if (allMoves.Count == 0) {
            return null;
        }

        var m = allMoves[Random.Range(0, allMoves.Count - 1)];
        return m;
    }

    public override bool IsHuman() {
        return false;
    }
}
