using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play
{
    public readonly Piece ownPiece;
    public readonly Tile Tile;
    public readonly bool BlockedMove; // is the piece unable to move to this tile/
    public readonly Piece Blocker; // is there a piece blocking 'ownPiece' to move to this tile?
    public readonly Piece pieceOnTile;
    public readonly Play previousPlay; // reverse-linked list used to compute a movement vector
    public bool canCaptureAtDestination; // is ownPiece able to CAPTURE an opponent piece at this Tile, or just walk there if it's empty?

    public Play(Piece moving, Tile t, Piece blocker, Play previous, bool canCaptureAtDestination, bool moveBlocked)
    {
        this.ownPiece = moving;
        this.Tile = t;
        this.Blocker = blocker;
        this.pieceOnTile = t.CurrentPiece;
        this.previousPlay = previous;
        this.canCaptureAtDestination = canCaptureAtDestination;
        this.BlockedMove = moveBlocked;
    }

    public bool isCheck()
    {
        return pieceOnTile != null && pieceOnTile.player != ownPiece.player && pieceOnTile.isKing;
    }

    // a list of all the tiles that led to the the current play position
    public List<Tile> MovementVector()
    {
        var res = new List<Tile>();
        var curr = this;
        while (curr != null)
        {
            res.Add(curr.Tile);
            curr = curr.previousPlay;
        }

        res.Add(ownPiece.tile); // origin

        return res;
    }
}
