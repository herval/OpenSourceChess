using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PieceType {
    Rook,
    King,
    Queen,
    Bishop,
    Knight,
    Pawn,
    CheckersPawn,
    CheckersKing,
}

public enum MoveType {
    Movement,
    Capture,
    Casteling,
    DoubleMove,
}

// lightweight representation of a piece, dettached from ui behaviors
public class Piece {
    public PieceType Type;
    public PlayerPosition StartingPosition;
    public Player Player;
    public bool MovedAtLeastOnce; // used for pawns' first move
    public Tile Tile;
    public int Value; // used for min/max computations https://en.wikipedia.org/wiki/Chess_piece_relative_value
    public bool IsKing; // used to identify LE ROI
    public List<Move> PotentialMoves = new List<Move>();
    public char FigurineAlgebraicNotation; // algebraic notation name
    public MoveType LastMoveType;

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

    static (int, int)[] TOP_DOUBLE_DIAGONALS = new (int, int)[] {
        (2, 2),
        (-2, 2),
    };

    static (int, int)[] TOP_DIAGONALS = new (int, int)[] {
        (1, 1),
        (-1, 1),
    };

    static (int, int)[] BOTTOM_DIAGONALS = new (int, int)[] {
        (-1, -1),
        (1, -1)
    };

    static (int, int)[] BOTTOM_DOUBLE_DIAGONALS = new (int, int)[] {
        (-2, -2),
        (2, -2)
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
        if (x + 3 < board.Pieces.GetLength(0)) {
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
                            null,
                            null,
                            null,
                            moveType: MoveType.Casteling,
                            blocked: false,
                            isFirstMove: !king.MovedAtLeastOnce,
                            isRewind: false,
                            connectedPlays: new List<Move>() {
                                new Move(
                                    board,
                                    rook,
                                    board.Tiles[x + 1, y],
                                    null,
                                    null,
                                    null,
                                    moveType: MoveType.Casteling,
                                    blocked: false,
                                    isFirstMove: !rook.MovedAtLeastOnce
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
                            null,
                            moveType: MoveType.Casteling,
                            false,
                            !king.MovedAtLeastOnce,
                            false,
                            new List<Move>() {
                                new Move(
                                    board,
                                    rook,
                                    board.Tiles[x - 1, y],
                                    null,
                                    null,
                                    captureAt: null,
                                    moveType: MoveType.Casteling,
                                    blocked: false,
                                    isFirstMove: !rook.MovedAtLeastOnce
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
                    .SelectMany(c => c.FindAll(m => m.TileCaptured != null))
                    .ToList()
                    .ConvertAll(p => p.TileTo);

                p.PotentialMoves = p.PotentialMoves.FindAll(m => !threatenedTiles.Contains(m.TileTo));

                // the king cannot capture a piece that would _unblock_ a check attempt
                // TODO this is not needed I think
                blockedCheckAttempts.ForEach(a => {
                    Debug.Log("Evaluating blocked threat by " + a.OwnPiece);
                    p.PotentialMoves = p.PotentialMoves.FindAll(m =>
                        m.CapturedPiece != a.CapturedPiece
                    );
                    ;
                });
            } else {
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
                    maxSquares,
                    board,
                    MovementType.MoveOnly,
                    null);

                potentialMoves.ForEach(m => {
                    // double jump!
                    if (Math.Abs(m.TileTo.Y - m.TileFrom.Y) > 1) {
                        m.MoveType = MoveType.DoubleMove;
                    }
                });

                // eating diagonally
                var captures = tryMove(Tile.X, Tile.Y, 1, 1,
                        1, board, new List<Move>(),
                        MovementType.CaptureOnly,
                        null);

                captures.AddRange(
                    tryMove(Tile.X, Tile.Y, -1, 1,
                        1, board, new List<Move>(),
                        MovementType.CaptureOnly,
                        null)
                );

                potentialMoves.AddRange(captures);

                potentialMoves.AddRange(EnPassantMoves(board, captures));
                break;
            case PieceType.Knight:
                potentialMoves = tryAll(KNIGHT_MOVES,
                    Tile.X, Tile.Y,
                    1, board,
                    MovementType.MoveOrCapture,
                    null);
                break;

            case PieceType.CheckersPawn:
                potentialMoves = CheckerMoves(board);

                break;
            default:
                return;
        }

        this.PotentialMoves = potentialMoves.ToList();
    }

    private IEnumerable<Move> EnPassantMoves(Board board, List<Move> currentCaptures) {
        // if there's a pawn that double-moved crossing his pawns diagonals, it's capturable
        var pieces = board.Pieces;

        var capturable = new List<Piece>();

        if (this.Tile.X > 0) {
            var neighbor = pieces[this.Tile.X - 1, this.Tile.Y];
            if (neighbor != null && neighbor.LastMoveType == MoveType.DoubleMove && neighbor.Player != this.Player) {
                capturable.Add(neighbor);
            }
        }

        if (this.Tile.X < pieces.GetLength(0) - 1) {
            var neighbor = pieces[this.Tile.X + 1, this.Tile.Y];
            if (neighbor != null && neighbor.LastMoveType == MoveType.DoubleMove && neighbor.Player != this.Player) {
                capturable.Add(neighbor);
            }
        }

        // one up or one down
        var dy = this.StartingPosition == PlayerPosition.Bottom ? 1 : -1;

        var potentialEnPassants = capturable.ConvertAll(p => new Move(
            board,
            this,
            board.Tiles[p.Tile.X, p.Tile.Y + dy],
            board.Pieces[p.Tile.X, p.Tile.Y],
            null,
            board.Tiles[p.Tile.X, p.Tile.Y],
            moveType: MoveType.Capture,
            false,
            !this.MovedAtLeastOnce,
            false,
            null)
        );

        // only count en passants when the pawn won't capture another piece at the destination
        var regularCapturedTiles = currentCaptures.FindAll(c => c.CapturedPiece != null).ConvertAll(c => c.TileTo);

        return potentialEnPassants.FindAll(m => !regularCapturedTiles.Contains(m.TileTo));
    }

    private List<Move> CheckerMoves(Board board) {
        // basic moves
        var moves = tryAll(
            TOP_DIAGONALS, // "forward" only
            Tile.X, Tile.Y,
            1, board,
            MovementType.MoveOnly,
            null);

        // capturing by jumping over
        (int, int)[] destinations;
        (int, int)[] captures;

        if (this.StartingPosition == PlayerPosition.Bottom) {
            destinations = TOP_DOUBLE_DIAGONALS;
            captures = TOP_DIAGONALS;
        } else {
            destinations = BOTTOM_DOUBLE_DIAGONALS;
            captures = BOTTOM_DIAGONALS;
        }

        var curX = Tile.X;
        var curY = Tile.Y;

        for (int i = 0; i < destinations.Length; i++) {
            var dest = destinations[i];
            var capt = captures[i];

            int dx = curX + dest.Item1;
            int dy = curY + dest.Item2;

            if (dx < 0 || dx >= board.Tiles.GetLength(0)) { // out of bounds, skip
                continue;
            }
            if (dy < 0 || dy >= board.Tiles.GetLength(1)) { // out of bounds, skip
                continue;
            }

            var cx = curX + capt.Item1;
            var cy = curY + capt.Item2;

            if (board.Pieces[dx, dy] == null) { // can jump here, nice
                if (board.Pieces[cx, cy] != null && board.Pieces[cx, cy].Player != this.Player) {
                    moves.Add(new Move(
                        board,
                        this,
                        board.Tiles[dx, dy],
                        board.Pieces[cx, cy],
                        null,
                        board.Tiles[cx, cy],
                        moveType: MoveType.Casteling,
                        false,
                        !this.MovedAtLeastOnce,
                        false,
                        null
                    ));
                }
            }

        }

        return moves;
    }

    private List<Move> tryAll((int, int)[] directions, int currentX, int currentY, int maxMoves, Board board,
        MovementType movementType, Piece blocker) {
        var res = new List<Move>();
        foreach (var xy in directions) {
            res.AddRange(
                tryMove(currentX, currentY, xy.Item1, xy.Item2, maxMoves, board, new List<Move>(), movementType, blocker, null)
            );
        }

        return res;
    }

    // TODO simplify all the bools
    private List<Move> tryMove(int x, int y, int deltaX, int deltaY, int maxMoves, Board board, List<Move> validMoves,
        MovementType movementType, Piece blocker, Move prev = null) {
        // flip the y axis when piece is facing down
        int yFlip = StartingPosition == PlayerPosition.Bottom ? 1 : -1;
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
            var tileTo = board.Tiles[newX, newY];
            var capturedPiece = board.Pieces[newX, newY];
            var isCapture = canCapture && capturedPiece != null;

            var newMove = new Move(
                board,
                this,
                tileTo,
                t,
                prev,
                isCapture ? tileTo : null, // only fill this out if there's an actual captured piece
                moveType: isCapture ? MoveType.Capture : MoveType.Movement,
                blocked: blockedMove,
                isFirstMove: !this.MovedAtLeastOnce);
            prev = newMove;
            validMoves.Add(newMove);
        }

        // can't move further when hitting another piece
        // but we compute the full motion anyway in case we get to a check
        if (t != null) {
            // already blocked by one piece OR by a piece the player owns, so no point on going further
            if (blocker != null) {
                return validMoves;
            } else {
                blocker = t;
            }
        }

        return tryMove(newX, newY, deltaX, deltaY, maxMoves - 1, board, validMoves, movementType, blocker, prev);
    }
}
