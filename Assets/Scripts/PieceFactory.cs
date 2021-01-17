using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// creates pieces & manages their assets
public class PieceFactory : MonoBehaviour
{

    public GameObject WhiteRook;
    public GameObject WhiteKing;
    public GameObject WhiteQueen;
    public GameObject WhiteBishop;
    public GameObject WhitePawn;
    public GameObject WhiteKnight;

    public GameObject BlackRook;
    public GameObject BlackKing;
    public GameObject BlackQueen;
    public GameObject BlackBishop;
    public GameObject BlackPawn;
    public GameObject BlackKnight;


    public Piece Create(Tile tile, Piece.Type type, bool faceUp, Player player)
    {
        GameObject prefab = null;
        int value = 0;
        bool isKing = false;


        // TODO decouple facing from piece color
        switch (type)
        {
            case Piece.Type.Rook:
                value = 50;
                prefab = faceUp ? WhiteRook : BlackRook;
                break;

            case Piece.Type.Bishop:
                value = 30;
                prefab = faceUp ? WhiteBishop : BlackBishop;
                break;
            case Piece.Type.King:
                value = 900;
                isKing = true;
                prefab = faceUp ? WhiteKing : BlackKing;
                break;
            case Piece.Type.Queen:
                value = 90;
                prefab = faceUp ? WhiteQueen : BlackQueen;
                break;
            case Piece.Type.Pawn:
                value = 10;
                prefab = faceUp ? WhitePawn : BlackPawn;
                break;
            case Piece.Type.Knight:
                value = 30;
                prefab = faceUp ? WhiteKnight : BlackKnight;
                break;
            default:
                Debug.Log("Unsupported type: " + type);
                return null;
        }

        var piece = Instantiate(prefab, tile.transform);
        var sm = piece.GetComponent<SpriteRenderer>();

        var p = piece.GetComponent<Piece>();
        p.isKing = isKing;
        p.type = type;
        p.value = value;
        p.facingUp = faceUp;
        p.color = player.color;
        p.tile = tile;
        p.player = player;

        p.value = (p.color == Color.white ? 1 : -1) * p.value; // pieces have positive/negative value, depending on what side they're on
        p.name = (p.color == Color.white ? "white" : "black") + " " + type.ToString();

        //Debug.Log("piece created: " + type + ", "+ faceUp);
        tile.CurrentPiece = p;
        return p;
    }

}
