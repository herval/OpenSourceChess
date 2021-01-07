using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// creates pieces & manages their assets
public class PieceFactory : MonoBehaviour
{

    public Sprite LightRook;
    public Sprite LightKing;
    public Sprite LightQueen;
    public Sprite LightBishop;
    public Sprite LightPawn;
    public Sprite LightKnight;

    public Sprite DarkRook;
    public Sprite DarkKing;
    public Sprite DarkQueen;
    public Sprite DarkBishop;
    public Sprite DarkPawn;
    public Sprite DarkKnight;


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
                sm.sprite = faceUp ? LightBishop : DarkBishop;
                break;
            case Piece.Type.King:
                sm.sprite = faceUp ? LightKing : DarkKing;
                break;
            case Piece.Type.Queen:
                sm.sprite = faceUp ? LightQueen : DarkQueen;
                break;
            case Piece.Type.Rook:
                sm.sprite = faceUp ? LightRook : DarkRook;
                break;
            case Piece.Type.Pawn:
                sm.sprite = faceUp ? LightPawn : DarkPawn;
                break;
            case Piece.Type.Knight:
                sm.sprite = faceUp ? LightKnight : DarkKnight;
                break;
            default:
                Debug.Log("Unsupported type: " + type);
                return null;
        }

        Debug.Log("piece created: " + type + ", "+ faceUp);
        return piece;
    }

}
