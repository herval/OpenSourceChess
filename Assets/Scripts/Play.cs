using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Play
{
    public readonly Piece ownPiece;
    public readonly Tile Tile;
    public readonly bool Blocked;
    public readonly Piece pieceOnTile;

    public Play(Piece moving, Tile t, bool blocked)
    {
        this.ownPiece = moving;
        this.Tile = t;
        this.Blocked = blocked;
        this.pieceOnTile = t.CurrentPiece;
    }

    public bool isCheck()
    {
        return pieceOnTile != null && pieceOnTile.player != ownPiece.player && pieceOnTile.isKing;
    }
}
