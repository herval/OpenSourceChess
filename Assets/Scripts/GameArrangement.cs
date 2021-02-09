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

    Piece.PieceType[,] Arrangement = {
        { Piece.PieceType.Rook, Piece.PieceType.Knight, Piece.PieceType.Bishop, Piece.PieceType.Queen, Piece.PieceType.King, Piece.PieceType.Bishop, Piece.PieceType.Knight, Piece.PieceType.Rook },
        { Piece.PieceType.Pawn, Piece.PieceType.Pawn, Piece.PieceType.Pawn, Piece.PieceType.Pawn, Piece.PieceType.Pawn, Piece.PieceType.Pawn, Piece.PieceType.Pawn, Piece.PieceType.Pawn },
    };


    // Start is called before the first frame update
    public override void Initialize(Board board, Player playerOne, Player playerTwo)
    {
        // render top pieces, they always belong to player two
        for (int x = 0; x < Arrangement.GetLength(0); x++)
        {
            for (int y = 0; y < Arrangement.GetLength(1); y++)
            {
                board.AddPiece(Arrangement[x, y], y, board.Height-x-1, playerTwo);
            }
        }


        // render bottom pieces
        for (int x = 0; x < Arrangement.GetLength(0); x++)
        {
            for (int y = 0; y < Arrangement.GetLength(1); y++)
            {
                board.AddPiece(Arrangement[x, y], y, x, playerOne);
            }
        }

    }

}
