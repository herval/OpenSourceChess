using System.Collections.Generic;
using UnityEngine;

public class Play
{
    public readonly Piece ownPiece;
    public readonly Piece PieceAtDestination; // is there a piece blocking 'ownPiece' to move to this tile?
    public bool canCaptureAtDestination; // is ownPiece able to CAPTURE an opponent piece at this Tile, or just walk there if it's empty?
    public readonly Play previousPlay; // reverse-linked list used to compute a movement vector
    public readonly Tile TileFrom;
    public readonly Tile TileTo;
    public readonly bool BlockedMove; // is the piece unable to move to this tile?
    public readonly bool isFirstMove; // is this the first move of a given piece?
    
    public readonly List<Play> ConnectedPlays; // for plays involving multiple pieces (eg casteling)
    public readonly bool isRewind;

    public Play(Piece moving, Tile destinationTile, Piece pieceAtDestination, Play previous, bool canCaptureAtDestination, bool blocked, bool isFirstMove, bool isRewind = false, List<Play> connectedPlays = null)
    {
        this.ConnectedPlays = connectedPlays;
        this.TileFrom = moving.tile;
        this.TileTo = destinationTile;
        this.BlockedMove = blocked;
        this.previousPlay = previous;
        this.ownPiece = moving;
        this.PieceAtDestination = pieceAtDestination;
        this.isFirstMove = isFirstMove;
        this.isRewind = isRewind;
        this.canCaptureAtDestination = canCaptureAtDestination;
    }

    public bool isCheck()
    {
        return PieceAtDestination != null && PieceAtDestination.player != ownPiece.player && PieceAtDestination.isKing;
    }

    // a list of all the tiles that led to the the current play position
    public List<Tile> MovementVector()
    {
        var res = new List<Tile>();
        Play curr = this;
        while (curr != null)
        {
            res.Add(curr.TileTo);
            curr = curr.previousPlay;
        }

        res.Add(ownPiece.tile); // origin

        return res;
    }

    public void Move(AfterAnimationCallback done)
    {
        // deselect current piece's tile
        ownPiece.tile.Selected = false;

        // if can move, get going
        // destroy existing child piece
        if (!isRewind && TileTo.CurrentPiece != null)
        {
            Debug.Log("Killing existing on " + this);
            // TODO animate
            ownPiece.player.Capture(TileTo.CurrentPiece); 
            // TODO uncapture?
        }

        if (isRewind && PieceAtDestination != null)
        {
            PieceAtDestination.SetTile(TileFrom, true, null);
            PieceAtDestination.player.PutBack(PieceAtDestination);
        }

        ownPiece.movedAtLeastOnce = !isRewind || !isFirstMove; // put back to unmoved
        ownPiece.SetTile(TileTo, false, done);
    }

    // build a "rewind" move
    public Play Reverse()
    {
        return new Play(
            ownPiece,
            TileFrom,
            this.PieceAtDestination,
            null,
            true,
            false,
            this.isFirstMove,
            true,
            this.ConnectedPlays?.ConvertAll(m => m.Reverse()));
    }

    public string ToOfficialNotation()
    {
        return "duh";
    }
}
