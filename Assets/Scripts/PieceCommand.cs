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
    private PieceView CapturedPiece;
    public TileView TileTo;
    public TileView TileFrom;
    public TileView TileCapturedAt;

    public List<Movement> ConnectedMovements = new List<Movement>();

    public Movement(Move play, PieceView ownPiece, PieceView capturedPiece, TileView tileTo, TileView tileFrom, TileView tileCapturedPiece) {
        this.OwnPiece = ownPiece;
        this.CapturedPiece = capturedPiece;
        this.TileFrom = tileFrom;
        this.TileTo = tileTo;
        this.TileCapturedAt = tileCapturedPiece;
        this.Play = play;
    }

    public Board Move(Board board, AfterAnimationCallback done) {
        var pieceStates = this.Play.Execute();

        // deselect current piece's tile
        OwnPiece.TileView.Selected = false;

        if (Play.IsRewind && CapturedPiece != null) {
            CapturedPiece.SetTile(TileCapturedAt, true, null);
            CapturedPiece.Player.PutBack(CapturedPiece);
        } else if (CapturedPiece != null) {
            OwnPiece.Player.Capture(CapturedPiece);
            TileCapturedAt.CurrentPiece = null;
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
            this.CapturedPiece,
            this.TileFrom,
            this.TileTo,
            this.TileCapturedAt);
    }
}