using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// default turn manager checks for losing conditions
public class TurnManager {

    public virtual PieceCommand ActOn(Player player, Player opponent, Board board)
    {
        board.ComputePotentialMoves(player, opponent);

        var allMoves = player.PotentialMoves();
        if (allMoves.Count == 0)
        {
            return new LoseGame(player);
        }

        return null;
    }
}
