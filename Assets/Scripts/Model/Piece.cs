using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// lightweight representation of a piece, dettached from ui behaviors
public class Piece {
    public PieceType Type;
    public bool FacingUp;
    public Player Player;
    public bool MovedAtLeastOnce; // used for pawns' first move
    public Tile Tile;
    public int Value; // used for min/max computations https://en.wikipedia.org/wiki/Chess_piece_relative_value
    public bool IsKing; // used to identify LE ROI
    public List<Move> PotentialMoves = new List<Move>();
    public char FigurineAlgebraicNotation; // algebraic notation name

    private enum MovementType {
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


    private List<Move> CastelingMoves(Piece king, Board board) {
        var moves = new List<Move>();
        if (king.MovedAtLeastOnce) {
            return moves;
        }

        // TODO dedup this crap

        // 2 unblocked to the right
        var x = king.Tile.X;
        var y = king.Tile.Y;
        if (x + 3 < board.Pieces.Length) {
            var rook = board.Pieces[x + 3, y];
            if (rook != null) {
                if (board.Pieces[x + 1, y] == null &&
                    board.Pieces[x + 2, y] == null &&
                    rook?.Type == PieceType.Rook && !rook.MovedAtLeastOnce
                ) {
                    moves.Add(
                        new Move(
                            board,
                            moving: king,
                            board.Tiles[x + 2, y],
                            pieceAtDestination: null,
                            null,
                            false,
                            false,
                            !king.MovedAtLeastOnce,
                            false,
                            isCasteling: true,
                            connectedPlays: new List<Move>() {
                                new Move(
                                    board,
                                    rook,
                                    board.Tiles[x + 1, y],
                                    null,
                                    null,
                                    false,
                                    false,
                                    !rook.MovedAtLeastOnce,
                                    isCasteling: true
                                )
                            })
                    );
                }
            }
        }

        // 3 unblocked to the left
        x = king.Tile.X;
        y = king.Tile.Y;
        if (x - 4 >= 0) {
            var rook = board.Pieces[x - 4, y];
            if (rook != null) {
                if (board.Pieces[x - 1, y] == null &&
                    board.Pieces[x - 2, y] == null &&
                    board.Pieces[x - 3, y] == null &&
                    rook.Type == PieceType.Rook && !rook.MovedAtLeastOnce
                ) {
                    moves.Add(
                        new Move(
                            board,
                            king,
                            board.Tiles[x - 2, y],
                            null,
                            null,
                            false,
                            false,
                            !king.MovedAtLeastOnce,
                            false,
                            isCasteling: true,
                            new List<Move>() {
                                new Move(
                                    board,
                                    rook,
                                    board.Tiles[x - 1, y],
                                    null,
                                    null,
                                    false,
                                    false,
                                    !rook.MovedAtLeastOnce,
                                    isCasteling: true
                                )
                            })
                    );
                }
            }
        }

        return moves;
    }

    internal void ResetMoves() {
        this.PotentialMoves.Clear();
    }

    public static void ComputePotentialMoves(Board board, Player currentPlayer, Player opponent) {
        var pieces = board.Pieces;
        // Debug.Log("Computing moves for " + (currentPlayer.color == Color.white ? "white" : "black"));
        // at the beginning of a player turn, we have to cross-check a bunch of stuff

        // mark everything as movable to start
        currentPlayer.Pieces.ForEach(p => p.ResetMoves());
        opponent.Pieces.ForEach(p => p.ResetMoves());

        // compute all movement vectors of the _opponent_ pieces
        opponent.Pieces.ForEach((p) => { p.ComputeMoves(board); });


        // find all the moves that threaten the king
        List<Move> checkMoves = opponent.Pieces
            .ConvertAll(p => p.PotentialMoves.FindAll(m => m.IsCheck() && !m.BlockedMove)).SelectMany(c => c).ToList();
        List<List<Tile>> attackVectors = checkMoves.ConvertAll(m => m.MovementVector());

        var blockedCheckAttempts = opponent.Pieces.ConvertAll(p =>
            p.PotentialMoves.FindAll(m => m.IsCheck() && m.BlockedMove)
        ).SelectMany(x => x).ToList();

        currentPlayer.InCheck = checkMoves.Count > 0;

        // compute all the movement vectors of current player's pieces
        // the king CANNOT move to a threatened tile
        currentPlayer.Pieces.ForEach(p => {
            p.ComputeMoves(board);

            // le roi is a little snowflake
            if (p.IsKing) {
                // the king cannot move into traps!
                var threatenedTiles = opponent.Pieces
                    .ConvertAll(p => p.PotentialMoves)
                    .SelectMany(c => c.FindAll(m => m.CanCaptureAtDestination))
                    .ToList()
                    .ConvertAll(p => p.TileTo);

                p.PotentialMoves = p.PotentialMoves.FindAll(m => !threatenedTiles.Contains(m.TileTo));

                // the king cannot capture a piece that would _unblock_ a check attempt
                // TODO this is not needed I think
                blockedCheckAttempts.ForEach(a => {
                    Debug.Log("Evaluating blocked threat by " + a.OwnPiece);
                    p.PotentialMoves = p.PotentialMoves.FindAll(m =>
                        m.PieceAtDestination != a.PieceAtDestination
                    );
                    ;
                });
            }
            else {
                // if there's any directly threatening the king, current player is in check and can only defend
                // enable only movements that will either block *all* attack vector or capture threatening pieces
                attackVectors.ForEach(a => { p.PotentialMoves = p.PotentialMoves.FindAll(m => a.Contains(m.TileTo)); });
            }
        });


        // pieces BLOCKING an opponent may only move as long as they remain in the blocking path or capture the attacker
        blockedCheckAttempts.ForEach(checkAttempt => {
            var vector = checkAttempt.MovementVector().Skip(1).ToList(); // skip the check position itself
            var blockingPieces = vector
                .FindAll(t => pieces[t.X, t.Y]?.Player == currentPlayer)
                .ConvertAll(t => pieces[t.X, t.Y]); // grab all player pieces blocking the move

            if (blockingPieces.Count == 1) // only restrict movement if there's only ONE piece defending the king
            {
                blockingPieces[0].PotentialMoves =
                    blockingPieces[0].PotentialMoves.FindAll(m => vector.Contains(m.TileTo));
            }
        });
    }

