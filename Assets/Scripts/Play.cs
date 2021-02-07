using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play
{
    public readonly Piece ownPiece;
    public readonly Piece Blocker; // is there a piece blocking 'ownPiece' to move to this tile?
    public readonly Piece pieceOnTile;
    public bool canCaptureAtDestination; // is ownPiece able to CAPTURE an opponent piece at this Tile, or just walk there if it's empty?
    public readonly Play previousPlay; // reverse-linked list used to compute a movement vector
    public readonly Tile Tile;
    public readonly bool BlockedMove; // is the piece unable to move to this tile?
    
    public readonly List<Play> ConnectedPlays; // for plays involving multiple pieces (eg casteling)

    public Play(Piece moving, Tile destinationTile, Piece blocker, Play previous, bool canCaptureAtDestination, bool blocked, List<Play> connectedPlays = null)
    {
        this.ConnectedPlays = connectedPlays;
        this.Tile = destinationTile;
        this.BlockedMove = blocked;
        this.previousPlay = previous;
        this.ownPiece = moving;
        this.Blocker = blocker;
        this.pieceOnTile = destinationTile.CurrentPiece;
        this.canCaptureAtDestination = canCaptureAtDestination;
    }

    public bool isCheck()
    {
        return pieceOnTile != null && pieceOnTile.player != ownPiece.player && pieceOnTile.isKing;
    }

    // a list of all the tiles that led to the the current play position
    public List<Tile> MovementVector()
    {
        var res = new List<Tile>();
        Play curr = this;
        while (curr != null)
        {
            res.Add(curr.Tile);
            curr = curr.previousPlay;
        }

        res.Add(ownPiece.tile); // origin

        return res;
    }
}
