using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Piece : MonoBehaviour
{

    public enum PieceType
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
    
    static (int, int)[] FORWARD = new (int, int)[] {
        (0, 1),
    };

    static (int, int)[] DIAGONALS = new (int, int)[] {
        (1, 1),
        (-1, -1),
        (-1, 1),
        (1, -1)
    };

    static (int, int)[] LINEARS = new (int, int)[] {
        (1, 0),
        (0, 1),
        (-1, 0),
        (0, -1)
    };

    static (int, int)[] ADJACENCIES = new (int, int)[] {
        (1, 0),
        (0, 1),
        (-1, 0),
        (0, -1),
        (1, 1),
        (-1, -1),
        (-1, 1),
        (1, -1)
    };

    static (int, int)[] KNIGHT_MOVES = new (int, int)[] {
        (2, 1),
        (2, -1),
        (-2, 1),
        (-2, -1),
        (1, 2),
        (1, -2),
        (-1, 2),
        (-1, -2)
    };

    [FormerlySerializedAs("pieceType")] [FormerlySerializedAs("type")] public PieceType Type;
    [FormerlySerializedAs("facingUp")] public bool FacingUp;
    [FormerlySerializedAs("color")] public Color Color;

    private Tile _tile;
    public Tile Tile
    {
        get
        {
            return _tile;
        }

        set
        {
            _tile = value;
            if (Sprite == null)
            {
                Sprite = this.GetComponent<SpriteRenderer>();
                if (Sprite == null)
                {
                    Sprite = this.GetComponentInChildren<SpriteRenderer>();
                }
                if (Sprite == null)
                {
                    throw new MissingReferenceException("A sprite is required!");
                }
            }

            // set the order in layer for the sprite
            Sprite.sortingOrder = -value.Y;
        }
    }

    [FormerlySerializedAs("player")] public Player Player;

    internal int Value; // used for min/max computations https://en.wikipedia.org/wiki/Chess_piece_relative_value

    internal bool IsKing; // used to identify LE ROI

    [FormerlySerializedAs("movedAtLeastOnce")] public bool MovedAtLeastOnce = false; // used for pawns' first move

    public List<Play> PotentialMoves = new List<Play>();
    [FormerlySerializedAs("sprite")] public SpriteRenderer Sprite;

    public static void ComputePotentialMoves(Tile[,] tiles, Player currentPlayer, Player opponent)
    {
        // Debug.Log("Computing moves for " + (currentPlayer.color == Color.white ? "white" : "black"));
        // at the beginning of a player turn, we have to cross-check a bunch of stuff

        // mark everything as movable to start
        currentPlayer.Pieces.ForEach(p => p.ResetMoves());
        opponent.Pieces.ForEach(p => p.ResetMoves());

        // compute all movement vectors of the _opponent_ pieces
        opponent.Pieces.ForEach((p) =>
        {
            p.ComputeMoves(tiles);
        });


        // find all the moves that threaten the king
        List<Play> checkMoves = opponent.Pieces.ConvertAll(p => p.PotentialMoves.FindAll(m => m.IsCheck() && !m.BlockedMove)).SelectMany(c => c).ToList();
        List<List<Tile>> attackVectors = checkMoves.ConvertAll(m => m.MovementVector());

        var blockedCheckAttempts = opponent.Pieces.ConvertAll(p =>
            p.PotentialMoves.FindAll(m => m.IsCheck() && m.BlockedMove)
        ).SelectMany(x => x).ToList();

        currentPlayer.InCheck = checkMoves.Count > 0;

        // compute all the movement vectors of current player's pieces
        // the king CANNOT move to a threatened tile
        currentPlayer.Pieces.ForEach(p =>
        {
            p.ComputeMoves(tiles);

            // le roi is a little snowflake
            if (p.IsKing)
            {
                // the king cannot move into traps!
                var threatenedTiles = opponent.Pieces
                        .ConvertAll(p => p.PotentialMoves)
                        .SelectMany(c => c.FindAll(m => m.CanCaptureAtDestination))
                        .ToList()
                        .ConvertAll(p => p.TileTo);
                
                p.PotentialMoves = p.PotentialMoves.FindAll(m => !threatenedTiles.Contains(m.TileTo));

                // the king cannot capture a piece that would _unblock_ a check attempt
                // TODO this is not needed I think
                blockedCheckAttempts.ForEach(a =>
                {
                    Debug.Log("Evaluating blocked threat by " + a.OwnPiece);
                    p.PotentialMoves = p.PotentialMoves.FindAll(m =>
                        m.PieceAtDestination != a.PieceAtDestination
                    ); ;
                });
            }
            else
            {
                // if there's any directly threatening the king, current player is in check and can only defend
                // enable only movements that will either block *all* attack vector or capture threatening pieces
                attackVectors.ForEach(a =>
                {
                    p.PotentialMoves = p.PotentialMoves.FindAll(m => a.Contains(m.TileTo));
                });
            }
        });


        // pieces BLOCKING an opponent may only move as long as they remain in the blocking path or capture the attacker
        blockedCheckAttempts.ForEach(checkAttempt =>
            {
                var vector = checkAttempt.MovementVector().Skip(1).ToList(); // skip the check position itself
                var blockingPieces = vector
                    .FindAll(t => t.CurrentPiece?.Player == currentPlayer)
                    .ConvertAll(t => t.CurrentPiece); // grab all player pieces blocking the move

                if (blockingPieces.Count == 1) // only restrict movement if there's only ONE piece defending the king
                {
                    blockingPieces[0].PotentialMoves = blockingPieces[0].PotentialMoves.FindAll(m => vector.Contains(m.TileTo));
                }
            });
    }
    
    public void Freeze()
    {
        setAnimated(false);
    }

    public void Unfreeze()
    {
        setAnimated(true);
    }

    private void setAnimated(bool animated)
    {
        var animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.enabled = animated;
        }
    }

    internal void ResetMoves()
    {
        this.PotentialMoves.Clear();
    }

    public Play UnblockedMoveTo(Tile tile)
    {
        return this.PotentialMoves.Find(m => m.TileTo == tile && !m.BlockedMove);
    }

    internal void ComputeMoves(Tile[,] tiles)
    {
        List<Play> potentialMoves;

        switch (Type)
        {
            case PieceType.Bishop:
                // verticals until hitting something
                potentialMoves = tryAll(DIAGONALS,
                    Tile.X, Tile.Y,
                    int.MaxValue, tiles,
                    MovementType.MoveOrCapture,
                    null);
                break;
            case PieceType.King:
                potentialMoves = tryAll(ADJACENCIES,
                    Tile.X, Tile.Y, 1, tiles,
                    MovementType.MoveOrCapture,
                    null);

                // find the rooks
                potentialMoves.AddRange(CastelingMoves(this, tiles));
                
                break;
            case PieceType.Queen:
                potentialMoves = tryAll(ADJACENCIES,
                    Tile.X, Tile.Y,
                    int.MaxValue, tiles,
                    MovementType.MoveOrCapture,
                    null);
                break;
            case PieceType.Rook:
                // straight lines until hitting an adversary
                potentialMoves = tryAll(LINEARS,
                    Tile.X, Tile.Y,
                    int.MaxValue, tiles,
                    MovementType.MoveOrCapture,
                    null);
                break;
            case PieceType.Pawn:
                // if first move, can move one or two squares
                int maxSquares = MovedAtLeastOnce ? 1 : 2;

                potentialMoves = tryAll(FORWARD,
                    Tile.X, Tile.Y,
                    maxSquares, tiles,
                    MovementType.MoveOnly,
                    null);

                // eating diagonally
                potentialMoves.AddRange(
                    tryMove(Tile.X, Tile.Y, 1, 1,
                        1, tiles, new List<Play>(),
                        MovementType.CaptureOnly,
                        null)
                );
                potentialMoves.AddRange(
                    tryMove(Tile.X, Tile.Y, -1, 1,
                        1, tiles, new List<Play>(),
                        MovementType.CaptureOnly,
                        null)
                );

                break;
            case PieceType.Knight:
                potentialMoves = tryAll(KNIGHT_MOVES,
                    Tile.X, Tile.Y,
                    1, tiles,
                    MovementType.MoveOrCapture,
                    null);
                break;
            default:
                return;
        }

        this.PotentialMoves = potentialMoves.ToList();

    }

    private List<Play> CastelingMoves(Piece king, Tile[,] tiles)
    {
        var moves = new List<Play>(); 
        if (king.MovedAtLeastOnce)
        {
            return moves;
        }
        
        // TODO dedup this crap
        
        // 2 unblocked to the right
        var x = king.Tile.X;
        var y = king.Tile.Y;
        if (x + 3 < tiles.Length)
        {
            var rook = tiles[x + 3, y]?.CurrentPiece;
            if (rook != null)
            {
                if (tiles[x + 1, y].CurrentPiece == null &&
                    tiles[x + 2, y].CurrentPiece == null &&
                    rook.Type == PieceType.Rook && !rook.MovedAtLeastOnce
                )
                {
                    moves.Add(
                        new Play(
                            king,
                            tiles[x + 2, y], 
                            null, 
                            null, 
                            false, 
                            false,
                            !king.MovedAtLeastOnce,
                            false,
                            new List<Play>()
                            {
                                new Play(rook, 
                                    tiles[x + 1, y], 
                                    null, 
                                    null, 
                                    false, 
                                    false,
                                    !rook.MovedAtLeastOnce)
                            })
                    );
                }
            }
        }

        // 3 unblocked to the left
        x = king.Tile.X;
        y = king.Tile.Y;
        if (x - 4 >= 0)
        {
            var rook = tiles[x - 4, y]?.CurrentPiece;
            if (rook != null)
            {
                if (tiles[x - 1, y].CurrentPiece == null &&
                    tiles[x - 2, y].CurrentPiece == null &&
                    tiles[x - 3, y].CurrentPiece == null &&
                    rook.Type == PieceType.Rook && !rook.MovedAtLeastOnce
                )
                {
                    moves.Add(
                        new Play(
                            king, 
                            tiles[x - 2, y], 
                            null, 
                            null, 
                            false,
                            false,
                            !king.MovedAtLeastOnce,
                            false,
                            new List<Play>()
                            {
                                new Play(
                                    rook, 
                                    tiles[x - 1, y], 
                                    null, 
                                    null, 
                                    false, 
                                    false,
                                    !rook.MovedAtLeastOnce)
                            })
                    );
                }
            }
        }

        return moves;
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
        int yFlip = FacingUp ? 1 : -1;
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
            if (t.CurrentPiece != null && t.CurrentPiece.Player == this.Player)
            { // can't capture own pieces, but they can block the movement
                blocker = t.CurrentPiece;
            }
            
            // mark moves where the piece can only CAPTURE at the destination as blocked when there's nothing to capture there
            var blockedMove = blocker != null || (movementType == MovementType.CaptureOnly && t.CurrentPiece == null);

            var newMove = new Play(
                this, 
                tiles[newX, newY], 
                t.CurrentPiece, 
                prev, 
                canCapture, 
                blockedMove,
                !this.MovedAtLeastOnce);
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

    public void SetTile(Tile tile, bool skipAnimation, AfterAnimationCallback done)
    {
        tile.CurrentPiece = this;

        this.Tile.CurrentPiece = null; // remove ref from tile so piece doesn't show in two places at the same time
        this.Tile = tile;
        this.transform.parent = tile.transform;
        if (skipAnimation)
        {
            this.transform.position = tile.transform.position;
            done?.Invoke(true);
        }
        else
        {
            // TODO is this efficient?
            StartCoroutine(
                AnimationHelper.MoveOverSeconds(
                    this.gameObject, 
                    this.Tile.transform.position, 
                    0.2f,
                    done)); 
        }
        
        
    }
}
