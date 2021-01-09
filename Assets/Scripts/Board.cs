using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{

    public int width = 10;
    public int height = 10; // 10 as deault so a single tile has a scale of 0.1

    public GameObject tilePrefab;

    public PieceFactory pieceFactory;

    public Color darkTiles;
    public Color clearTiles;

    Tile[,] tiles;
    

    public void AddPiece(Piece.Type piece, bool facingUp, int x, int y, Color color)
    {
        GameObject p = pieceFactory.Create(tiles[x,y].transform, piece, facingUp, color);

        p.transform.position = tiles[x, y].transform.position;
    }

    public void ComputePotentialMoves()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y].CurrentPiece()?.ComputeMoves(x, y, tiles);
            }
        }
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
                tileObj.GetComponent<Tile>().Color = nextColor;
                tiles[x, y] = tileObj.GetComponent<Tile>();
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
