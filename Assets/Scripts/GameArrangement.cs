using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameArrangement {
    public abstract Piece[,] Initialize(PieceFactory factory, TileView[,] boardView, PlayerView bottomPlayer,
        PlayerView topPlayer);
}

// a classic game of chess
public class StandardGameArrangement : GameArrangement {
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
    public override Piece[,] Initialize(
        PieceFactory factory,
        TileView[,] boardView,
        PlayerView bottomPlayer,
        PlayerView topPlayer
    ) {
        Piece[,] pieces = new Piece[boardView.GetLength(0), boardView.GetLength(1)];

        // render top pieces, they always belong to player facing down
        for (int y = 0; y < Arrangement.GetLength(0); y++) {
            for (int x = 0; x < Arrangement.GetLength(1); x++) {
                pieces[x, boardView.GetLength(1) - y - 1] = AddPiece(factory, Arrangement[y, x],
                    boardView[x, boardView.GetLength(1) - y - 1], topPlayer);
            }
        }

        // render bottom pieces
        for (int y = 0; y < Arrangement.GetLength(0); y++) {
            for (int x = 0; x < Arrangement.GetLength(1); x++) {
                pieces[x, y] = AddPiece(factory, Arrangement[y, x], boardView[x, y], bottomPlayer);
            }
        }

        return pieces;
    }


    private Piece AddPiece(PieceFactory factory, PieceType piece, TileView tile, PlayerView player) {
        PieceView p = factory.Create(tile, piece, player);
        player.Pieces.Add(p.State);

        p.transform.position = tile.transform.position;

        return p.State;
    }
}