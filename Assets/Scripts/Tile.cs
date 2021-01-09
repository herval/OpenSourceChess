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

    public Piece CurrentPiece()
    {
        return GetComponentInChildren<Piece>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Piece);

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
