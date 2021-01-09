using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a classic game of chess
public class StandardGameArrangement : MonoBehaviour
{

    Piece.Type[,] arrangement = new Piece.Type[,]{
        { Piece.Type.Rook, Piece.Type.Knight, Piece.Type.Bishop, Piece.Type.Queen, Piece.Type.King, Piece.Type.Bishop, Piece.Type.Knight, Piece.Type.Rook },
        { Piece.Type.Pawn, Piece.Type.Pawn, Piece.Type.Pawn, Piece.Type.Pawn, Piece.Type.Pawn, Piece.Type.Pawn, Piece.Type.Pawn, Piece.Type.Pawn },
    };


    // Start is called before the first frame update
    public void Initialize(Board board, Player playerOne, Player playerTwo)
    {
        // render top pieces
        for (int x = 0; x < arrangement.GetLength(0); x++)
        {
            for (int y = 0; y < arrangement.GetLength(1); y++)
            {
                board.AddPiece(arrangement[x, y], false, y, board.height-x-1, playerTwo.color);
            }
        }


        // render bottom pieces
        for (int x = 0; x < arrangement.GetLength(0); x++)
        {
            for (int y = 0; y < arrangement.GetLength(1); y++)
            {
                board.AddPiece(arrangement[x, y], true, y, x, playerOne.color);
            }
        }

    }

}
