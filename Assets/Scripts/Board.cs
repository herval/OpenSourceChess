using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{

    public int width = 10;
    public int height = 10; // 10 as deault so a single tile has a scale of 0.1

    public GameObject tilePrefab;

    public PieceFactory pieceFactory;

    public Color darkTiles;
    public Color clearTiles;

    Tile[,] tiles;

    public void AddPiece(Piece.Type piece, int x, int y, Player player)
    {
        Piece p = pieceFactory.Create(tiles[x,y], piece, player.facingUp, player);
        player.Pieces.Add(p);

        p.transform.position = tiles[x, y].transform.position;
    }

    public void ComputePotentialMoves(Player currentPlayer, Player opponent)
    {
        Debug.Log("Computing moves for " + (currentPlayer.color == Color.white ? "white" : "black"));
        // at the beginning of a player turn, we have to cross-check a bunch of stuff

        // mark everything as movable to start
        currentPlayer.Pieces.ForEach(p => p.ResetMoves());
        opponent.Pieces.ForEach(p => p.ResetMoves());

        // compute all movement vectors of the _opponent_ pieces
        opponent.Pieces.ForEach((p) => {
            p.ComputeMoves(tiles);
        });


        // find all the moves that threaten the king
        List<Play> checkMoves = opponent.Pieces.ConvertAll(p => p.PotentialMoves.FindAll(m => m.isCheck() && m.Blocker == null)).SelectMany(c => c).ToList();
        List<List<Tile>> attackVectors = checkMoves.ConvertAll(m => m.MovementVector());

        var blockedCheckAttempts = opponent.Pieces.ConvertAll(p =>
            p.PotentialMoves.FindAll(m => m.isCheck() && m.Blocker != null)
        ).SelectMany(x => x).ToList();


        // compute all the movement vectors of current player's pieces
        // the king CANNOT move to a threatened tile
        currentPlayer.Pieces.ForEach(p => {
        p.ComputeMoves(tiles);

            // le roi is a little snowflake
            if (p.isKing)
            {
                // the king cannot move into traps!
                var threatenedTiles = opponent.Pieces
                    .ConvertAll(p => p.PotentialMoves)
                    .SelectMany(c => c)
                    .ToList()
                    .FindAll(p => p.Blocker == null)
                    .ConvertAll(p => p.Tile)
                    .ToList();
                p.PotentialMoves = p.PotentialMoves.FindAll(m => !threatenedTiles.Contains(m.Tile));

                // the king cannot capture a piece that would _unblock_ a check attempt
                blockedCheckAttempts.ForEach(a =>
                {
                    Debug.Log("Evaluating blocked threat by " + a.ownPiece);
                    p.PotentialMoves = p.PotentialMoves.FindAll(m =>
                        m.pieceOnTile != a.Blocker
                    ); ;
                });
            }
            else
            {
                // if there's any directly threatening the king, current player is in check and can only defend
                // enable only movements that will either block *all* attack vector or capture threatening pieces
                attackVectors.ForEach(a =>
                {
                    p.PotentialMoves = p.PotentialMoves.FindAll(m => a.Contains(m.Tile));
                });
            }
        });


        // pieces BLOCKING an opponent may only move as long as they remain in the blocking path or capture the attacker
        blockedCheckAttempts.ForEach(checkAttempt =>
            {
                var vector = checkAttempt.MovementVector().Skip(1).ToList(); // skip the check position itself
                var blockingPieces = vector
                    .FindAll(t => t.CurrentPiece?.player == currentPlayer)
                    .ConvertAll(t => t.CurrentPiece); // grab all player pieces blocking the move

                if(blockingPieces.Count == 1) // only restrict movement if there's only ONE piece defending the king
                {
                    blockingPieces[0].PotentialMoves = blockingPieces[0].PotentialMoves.FindAll(m => vector.Contains(m.Tile));
                }
            });
    }

    public void Reset()
    {
        Color nextColor;

        tiles = new Tile[width, height];

        var totalSize = this.transform.GetComponent<SpriteRenderer>().bounds.size;
        float tileSizeX = totalSize.x / ((float)width);
        float tileSizeY = totalSize.y / ((float)height);

        // half a board offset since coords are in the center
        float offsetX = totalSize.x / 2f;
        float offsetY = totalSize.y / 2f;

        // offset the tiles half a tile to the right, taking the new scale in consideration, based on the actual width x height of the board
        var tileScale = tilePrefab.transform.localScale;
        var tileSize = tilePrefab.GetComponent<SpriteRenderer>().bounds.size;
        float newTileScaleX = 1f / width;
        float newTileScaleY = 1f / height;

        // if the board was always 8x8, we wouldn't need to rescale - this computes the new size so we can render in the right place
        float tileOffsetX = ((tileSize.x / tileScale.x) * newTileScaleX) / 2f;
        float tileOffsetY = ((tileSize.y / tileScale.y) * newTileScaleY) / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // render locations relative to the scale of the tiles
                // TODO use the size of the board as bounds instead

                float px = (((float)x) * tileSizeX) - offsetX + tileOffsetX;
                float py = ((((float)y) + 1f) * tileSizeY) - offsetY + tileOffsetY; // this makes ZERO SENSE

                // scale up tiles to fit
                var tileObj = Instantiate(tilePrefab,
                    new Vector3(px, py, 0f), // offset rendering by half a board + half a tile, since coords start on the center
                    Quaternion.identity, this.transform);
                tileObj.GetComponent<SpriteRenderer>().transform.localScale = new Vector3(newTileScaleX, newTileScaleY, 0f); // reescale for reals


                // if the square is double odd or double even, it‘s Dark (diagonals)
                if ((x+y) % 2 != 0)
                {
                    nextColor = darkTiles;
                }
                else
                {
                    nextColor = clearTiles;
                }

                // alternate colors
                tiles[x, y] = tileObj.GetComponent<Tile>();
                tiles[x, y].Color = nextColor;

                tiles[x, y].x = x;
                tiles[x, y].y = y;
            }
        }

        // if we dont do this, the prototype tile on the corner will be clickable :fliptable
        tilePrefab.GetComponent<BoxCollider>().enabled = false;
    }

    internal Tile[,] Tiles()
    {
        return tiles;
    }

    internal Tile TileAt(int x, int y)
    {
        return tiles[x, y];
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(
            new Vector3(0, 0, 0),
            new Vector3(width * tilePrefab.transform.localScale.x, height * tilePrefab.transform.localScale.y, 0)
        );

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
