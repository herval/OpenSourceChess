using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

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

    private enum MovementType
    {
        MoveOrCapture,
        CaptureOnly,
        MoveOnly,
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
    public bool facingUp;
    public Color color;

    private Tile _tile;
    public Tile tile
    {
        get
        {
            return _tile;
        }

        set
        {
            _tile = value;
            if (sprite == null)
            {
                sprite = this.GetComponent<SpriteRenderer>();
                if (sprite == null)
                {
                    sprite = this.GetComponentInChildren<SpriteRenderer>();
                }
                if (sprite == null)
                {
                    throw new MissingReferenceException("A sprite is required!");
                }
            }

            // set the order in layer for the sprite
            sprite.sortingOrder = -value.y;
        }
    }

    public Player player;

    internal int value; // used for min/max computations https://en.wikipedia.org/wiki/Chess_piece_relative_value

    internal bool isKing; // used to identify LE ROI

    public bool movedAtLeastOnce = false; // used for pawns' first move

    public List<Play> PotentialMoves = new List<Play>();
    public SpriteRenderer sprite;

    public void Freeze()
    {
        var animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    internal void ResetMoves()
    {
        this.PotentialMoves.Clear();
    }

    public void MoveTo(Tile tile, AfterAnimationCallback done)
    {
        // deselect current piece's tile
        this.tile.Selected = false;

        // if moving to its own tile, just reposition and deselect it
        if (tile == this.tile || !this.CanMoveTo(tile))
        {
            //Debug.Log("Cant move!");
            this.transform.position = this.tile.transform.position;

            done.Invoke(false);
        }

        // if can move, get going
        // destroy existing child piece
        var c = tile.CurrentPiece;
        if (c != null && c != this)
        {
            Debug.Log("Killing existing on " + this);
            // TODO animate
            this.player.Capture(tile.CurrentPiece);
        }
        tile.CurrentPiece = this;

        this.tile.CurrentPiece = null; // remove ref from tile so piece doesn't show in two places at the same time
        this.tile = tile;
        this.movedAtLeastOnce = true;
        this.transform.parent = tile.transform;

        // TODO is this efficient?
        StartCoroutine(AnimationHelper.MoveOverSeconds(this.gameObject, this.tile.transform.position, 0.2f, done));
    }
    
    public bool CanMoveTo(Tile tile)
    {
        return this.PotentialMoves.Find(m => m.Tile == tile && !m.BlockedMove) != null;
    }

    internal void ComputeMoves(Tile[,] tiles)
    {
        List<Play> potentialMoves;

        switch (type)
        {
            case Piece.Type.Bishop:
                // verticals until hitting something
                potentialMoves = tryAll(diagonals,
                    tile.x, tile.y,
                    int.MaxValue, tiles,
                    MovementType.MoveOrCapture,
                    null);
                break;
            case Piece.Type.King:
                potentialMoves = tryAll(adjacencies,
                    tile.x, tile.y, 1, tiles,
                    MovementType.MoveOrCapture,
                    null);
                break;
            case Piece.Type.Queen:
                potentialMoves = tryAll(adjacencies,
                    tile.x, tile.y,
                    int.MaxValue, tiles,
                    MovementType.MoveOrCapture,
                    null);
                break;
            case Piece.Type.Rook:
                // straight lines until hitting an adversary
                potentialMoves = tryAll(linears,
                    tile.x, tile.y,
                    int.MaxValue, tiles,
                    MovementType.MoveOrCapture,
                    null);
                break;
            case Piece.Type.Pawn:
                // if first move, can move one or two squares
                int maxSquares = movedAtLeastOnce ? 1 : 2;

                potentialMoves = tryAll(forward,
                    tile.x, tile.y,
                    maxSquares, tiles,
                    MovementType.MoveOnly,
                    null);

                // eating diagonally
                potentialMoves.AddRange(
                    tryMove(tile.x, tile.y, 1, 1,
                        1, tiles, new List<Play>(),
                        MovementType.CaptureOnly,
                        null)
                );
                potentialMoves.AddRange(
                    tryMove(tile.x, tile.y, -1, 1,
                        1, tiles, new List<Play>(),
                        MovementType.CaptureOnly,
                        null)
                );

                break;
            case Piece.Type.Knight:
                potentialMoves = tryAll(knightMoves,
                    tile.x, tile.y,
                    1, tiles,
                    MovementType.MoveOrCapture,
                    null);
                break;
            default:
                return;
        }

        this.PotentialMoves = potentialMoves.ToList();

    }

    private List<Play> tryAll((int, int)[] directions, int currentX, int currentY, int maxMoves, Tile[,] tiles, MovementType movementType, Piece blocker)
    {
        var res = new List<Play>();
        foreach (var xy in directions)
        {
            res.AddRange(
                tryMove(currentX, currentY, xy.Item1, xy.Item2, maxMoves, tiles, new List<Play>(), movementType, blocker, null)
            );
        }

        return res;
    }

    // TODO simplify all the bools
    private List<Play> tryMove(int x, int y, int deltaX, int deltaY, int maxMoves, Tile[,] tiles, List<Play> validMoves, MovementType movementType, Piece blocker, Play prev = null)
    {
        // flip the y axis when piece is facing down
        int yFlip = facingUp ? 1 : -1;
        int newX = x + deltaX;
        int newY = y + (deltaY * yFlip);

        // we're done when hitting a corner or there's no more moves possible
        if (newX < 0 || newX >= tiles.GetLength(0)
              || newY < 0 || newY >= tiles.GetLength(1)
              || (maxMoves <= 0))
        {
            return validMoves;
        }

        var t = tiles[newX, newY];
        var canCapture = movementType == MovementType.CaptureOnly || movementType == MovementType.MoveOrCapture;

        // some pieces can only move to a position if there's something capturable there (or vice-versa)
        if ((movementType == MovementType.MoveOrCapture || movementType == MovementType.CaptureOnly) || // can do whatever or capture at destination (we'll check if the capture move is valid in a bit)
            (movementType == MovementType.MoveOnly && t.CurrentPiece == null)) // can only move when there's NOT a capturable piece (we short-circuit this instead of making it a "blocked move" bc this is a "non threatening" potential move
        {
            if (t.CurrentPiece != null && t.CurrentPiece.player == this.player)
            { // can't capture own pieces, but they can block the movement
                blocker = t.CurrentPiece;
            }
            
            // mark moves where the piece can only CAPTURE at the destination as blocked when there's nothing to capture there
            var blockedMove = blocker != null || (movementType == MovementType.CaptureOnly && t.CurrentPiece == null);

            var newMove = new Play(this, tiles[newX, newY], blocker, prev, canCapture, blockedMove);
            prev = newMove;
            validMoves.Add(newMove);
        }

        // can't move further when hitting another piece
        // but we compute the full motion anyway in case we get to a check
        if (t.CurrentPiece != null)
        {
            // already blocked by one piece OR by a piece the player owns, so no point on going further
            if (blocker != null)// || t.CurrentPiece.player == this.player)
            {
                return validMoves;
            }
            else
            {
                blocker = t.CurrentPiece;
            }
        }

        return tryMove(newX, newY, deltaX, deltaY, maxMoves - 1, tiles, validMoves, movementType, blocker, prev);
    }

    public List<Play> UnblockedMoves()
    {
        return this.PotentialMoves.FindAll(m => !m.BlockedMove);
    }
}
