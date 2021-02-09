using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// default turn manager checks for losing conditions
public class TurnManager {

    public virtual PieceCommand ActOn(Player player, Player opponent, Board board)
    {
        Piece.ComputePotentialMoves(board.Tiles, player, opponent);

        var allMoves = player.UnblockedMoves();
        if (allMoves.Count == 0)
        {
            return new LoseGame(player);
        }

        return null;
    }
}
