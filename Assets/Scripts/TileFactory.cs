using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TileFactory : MonoBehaviour
{
    [FormerlySerializedAs("darkTilePrefabs")] public GameObject[] DarkTilePrefabs;
    [FormerlySerializedAs("lightTilePrefabs")] public GameObject[] LightTilePrefabs;


    public Tile[,] Reset(Board board)
    {
        var tiles = new Tile[board.Width, board.Height];

        var totalSize = board.transform.GetComponent<SpriteRenderer>().bounds.size;
        float tileSizeX = totalSize.x / ((float)board.Width);
        float tileSizeY = totalSize.y / ((float)board.Height);

        // half a board offset since coords are in the center
        float offsetX = totalSize.x / 2f;
        float offsetY = totalSize.y / 2f;

        // offset the tiles half a tile to the right, taking the new scale in consideration, based on the actual width x height of the board
        var tileScale = DarkTilePrefabs[0].transform.localScale;
        var tileSize = DarkTilePrefabs[0].GetComponent<SpriteRenderer>().bounds.size;
        float newTileScaleX = 1f / board.Width;
        float newTileScaleY = 1f / board.Height;

        // if the board was always 8x8, we wouldn't need to rescale - this computes the new size so we can render in the right place
        float tileOffsetX = ((tileSize.x / tileScale.x) * newTileScaleX) / 2f;
        float tileOffsetY = ((tileSize.y / tileScale.y) * newTileScaleY) / 2f;

        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                // render locations relative to the scale of the tiles
                // TODO use the size of the board as bounds instead

                float px = (((float)x) * tileSizeX) - offsetX + tileOffsetX;
                float py = ((((float)y) + 1f) * tileSizeY) - offsetY + tileOffsetY; // this makes ZERO SENSE

                // if the square is double odd or double even, it‘s Dark (diagonals)
                var i = ((x + y) % 2 != 0) ? DarkTilePrefabs.Length : LightTilePrefabs.Length; // supports variety of tiles
                i = Random.Range(0, i - 1);
                var tilePrefab = ((x + y) % 2 != 0) ? DarkTilePrefabs[i] : LightTilePrefabs[i];

                // scale up tiles to fit
                var tileObj = Instantiate(tilePrefab,
                    new Vector3(px, py, 0f), // offset rendering by half a board + half a tile, since coords start on the center
                    Quaternion.identity, board.transform);
                tileObj.GetComponent<SpriteRenderer>().transform.localScale = new Vector3(newTileScaleX, newTileScaleY, 0f); // reescale for reals

                // alternate colors
                tiles[x, y] = tileObj.GetComponent<Tile>();
                tiles[x, y].Color = tilePrefab == DarkTilePrefabs[0] ? DarkTilePrefabs[i].GetComponent<Tile>().Color : LightTilePrefabs[i].GetComponent<Tile>().Color;

                tiles[x, y].X = x;
                tiles[x, y].Y = y;
            }
        }

        // if we dont do this, the prototype tile on the corner will be clickable :fliptable
        //tilePrefab.GetComponent<BoxCollider>().enabled = false;

        return tiles;
    }
}
