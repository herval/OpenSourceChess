using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PieceArrangement {
    public abstract void Position(PieceFactory factory, TileView[,] boardView, ref Piece[,] pieces, PlayerView player);
}

// a classic game of chess
public class StandardPieceArrangement : PieceArrangement {
    PieceType[,] Arrangement = {
        {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King, PieceType.Bishop,
            PieceType.Knight, PieceType.Rook
        }, {
            PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn,
            PieceType.Pawn, PieceType.Pawn
        },
    };

    // Start is called before the first frame update
    public override void Position(
        PieceFactory factory,
        TileView[,] boardView,
        ref Piece[,] pieces,
        PlayerView player
    ) {
        if (player.StartingPosition == PlayerPosition.Top) {
            // render top pieces, they always belong to player facing down
            for (int y = 0; y < Arrangement.GetLength(0); y++) {
                for (int x = 0; x < Arrangement.GetLength(1); x++) {
                    pieces[x, boardView.GetLength(1) - y - 1] = AddPiece(factory, Arrangement[y, x],
                        boardView[x, boardView.GetLength(1) - y - 1], player);
                }
            }
        }
        else {
            // render bottom pieces
            for (int y = 0; y < Arrangement.GetLength(0); y++) {
                for (int x = 0; x < Arrangement.GetLength(1); x++) {
                    pieces[x, y] = AddPiece(factory, Arrangement[y, x], boardView[x, y], player);
                }
            }
        }
    }


    private Piece AddPiece(PieceFactory factory, PieceType piece, TileView tile, PlayerView player) {
        PieceView p = factory.Create(tile, piece, player);
        player.Pieces.Add(p.State);

        p.transform.position = tile.transform.position;

        return p.State;
    }
}