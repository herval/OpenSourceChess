using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool Highlighted = false; // mouse over
    public bool Selected = false; // tile where the piece about to move is
    internal bool PotentialMove = false; // available tiles to move to
    internal bool BlockedMove = false; // moves piece is not able to get to (shown only for debugging)
    public Color Color;

    public int x;
    public int y;

    // TODO get rid of this circular reference
    public Piece CurrentPiece; // TODO using this bc GetComponent introduces bugs when updating states (eg if u change the current piece in a given turn, recomputing moves wont happen)

    // Update is called once per frame
    void Update()
    {
        // TODO use <Renderer>.material

        if (Selected)
        {
            GetComponent<SpriteRenderer>().color = Color.red;
#if UNITY_EDITOR
} else if (BlockedMove) // for debugging only
        {
            GetComponent<SpriteRenderer>().color = Color.cyan;
#endif
        } else if (Highlighted)
        {
            GetComponent<SpriteRenderer>().color = Color.yellow;
        } else if (PotentialMove)
        {
            GetComponent<SpriteRenderer>().color = Color.green;
        } else 
        {
            GetComponent<SpriteRenderer>().color = Color;
        }
    }

}
