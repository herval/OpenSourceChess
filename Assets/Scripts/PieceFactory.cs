using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// creates pieces & manages their assets
public class PieceFactory : MonoBehaviour
{

    public Sprite WhiteRook;
    public Sprite WhiteKing;
    public Sprite WhiteQueen;
    public Sprite WhiteBishop;
    public Sprite WhitePawn;
    public Sprite WhiteKnight;

    public Sprite BlackRook;
    public Sprite BlackKing;
    public Sprite BlackQueen;
    public Sprite BlackBishop;
    public Sprite BlackPawn;
    public Sprite BlackKnight;


    public GameObject piecePrefab;

    //public GameObject[] piecePrefabs;

    public GameObject Create(Transform parent, Piece.Type type, bool faceUp)
    {
        var piece = Instantiate(piecePrefab, parent);
        var sm = piece.GetComponent<SpriteRenderer>();

        var p = piece.GetComponent<Piece>();
        p.type = type;
        p.facingUp = faceUp;

        // TODO decouple facing from piece color
        switch (type)
        {
            case Piece.Type.Bishop:
                sm.sprite = faceUp ? WhiteBishop : BlackBishop;
                break;
            case Piece.Type.King:
                sm.sprite = faceUp ? WhiteKing : BlackKing;
                break;
            case Piece.Type.Queen:
                sm.sprite = faceUp ? WhiteQueen : BlackQueen;
                break;
            case Piece.Type.Rook:
                sm.sprite = faceUp ? WhiteRook : BlackRook;
                break;
            case Piece.Type.Pawn:
                sm.sprite = faceUp ? WhitePawn : BlackPawn;
                break;
            case Piece.Type.Knight:
                sm.sprite = faceUp ? WhiteKnight : BlackKnight;
                break;
            default:
                Debug.Log("Unsupported type: " + type);
                return null;
        }

        Debug.Log("piece created: " + type + ", "+ faceUp);
        return piece;
    }

}
