using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PieceCommand { }


public class LoseGame : PieceCommand
{
    public readonly Player player;

    public LoseGame(Player p)
    {
        this.player = p;
    }
}

public class MoveTo : PieceCommand
{
    public readonly Piece piece;
    public readonly Tile tile;

    public MoveTo(Piece p, Tile t)
    {
        this.piece = p;
        this.tile = t;
    }
}