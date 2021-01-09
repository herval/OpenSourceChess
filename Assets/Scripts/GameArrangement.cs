using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameArrangement : MonoBehaviour
{
    public abstract void Initialize(Board board, Player playerOne, Player playerTwo);
}

// a classic game of chess
public class StandardGameArrangement : GameArrangement
{

    Piece.Type[,] arrangement = new Piece.Type[,]{
        { Piece.Type.Rook, Piece.Type.Knight, Piece.Type.Bishop, Piece.Type.Queen, Piece.Type.King, Piece.Type.Bishop, Piece.Type.Knight, Piece.Type.Rook },
        { Piece.Type.Pawn, Piece.Type.Pawn, Piece.Type.Pawn, Piece.Type.Pawn, Piece.Type.Pawn, Piece.Type.Pawn, Piece.Type.Pawn, Piece.Type.Pawn },
    };


    // Start is called before the first frame update
    public override void Initialize(Board board, Player playerOne, Player playerTwo)
    {
        // render top pieces, they always belong to player two
        for (int x = 0; x < arrangement.GetLength(0); x++)
        {
            for (int y = 0; y < arrangement.GetLength(1); y++)
            {
                board.AddPiece(arrangement[x, y], y, board.height-x-1, playerTwo);
            }
        }


        // render bottom pieces
        for (int x = 0; x < arrangement.GetLength(0); x++)
        {
            for (int y = 0; y < arrangement.GetLength(1); y++)
            {
                board.AddPiece(arrangement[x, y], y, x, playerOne);
            }
        }

    }

}
