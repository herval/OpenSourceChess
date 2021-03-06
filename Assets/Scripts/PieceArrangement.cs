using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ArrangementType {
    Vanilla,
    Checkers,
    Random,
    Tiny
}

public abstract class PieceArrangement {
    protected abstract PieceType?[,] Arrangement(PlayerView player);

    public void Position(
        PieceFactory factory,
        TileView[,] boardView,
        ref Piece[,] pieces,
        PlayerView player
    ) {
        var arrangement = Arrangement(player);
        if (player.StartingPosition == PlayerPosition.Top) {
            // render top pieces, they always belong to player facing down
            for (int y = 0; y < arrangement.GetLength(0); y++) {
                for (int x = 0; x < arrangement.GetLength(1); x++) {
                    if (arrangement[y, x] != null) {
                        pieces[x, boardView.GetLength(1) - y - 1] = AddPiece(factory,
                            arrangement[y, x].GetValueOrDefault(),
                            boardView[x, boardView.GetLength(1) - y - 1], player);
                    }
                }
            }
        } else {
            // render bottom pieces
            for (int y = 0; y < arrangement.GetLength(0); y++) {
                for (int x = 0; x < arrangement.GetLength(1); x++) {
                    if (arrangement[y, x] != null) {
                        pieces[x, y] = AddPiece(factory, arrangement[y, x].GetValueOrDefault(), boardView[x, y],
                            player);
                    }
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

public class CheckersArrangement : PieceArrangement {
    PieceType?[,] arrangementBottom = {
        {
            null, PieceType.CheckersPawn, null, PieceType.CheckersPawn, null, PieceType.CheckersPawn,null, PieceType.CheckersPawn
        }, {
            PieceType.CheckersPawn, null, PieceType.CheckersPawn, null, PieceType.CheckersPawn, null, PieceType.CheckersPawn, null
        }, {
            null, PieceType.CheckersPawn, null, PieceType.CheckersPawn, null, PieceType.CheckersPawn,null, PieceType.CheckersPawn
        }
    };

    PieceType?[,] arrangementTop = {
        {
            PieceType.CheckersPawn, null, PieceType.CheckersPawn, null, PieceType.CheckersPawn, null, PieceType.CheckersPawn, null
        }, {
            null, PieceType.CheckersPawn, null, PieceType.CheckersPawn, null, PieceType.CheckersPawn,null, PieceType.CheckersPawn
        }, {
            PieceType.CheckersPawn, null, PieceType.CheckersPawn, null, PieceType.CheckersPawn, null, PieceType.CheckersPawn, null
        }
    };

    protected override PieceType?[,] Arrangement(PlayerView player) {
        if (player.StartingPosition == PlayerPosition.Bottom) {
            return arrangementBottom;
        } else {
            return arrangementTop;
        }
    }
}

public class HordeArrangement : PieceArrangement {
    PieceType?[,] arrangement = {
        {
            PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn,
            PieceType.Pawn, PieceType.Pawn
        }, {
            PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn,
            PieceType.Pawn, PieceType.Pawn
        }, {
            PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn,
            PieceType.Pawn, PieceType.Pawn
        }, {
            null, null, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn,
            null, null
        }
    };


    protected override PieceType?[,] Arrangement(PlayerView player) {
        return arrangement;
    }
}

public class TinyArmyArrangement : PieceArrangement {
    private PieceType[] possibleTypes =
        {PieceType.Pawn, PieceType.Queen, PieceType.Bishop, PieceType.Knight, PieceType.Rook};

    protected override PieceType?[,] Arrangement(PlayerView player) {
        PieceType?[,] arrangement = new PieceType?[3, 8];

        var kingPosX = Random.Range(0, arrangement.GetLength(0));
        var kingPosY = Random.Range(0, arrangement.GetLength(1) - 1);
        arrangement[kingPosX, kingPosY] = PieceType.King;

        int piecesRemaining = 5;
        while (piecesRemaining > 0) {
            var x = Random.Range(0, arrangement.GetLength(0));
            var y = Random.Range(0, arrangement.GetLength(1) - 1);

            if (arrangement[x, y] == null) {
                arrangement[x, y] = possibleTypes[Random.Range(0, possibleTypes.Length)];
                piecesRemaining--;
            }
        }

        return arrangement;
    }
}

public class RandomArmyArrangement : PieceArrangement {
    private PieceType[] possibleTypes =
        {PieceType.Pawn, PieceType.Queen, PieceType.Bishop, PieceType.Knight, PieceType.Rook};

    protected override PieceType?[,] Arrangement(PlayerView player) {
        PieceType?[,] arrangement = new PieceType?[2, 8];

        var kingPos = Random.Range(0, 7);
        arrangement[0, kingPos] = PieceType.King;
        for (int x = 0; x < arrangement.GetLength(0); x++) {
            for (int y = 0; y < arrangement.GetLength(1); y++) {
                if (arrangement[x, y] == null) {
                    arrangement[x, y] = possibleTypes[Random.Range(0, possibleTypes.Length)];
                }
            }
        }

        return arrangement;
    }
}

// a classic game of chess
public class StandardPieceArrangement : PieceArrangement {
    PieceType?[,] arrangement = {
        {
            PieceType.Rook, PieceType.Knight, PieceType.Bishop, PieceType.Queen, PieceType.King, PieceType.Bishop,
            PieceType.Knight, PieceType.Rook
        }, {
            PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn, PieceType.Pawn,
            PieceType.Pawn, PieceType.Pawn
        },
    };

    protected override PieceType?[,] Arrangement(PlayerView player) {
        return arrangement;
    }
}