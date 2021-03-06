using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


public class TileView : MonoBehaviour {
    public bool Highlighted = false; // mouse over
    public bool Selected = false; // tile where the piece about to move is
    internal bool PotentialMove = false; // available tiles to move to
    internal bool BlockedMove = false; // moves piece is not able to get to (shown only for debugging)
    internal bool Threatened = false; // is there a piece about to be captured here?
    public Color Color;

    public PieceView CurrentPiece;

    public Tile State = new Tile(0, 0);

    public int X {
        get {
            return State.X;
        }
        set {
            State.X = value;
        }
    }

    public int Y {
        get {
            return State.Y;
        }

        set {
            State.Y = value;
        }
    }


    // Update is called once per frame
    void Update() {
        // TODO use <Renderer>.material

        if (Selected || Threatened) {
            GetComponent<SpriteRenderer>().color = Color.red;
#if UNITY_EDITOR
} else if (BlockedMove) // for debugging only
        {
            GetComponent<SpriteRenderer>().color = Color.cyan;
#endif
        } else if (Highlighted) {
            GetComponent<SpriteRenderer>().color = Color.yellow;
        } else if (PotentialMove) {
            GetComponent<SpriteRenderer>().color = Color.green;
        } else {
            GetComponent<SpriteRenderer>().color = Color;
        }
    }

    public static Tile[,] ToTiles(TileView[,] tileViews) {
        Tile[,] tiles = new Tile[tileViews.GetLength(0), tileViews.GetLength(1)];
        for (int x = 0; x < tileViews.GetLength(0); x++) {
            for (int y = 0; y < tileViews.GetLength(1); y++) {
                tiles[x, y] = tileViews[x, y].State;
            }
        }

        return tiles;

    }
}
