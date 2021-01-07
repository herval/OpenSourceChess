using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{

    public enum Type
    {
        Rook,
        King,
        Queen,
        Bishop,
        Knight,
        Pawn,
    }

    public Type type;
    //Sprite sprite;
    public bool facingUp;

    public bool movedAtLeastOnce = false;

    public List<Tile> PotentialMoves = new List<Tile>();

    // TODO store x,y here?


    static (int, int)[] diagonals = new (int, int)[] {
        (1, 1),
        (-1, -1),
        (-1, 1),
        (1, -1)
    };

    static (int, int)[] linears = new (int, int)[] {
        (1, 0),
        (0, 1),
        (-1, 0),
        (0, -1)
    };

    static (int, int)[] adjacencies = new (int, int)[] {
        (1, 0),
        (0, 1),
        (-1, 0),
        (0, -1),
        (1, 1),
        (-1, -1),
        (-1, 1),
        (1, -1)
    };

    static (int, int)[] knightMoves = new (int, int)[] {
        (2, 1),
        (2, -1),
        (-2, 1),
        (-2, -1),
        (1, 2),
        (1, -2),
        (-1, 2),
        (-1, -2)
    };


    public bool CanMoveTo(Tile tile)
    {
        if (tile.CurrentPiece()?.facingUp == this.facingUp)
        {
            return false; // can't eat own pieces
        }

        return this.PotentialMoves.Contains(tile);
    }

    internal void ComputeMoves(int currentX, int currentY, Tile[,] tiles)
    {
        PotentialMoves.Clear();

        // TODO use inheritance?
        switch (type)
        {
            case Piece.Type.Bishop:
                // verticals until hitting something
                PotentialMoves = tryAll(diagonals, currentX, currentY, int.MaxValue, tiles);
                break;
            case Piece.Type.King:
                PotentialMoves = tryAll(adjacencies, currentX, currentY, 1, tiles);
                break;
            case Piece.Type.Queen:
                PotentialMoves = tryAll(adjacencies, currentX, currentY, int.MaxValue, tiles);
                break;
            case Piece.Type.Rook:
                // straight lines until hitting an adversary
                PotentialMoves = tryAll(linears, currentX, currentY, int.MaxValue, tiles);
                break;
            case Piece.Type.Pawn:
                // TODO if first move, can move one or two squares

                if(!movedAtLeastOnce)
                {
                    PotentialMoves.AddRange(
                        tryMove(currentX, currentY, 0, 2, 1, tiles, new List<Tile>())
                    );
                }

                PotentialMoves.AddRange(
                    tryMove(currentX, currentY, 0, 1, 1, tiles, new List<Tile>())
                );

                // eating diagonally
                PotentialMoves.AddRange(
                    tryMove(currentX, currentY, 1, 1, 1, tiles, new List<Tile>(), true)
                );
                PotentialMoves.AddRange(
                    tryMove(currentX, currentY, -1, 1, 1, tiles, new List<Tile>(), true)
                );

                break;
            case Piece.Type.Knight:
                PotentialMoves = tryAll(knightMoves, currentX, currentY, 1, tiles);
                break;
            default:
                return;
        }
    }

    private List<Tile> tryAll((int, int)[] directions, int currentX, int currentY, int maxMoves, Tile[,] tiles)
    {
        var res = new List<Tile>();
        foreach(var xy in directions)
        {
            res.AddRange(
                tryMove(currentX, currentY, xy.Item1, xy.Item2, maxMoves, tiles, new List<Tile>())
            );
        }

        return res;
    }

    private List<Tile> tryMove(int x, int y, int deltaX, int deltaY, int maxMoves, Tile[,] tiles, List<Tile> validMoves, bool onlyWhenCapturing = false)
    {
        // flip the y axis when piece is facing down
        int yFlip = facingUp ? 1 : -1;
        int newX = x + deltaX;
        int newY = y+(deltaY * yFlip);

        // we're done when hitting a corner or there's no more moves possible
        if (newX < 0 || newX >= tiles.GetLength(0)
              || newY < 0 || newY >= tiles.GetLength(1)
              || (maxMoves <= 0) )
        {
            return validMoves;
        }

        var t = tiles[newX, newY];
        // can eat when target is empty or contains enemy OR when it contains an enemy if onlyWhenCapturing is true
        if ((t.CurrentPiece() == null || t.CurrentPiece()?.facingUp != this.facingUp)
            && (!onlyWhenCapturing || (onlyWhenCapturing && t.CurrentPiece() != null)))
        {
            validMoves.Add(tiles[newX, newY]);
        }

        // can't move further when hitting another piece
        if(t.CurrentPiece() != null)
        {
            return validMoves;
        }

        return tryMove(newX, newY, deltaX, deltaY, maxMoves-1, tiles, validMoves, onlyWhenCapturing);
    }

    //public static GameObject Create(Piece.Type type, bool white)
    //{
    //    //Sprite spr = spriteManager.SpriteFor(type, white);
    //    return null; //Instantiate(WhiteBishop)
    //}


    //public bool CanMoveTo(Tile tile)
    //{
    //    return false;
    //}
}
