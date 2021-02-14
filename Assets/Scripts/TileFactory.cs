using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TileFactory : MonoBehaviour
{
    public GameObject[] DarkTilePrefabs;
    public GameObject[] LightTilePrefabs;


    public TileView[,] Reset(int width, int height, BoardView board)
    {
        var tiles = new TileView[width, height];

        var tilesParent = board.TileSpace;

        var totalSize = tilesParent.transform.GetComponent<SpriteRenderer>().bounds.size;
        float tileSizeX = totalSize.x / ((float)width);
        float tileSizeY = totalSize.y / ((float)height);

        // offset the tiles half a tile to the right, taking the new scale in consideration, based on the actual width x height of the board
        var tileScale = DarkTilePrefabs[0].transform.localScale;
        var tileSize = DarkTilePrefabs[0].GetComponent<SpriteRenderer>().bounds.size;
        float newTileScaleX = 1f / (float) width; // wtf these numbers
        float newTileScaleY = 1f / (float) height;

        // if the board was always 8x8, we wouldn't need to rescale - this computes the new size so we can render in the right place
        float tileOffsetX = tileSize.x * (tileScale.x / newTileScaleX);
        float tileOffsetY = tileSize.y * (tileScale.y / newTileScaleY);

        // half a board offset since coords are in the center
        float offsetX = (totalSize.x / 2f) - (tileSizeX/2f) - (newTileScaleX);
        float offsetY = (totalSize.y / 2f) - (tileSizeY/2f) - (newTileScaleY);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // render locations relative to the scale of the tiles
                // TODO use the size of the board as bounds instead

                float px = (((float)x) * tileSizeX) - offsetX + tileOffsetX;
                float py = (((float)y) * tileSizeY) - offsetY + tileOffsetY; // this makes ZERO SENSE

                // if the square is double odd or double even, it‘s Dark (diagonals)
                var i = ((x + y) % 2 == 0) ? DarkTilePrefabs.Length : LightTilePrefabs.Length; // supports variety of tiles
                i = Random.Range(0, i - 1);
                var tilePrefab = ((x + y) % 2 == 0) ? DarkTilePrefabs[i] : LightTilePrefabs[i];

                // scale up tiles to fit
                var tileObj = Instantiate(tilePrefab,
                    new Vector3(px, py, 0f), 
                    Quaternion.identity, tilesParent.transform);
                tileObj.GetComponent<SpriteRenderer>().transform.localScale = new Vector3(newTileScaleX, newTileScaleY, 0f); // reescale for reals
                
                // alternate colors
                tiles[x, y] = tileObj.GetComponent<TileView>();
                tiles[x, y].Color = tilePrefab == DarkTilePrefabs[0] ? DarkTilePrefabs[i].GetComponent<TileView>().Color : LightTilePrefabs[i].GetComponent<TileView>().Color;

                tiles[x, y].X = x;
                tiles[x, y].Y = y;
            }
        }

        return tiles;
    }
}
