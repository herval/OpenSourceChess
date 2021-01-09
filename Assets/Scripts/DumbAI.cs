using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// pick a random piece and move it to any available position
public class DumbAI : TurnManager
{
    public override PieceCommand ActOn(Player player, Board board)
    {
        // if no piece can move, declare defeat
        if(player.Pieces.FindAll(p => p.PotentialMoves.Count > 0).Count == 00)
        {
            return new LoseGame(player);
        }

        while (true)
        { // icky, BUT there should ALWAYS be an option to play, at this point
            Piece p = player.Pieces[Random.Range(0, player.Pieces.Count - 1)];
            if (p.PotentialMoves.Count > 0)
            {
                return new MoveTo(
                    p,
                    p.PotentialMoves[Random.Range(0, p.PotentialMoves.Count - 1)]
                );
            }
        }
    }
}
