using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool Highlighted = false;
    public bool Selected = false;
    internal bool PotentialMove = false;
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
