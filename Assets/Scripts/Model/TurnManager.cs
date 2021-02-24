using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// default turn manager checks for losing conditions
public class TurnManager {

    public virtual Play ActOn(Player player, Player opponent, Board board) {
        Piece.ComputePotentialMoves(board, player, opponent);

        var allMoves = player.UnblockedMoves();
        if (allMoves.Count == 0) {
            return new Lose(player);
        }

        return null;
    }

    public virtual bool IsReady() {
        return true;
    }
    
    public virtual void OnGameStarting() {
        
    }
    
    public virtual void GameStarted(Player player, Player opponent, Board board) {
        
    }

    public virtual bool IsHuman() {
        return true;
    }
}
