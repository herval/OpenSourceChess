using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PieceCommand {
}


public class LoseGame : PieceCommand {
    public readonly Player Player;

    public LoseGame(Player p) {
        this.Player = p;
    }
}

public class Movement : PieceCommand {
    public readonly Move Play;
    private PieceView OwnPiece;
    private PieceView PieceAtDestination;
    public TileView TileTo;
    public TileView TileFrom;

    public List<Movement> ConnectedMovements = new List<Movement>();

    public Movement(Move play, PieceView ownPiece, PieceView pieceAtDestination, TileView tileTo, TileView tileFrom) {
        this.OwnPiece = ownPiece;
        this.PieceAtDestination = pieceAtDestination;
        this.TileFrom = tileFrom;
        this.TileTo = tileTo;
        this.Play = play;
    }

    public Board Move(Board board, AfterAnimationCallback done) {
        var pieceStates = this.Play.Execute();

        // deselect current piece's tile
        OwnPiece.TileView.Selected = false;

        if (Play.isRewind && Play.PieceAtDestination != null) {
            PieceAtDestination.SetTile(TileFrom, true, null);
            PieceAtDestination.Player.PutBack(PieceAtDestination);
        }
        else if (Play.PieceAtDestination != null) {
            OwnPiece.Player.Capture(PieceAtDestination);
        }

        OwnPiece.SetTile(TileTo, false, done);

        board.Pieces = pieceStates;

        return board;
    }

    public Movement Reverse() {
        var play = Play.Reverse();
        return new Movement(
            play,
            this.OwnPiece,
            this.PieceAtDestination,
            this.TileFrom,
            this.TileTo);
    }
}