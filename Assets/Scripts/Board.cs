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

    public void Reset()
    {
        this.Tiles = TileFactory.Reset(this);
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
