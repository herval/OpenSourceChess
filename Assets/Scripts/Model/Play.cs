using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public interface Play {
}

public class Lose : Play {
    public Player Player;

    public Lose(Player player) {
        this.Player = player;
    }
}

public class Move : Play {
    public readonly Board Board;
    public readonly Piece OwnPiece;
    public readonly Piece PieceAtDestination; // is there a piece blocking 'ownPiece' to move to this tile?
    public bool CanCaptureAtDestination; // is ownPiece able to CAPTURE an opponent piece at this Tile, or just walk there if it's empty?
    public readonly Move PreviousPlay; // reverse-linked list used to compute a movement vector
    public readonly Tile TileFrom;
    public readonly Tile TileTo;
    public readonly bool BlockedMove; // is the piece unable to move to this tile?
    public readonly bool IsFirstMove; // is this the first move of a given piece?

    public readonly List<Move> ConnectedPlays; // for plays involving multiple pieces (eg casteling)
    public readonly bool isRewind;

    public Move(
        Board board,
        Piece moving,
        Tile destinationTile,
        Piece pieceAtDestination,
        Move previous,
        bool canCaptureAtDestination,
        bool blocked, bool isFirstMove,
        bool isRewind = false,
        List<Move> connectedPlays = null
    ) {
        this.Board = board;
        this.ConnectedPlays = connectedPlays;
        this.TileFrom = moving.Tile;
        this.TileTo = destinationTile;
        this.BlockedMove = blocked;
        this.PreviousPlay = previous;
        this.OwnPiece = moving;
        this.PieceAtDestination = pieceAtDestination;
        this.IsFirstMove = isFirstMove;
        this.isRewind = isRewind;
        this.CanCaptureAtDestination = canCaptureAtDestination;
    }

    public bool IsCheck() {
        return PieceAtDestination != null && PieceAtDestination.Player.Number != OwnPiece.Player.Number &&
               PieceAtDestination.IsKing;
    }

    // a list of all the tiles that led to the the current play position
    public List<Tile> MovementVector() {
        var res = new List<Tile>();
        Move curr = this;
        while (curr != null) {
            res.Add(curr.TileTo);
            curr = curr.PreviousPlay;
        }

        res.Add(OwnPiece.Tile); // origin

        return res;
    }

    public Piece[,] Execute() {
        Piece[,] res = (Piece[,]) Board.Pieces.Clone();

        // if can move, get going
        // destroy existing child piece
        if (!isRewind && PieceAtDestination != null) {
            Debug.Log("Killing existing on " + this);
            PieceAtDestination.Tile = null;
        }

        if (isRewind && PieceAtDestination != null) {
            PieceAtDestination.Tile = TileFrom;

            res[TileFrom.X, TileFrom.Y] = PieceAtDestination;
        }

        OwnPiece.MovedAtLeastOnce = !isRewind || !IsFirstMove; // put back to unmoved
        OwnPiece.Tile = TileTo;

        res[TileTo.X, TileTo.Y] = OwnPiece;
        res[TileFrom.X, TileFrom.Y] = null;
        return res;
    }

    // build a "rewind" move
    public Move Reverse() {
        return new Move(
            board: Board,
            moving: OwnPiece,
            destinationTile: TileFrom,
            pieceAtDestination: this.PieceAtDestination,
            previous: null,
            canCaptureAtDestination: true,
            blocked: false,
            isFirstMove: this.IsFirstMove,
            isRewind: true,
            connectedPlays: this.ConnectedPlays?.ConvertAll(m => m.Reverse()));
    }

    public string ToOfficialNotation() {
        return "duh";
    }
}