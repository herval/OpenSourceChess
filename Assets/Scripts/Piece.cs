using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    static (int, int)[] forward = new (int, int)[] {
        (0, 1),
    };

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

    public Type type;
    //Sprite sprite;
    public bool facingUp;
    public Color color;
    public Tile tile;
    public Player player;

    internal int value; // used for min/max computations https://en.wikipedia.org/wiki/Chess_piece_relative_value


    internal bool isKing;

    public bool movedAtLeastOnce = false;

    // Set when a piece is now allowed to move (ie during a check situation)
    public bool canMove = true;

    public List<Play> PotentialMoves = new List<Play>();

    internal void ResetMoves()
    {
        this.canMove = true;
        this.PotentialMoves.Clear();
    }

    public bool MoveTo(Tile tile)
    {
        // deselect current piece's tile
        this.tile.Selected = false;

        // if moving to its own tile, just reposition and deselect it
        if (tile == this.tile || !this.CanMoveTo(tile))
        {
            //Debug.Log("Cant move!");
            this.transform.position = this.tile.transform.position;
            return false;
        }

        // if can move, get going
        // destroy existing child piece
        var c = tile.CurrentPiece;
        if (c != null && c != this)
        {
            Debug.Log("Killing existing on " + this);
            tile.CurrentPiece.Kill();
        }
        tile.CurrentPiece = this;

        this.tile.CurrentPiece = null; // remove ref from tile so piece doesn't show in two places at the same time
        this.tile = tile;
        this.movedAtLeastOnce = true;
        this.transform.parent = tile.transform;

        // TODO is this efficient?
        StartCoroutine(AnimationHelper.MoveOverSeconds(this.gameObject, this.tile.transform.position, 0.2f));

        return true;
    }

    public void Kill()
    {
        Destroy(gameObject);
        player.Pieces.Remove(this);
    }

    public void Select()
    {
        this.tile.Selected = true;
    }

    public bool CanMoveTo(Tile tile)
    {
        return this.PotentialMoves.Find(m => m.Tile == tile && !m.Blocked) != null;
    }

    internal void ComputeMoves(Tile[,] tiles)
    {
        // TODO if player is in check, only allow moves that will eliminate or block the pieces involved on the check

        List<Play> potentialMoves;

        // TODO use inheritance?
        switch (type)
        {
            case Piece.Type.Bishop:
                // verticals until hitting something
                potentialMoves = tryAll(diagonals, tile.x, tile.y, int.MaxValue, tiles);
                break;
            case Piece.Type.King:
                potentialMoves = tryAll(adjacencies, tile.x, tile.y, 1, tiles);
                break;
            case Piece.Type.Queen:
                potentialMoves = tryAll(adjacencies, tile.x, tile.y, int.MaxValue, tiles);
                break;
            case Piece.Type.Rook:
                // straight lines until hitting an adversary
                potentialMoves = tryAll(linears, tile.x, tile.y, int.MaxValue, tiles);
                break;
            case Piece.Type.Pawn:
                // if first move, can move one or two squares
                int maxSquares = movedAtLeastOnce ? 1 : 2;

                potentialMoves = tryAll(forward, tile.x, tile.y, maxSquares, tiles, false, true, false);

                // eating diagonally
                potentialMoves.AddRange(
                    tryMove(tile.x, tile.y, 1, 1, 1, tiles, new List<Play>(), true, false, false)
                );
                potentialMoves.AddRange(
                    tryMove(tile.x, tile.y, -1, 1, 1, tiles, new List<Play>(), true, false, false)
                );

                break;
            case Piece.Type.Knight:
                potentialMoves = tryAll(knightMoves, tile.x, tile.y, 1, tiles);
                break;
            default:
                return;
        }

        this.PotentialMoves = potentialMoves
            //.FindAll((m) => !m.Blocked)
            .ToList(); 

    }

    private List<Play> tryAll((int, int)[] directions, int currentX, int currentY, int maxMoves, Tile[,] tiles, bool onlyWhenCapturing = false, bool onlyMoveNoEat = false, bool blocked = false)
    {
        var res = new List<Play>();
        foreach(var xy in directions)
        {
            res.AddRange(
                tryMove(currentX, currentY, xy.Item1, xy.Item2, maxMoves, tiles, new List<Play>(), onlyWhenCapturing, onlyMoveNoEat, blocked)
            );
        }

        return res;
    }

    // TODO simplify all the bools
    private List<Play> tryMove(int x, int y, int deltaX, int deltaY, int maxMoves, Tile[,] tiles, List<Play> validMoves, bool onlyWhenCapturing, bool onlyMoveNoEat, bool blocked)
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
        if ((t.CurrentPiece == null || t.CurrentPiece.player != this.player)
            && (!onlyWhenCapturing || (onlyWhenCapturing && t.CurrentPiece != null)) // can only move to that position if there's a capturable piece
            && (!onlyMoveNoEat || (onlyMoveNoEat && t.CurrentPiece == null))) // can only move when there's NOT a capturable piece
        {
            validMoves.Add(
                new Play(this, tiles[newX, newY], blocked)
            );
        }

        // can't move further when hitting another piece
        // but we compute the full motion anyway in case we get to a check
        if(t.CurrentPiece != null)
        {
            // already blocked by one piece OR by a piece the player owns, so no point on going further
            if (blocked || t.CurrentPiece.player == this.player)
            {
                return validMoves;
            }
            else
            {
                blocked = true;
            }
        }

        return tryMove(newX, newY, deltaX, deltaY, maxMoves-1, tiles, validMoves, onlyWhenCapturing, onlyMoveNoEat, blocked);
    }
}
