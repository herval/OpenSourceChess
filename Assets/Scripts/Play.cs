using System.Collections.Generic;
using UnityEngine;

public class Play
{
    public readonly Piece OwnPiece;
    public readonly Piece PieceAtDestination; // is there a piece blocking 'ownPiece' to move to this tile?
    public bool CanCaptureAtDestination; // is ownPiece able to CAPTURE an opponent piece at this Tile, or just walk there if it's empty?
    public readonly Play PreviousPlay; // reverse-linked list used to compute a movement vector
    public readonly Tile TileFrom;
    public readonly Tile TileTo;
    public readonly bool BlockedMove; // is the piece unable to move to this tile?
    public readonly bool IsFirstMove; // is this the first move of a given piece?
    
    public readonly List<Play> ConnectedPlays; // for plays involving multiple pieces (eg casteling)
    public readonly bool isRewind;

    public Play(Piece moving, Tile destinationTile, Piece pieceAtDestination, Play previous, bool canCaptureAtDestination, bool blocked, bool isFirstMove, bool isRewind = false, List<Play> connectedPlays = null)
    {
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

    public bool IsCheck()
    {
        return PieceAtDestination != null && PieceAtDestination.Player != OwnPiece.Player && PieceAtDestination.IsKing;
    }

    // a list of all the tiles that led to the the current play position
    public List<Tile> MovementVector()
    {
        var res = new List<Tile>();
        Play curr = this;
        while (curr != null)
        {
            res.Add(curr.TileTo);
            curr = curr.PreviousPlay;
        }

        res.Add(OwnPiece.Tile); // origin

        return res;
    }

    public void Move(AfterAnimationCallback done)
    {
        // deselect current piece's tile
        OwnPiece.Tile.Selected = false;

        // if can move, get going
        // destroy existing child piece
        if (!isRewind && TileTo.CurrentPiece != null)
        {
            Debug.Log("Killing existing on " + this);
            // TODO animate
            OwnPiece.Player.Capture(TileTo.CurrentPiece); 
            // TODO uncapture?
        }

        if (isRewind && PieceAtDestination != null)
        {
            PieceAtDestination.SetTile(TileFrom, true, null);
            PieceAtDestination.Player.PutBack(PieceAtDestination);
        }

        OwnPiece.MovedAtLeastOnce = !isRewind || !IsFirstMove; // put back to unmoved
        OwnPiece.SetTile(TileTo, false, done);
    }

    // build a "rewind" move
    public Play Reverse()
    {
        return new Play(
            OwnPiece,
            TileFrom,
            this.PieceAtDestination,
            null,
            true,
            false,
            this.IsFirstMove,
            true,
            this.ConnectedPlays?.ConvertAll(m => m.Reverse()));
    }

    public string ToOfficialNotation()
    {
        return "duh";
    }
}