    internal void ComputeMoves(Board board) {
        List<Move> potentialMoves;

        switch (Type) {
            case PieceType.Bishop:
                // verticals until hitting something
                potentialMoves = tryAll(DIAGONALS,
                    Tile.X, Tile.Y,
                    int.MaxValue, board,
                    MovementType.MoveOrCapture,
                    null);
                break;
            case PieceType.King:
                potentialMoves = tryAll(ADJACENCIES,
                    Tile.X, Tile.Y, 1, board,
                    MovementType.MoveOrCapture,
                    null);

                // find the rooks
                potentialMoves.AddRange(CastelingMoves(this, board));

                break;
            case PieceType.Queen:
                potentialMoves = tryAll(ADJACENCIES,
                    Tile.X, Tile.Y,
                    int.MaxValue, board,
                    MovementType.MoveOrCapture,
                    null);
                break;
            case PieceType.Rook:
                // straight lines until hitting an adversary
                potentialMoves = tryAll(LINEARS,
                    Tile.X, Tile.Y,
                    int.MaxValue, board,
                    MovementType.MoveOrCapture,
                    null);
                break;
            case PieceType.Pawn:
                // if first move, can move one or two squares
                int maxSquares = MovedAtLeastOnce ? 1 : 2;

                potentialMoves = tryAll(FORWARD,
                    Tile.X, Tile.Y,
                    maxSquares, board,
                    MovementType.MoveOnly,
                    null);

                // eating diagonally
                potentialMoves.AddRange(
                    tryMove(Tile.X, Tile.Y, 1, 1,
                        1, board, new List<Move>(),
                        MovementType.CaptureOnly,
                        null)
                );
                potentialMoves.AddRange(
                    tryMove(Tile.X, Tile.Y, -1, 1,
                        1, board, new List<Move>(),
                        MovementType.CaptureOnly,
                        null)
                );

                break;
            case PieceType.Knight:
                potentialMoves = tryAll(KNIGHT_MOVES,
                    Tile.X, Tile.Y,
                    1, board,
                    MovementType.MoveOrCapture,
                    null);
                break;
            default:
                return;
        }

        this.PotentialMoves = potentialMoves.ToList();
    }

    private List<Move> tryAll((int, int)[] directions, int currentX, int currentY, int maxMoves, Board board,
        MovementType movementType, Piece blocker) {
        var res = new List<Move>();
        foreach (var xy in directions) {
            res.AddRange(
                tryMove(currentX, currentY, xy.Item1, xy.Item2, maxMoves, board, new List<Move>(), movementType,
                    blocker, null)
            );
        }

        return res;
    }

    // TODO simplify all the bools
    private List<Move> tryMove(int x, int y, int deltaX, int deltaY, int maxMoves, Board board, List<Move> validMoves,
        MovementType movementType, Piece blocker, Move prev = null) {
        // flip the y axis when piece is facing down
        int yFlip = FacingUp ? 1 : -1;
        int newX = x + deltaX;
        int newY = y + (deltaY * yFlip);

        // we're done when hitting a corner or there's no more moves possible
        if (newX < 0 || newX >= board.Pieces.GetLength(0)
                     || newY < 0 || newY >= board.Pieces.GetLength(1)
                     || (maxMoves <= 0)) {
            return validMoves;
        }

        var t = board.Pieces[newX, newY];
        var canCapture = movementType == MovementType.CaptureOnly || movementType == MovementType.MoveOrCapture;

        // some pieces can only move to a position if there's something capturable there (or vice-versa)
        if ((movementType == MovementType.MoveOrCapture ||
             movementType == MovementType.CaptureOnly
            ) || // can do whatever or capture at destination (we'll check if the capture move is valid in a bit)
            (movementType == MovementType.MoveOnly && t == null)
        ) // can only move when there's NOT a capturable piece (we short-circuit this instead of making it a "blocked move" bc this is a "non threatening" potential move
        {
            if (t != null && t.Player == this.Player) {
                // can't capture own pieces, but they can block the movement
                blocker = t;
            }

            // mark moves where the piece can only CAPTURE at the destination as blocked when there's nothing to capture there
            var blockedMove = blocker != null || (movementType == MovementType.CaptureOnly && t == null);

            var newMove = new Move(
                board,
                this,
                board.Tiles[newX, newY],
                t,
                prev,
                canCaptureAtDestination: canCapture,
                blocked: blockedMove,
                isFirstMove: !this.MovedAtLeastOnce);
            prev = newMove;
            validMoves.Add(newMove);
        }

        // can't move further when hitting another piece
        // but we compute the full motion anyway in case we get to a check
        if (t != null) {
            // already blocked by one piece OR by a piece the player owns, so no point on going further
            if (blocker != null) // || t.CurrentPiece.player == this.player)
            {
                return validMoves;
            }
            else {
                blocker = t;
            }
        }

        return tryMove(newX, newY, deltaX, deltaY, maxMoves - 1, board, validMoves, movementType, blocker, prev);
    }
}


public enum PieceType {
    Rook,
    King,
    Queen,
    Bishop,
    Knight,
    Pawn,
}