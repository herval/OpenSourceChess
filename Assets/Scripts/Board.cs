using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

public class Board : MonoBehaviour
{
    [FormerlySerializedAs("width")] public int Width = 10;
    [FormerlySerializedAs("height")] public int Height = 10; // 10 as deault so a single tile has a scale of 0.1

    // TODO make these a single thing
    [FormerlySerializedAs("tileFactory")] public TileFactory TileFactory;
    [FormerlySerializedAs("pieceFactory")] public PieceFactory PieceFactory;

    public Tile[,] Tiles;

    public void AddPiece(Piece.PieceType piece, int x, int y, Player player)
    {
        Piece p = PieceFactory.Create(Tiles[x, y], piece, player.FacingUp, player);
        player.Pieces.Add(p);

        p.transform.position = Tiles[x, y].transform.position;
    }

    public void ComputePotentialMoves(Player currentPlayer, Player opponent)
    {
        // Debug.Log("Computing moves for " + (currentPlayer.color == Color.white ? "white" : "black"));
        // at the beginning of a player turn, we have to cross-check a bunch of stuff

        // mark everything as movable to start
        currentPlayer.Pieces.ForEach(p => p.ResetMoves());
        opponent.Pieces.ForEach(p => p.ResetMoves());

        // compute all movement vectors of the _opponent_ pieces
        opponent.Pieces.ForEach((p) =>
        {
            p.ComputeMoves(Tiles);
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
            p.ComputeMoves(Tiles);

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

    public void Reset()
    {
        this.Tiles = TileFactory.Reset(this);
    }

    internal Tile TileAt(int x, int y)
    {
        return Tiles[x, y];
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireCube(
        //    new Vector3(0, 0, 0),
        //    new Vector3(width * tilePrefab.transform.localScale.x, height * tilePrefab.transform.localScale.y, 0)
        //);

        // TODO draw the board wire
        //float tw= width / tilePrefab.transform.localScale.x;
        //float th = height / tilePrefab.transform.localScale.y;
        //Vector3 size = new Vector3(tw, th);

        //for (int x = 0; x < tiles.GetLength(0); x++)
        //{
        //    for (int y = 0; y < tiles.GetLength(1); y++)
        //    {
        //        Vector3 pos = new Vector3(x * tw, y * th);
        //        Gizmos.DrawWireCube(
        //            pos,
        //            size
        //        );
        //        Debug.Log("here");
        //    }
        //}
    }
}
