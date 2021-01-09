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

    public void SetPiece(Piece piece)
    {
        // destroy existing child piece
        var c = CurrentPiece();
        if (c != null && c != piece)
        {
            Debug.Log("Killing existing on " + this);
            Destroy(CurrentPiece().gameObject);
        }

        // TODO remove this and move it all to piece.SetTile(); instead
        piece.tile = this;
        piece.movedAtLeastOnce = true;
        piece.transform.parent = this.transform;
        piece.transform.position = this.transform.position; // TODO animate
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
