using System;
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
    public readonly Piece CapturedPiece; // is there a piece blocking 'ownPiece' to move to this tile?
    public readonly Move PreviousPlay; // reverse-linked list used to compute a movement vector
    public readonly Tile TileFrom;
    public readonly Tile TileTo;
    public readonly Tile TileCaptured; // not null if this move includes capturing a piece at this tile. It's usually the same as the TileTo, except for en passant or checkers moves
    public readonly bool BlockedMove; // is the piece unable to move to this tile?
    public readonly bool IsFirstMove; // is this the first move of a given piece?

    public readonly bool IsCasteling; // casteling has a special notation

    public readonly List<Move> ConnectedPlays; // for plays involving multiple pieces (eg casteling)
    public readonly bool IsRewind;

    public Move(
        Board board,
        Piece moving,
        Tile destinationTile,
        Piece capturedPiece,
        Move previous,
        Tile captureAt,
        bool blocked, bool isFirstMove,
        bool isRewind = false,
        bool isCasteling = false,
        List<Move> connectedPlays = null
    ) {
        this.Board = board;
        this.ConnectedPlays = connectedPlays;
        this.TileFrom = moving.Tile;
        this.TileTo = destinationTile;
        this.BlockedMove = blocked;
        this.PreviousPlay = previous;
        this.OwnPiece = moving;
        this.CapturedPiece = capturedPiece;
        this.IsFirstMove = isFirstMove;
        this.IsRewind = isRewind;
        this.TileCaptured = captureAt;
        this.IsCasteling = isCasteling;
    }

    public bool IsCheck() {
        return CapturedPiece != null && CapturedPiece.Player.Number != OwnPiece.Player.Number &&
               CapturedPiece.IsKing;
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
        Piece[,] res = (Piece[,])Board.Pieces.Clone();

        OwnPiece.MovedAtLeastOnce = !IsRewind || !IsFirstMove; // put back to unmoved
        OwnPiece.Tile = TileTo;

        res[TileTo.X, TileTo.Y] = OwnPiece;
        res[TileFrom.X, TileFrom.Y] = null;

        // if can move, get going
        // destroy existing child piece
        if (!IsRewind && CapturedPiece != null) {
            Debug.Log("Killing existing on " + this);
            CapturedPiece.Tile = null;
            res[TileCaptured.X, TileCaptured.Y] = null;
        }

        if (IsRewind && CapturedPiece != null) {
            CapturedPiece.Tile = TileCaptured;
            res[TileCaptured.X, TileCaptured.Y] = CapturedPiece;
        }

        return res;
    }

    // build a "rewind" move
    public Move Reverse() {
        return new Move(
            board: Board,
            moving: OwnPiece,
            destinationTile: TileFrom,
            capturedPiece: this.CapturedPiece,
            previous: this.PreviousPlay,
            captureAt: this.TileCaptured,
            blocked: false,
            isFirstMove: this.IsFirstMove,
            isRewind: true,
            connectedPlays: this.ConnectedPlays?.ConvertAll(m => m.Reverse()));
    }

    // https://en.wikipedia.org/wiki/Algebraic_notation_(chess)
    public string ToFigurineAlgebraicNotation() {
        if (IsCasteling) {
            return "0-0 or 0-0-0"; // TODO represent this properly
        }

        string res = "";
        res += OwnPiece.FigurineAlgebraicNotation;

        if (CapturedPiece != null) {
            res += "x";
        }

        res += TileTo.ToFigurineAlgebraicNotation(this.Board);

        if (this.IsCheck() && !this.BlockedMove) {
            res += "+";
        }

        // TODO represent checkmate?

        return res;
    }
}