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


    public PieceView Create(TileView tileView, PieceType pieceType, PlayerView player)
    {
        GameObject prefab = null;
        int value = 0;
        bool isKing = false;
        bool faceUp = player.FacingUp;

        // TODO decouple facing from piece color
        switch (pieceType)
        {
            case PieceType.Rook:
                value = 50;
                prefab = faceUp ? WhiteRook : BlackRook;
                break;

            case PieceType.Bishop:
                value = 30;
                prefab = faceUp ? WhiteBishop : BlackBishop;
                break;
            case PieceType.King:
                value = 900;
                isKing = true;
                prefab = faceUp ? WhiteKing : BlackKing;
                break;
            case PieceType.Queen:
                value = 90;
                prefab = faceUp ? WhiteQueen : BlackQueen;
                break;
            case PieceType.Pawn:
                value = 10;
                prefab = faceUp ? WhitePawn : BlackPawn;
                break;
            case PieceType.Knight:
                value = 30;
                prefab = faceUp ? WhiteKnight : BlackKnight;
                break;
            default:
                Debug.Log("Unsupported type: " + pieceType);
                return null;
        }

        var piece = Instantiate(prefab, tileView.transform);

        var p = piece.GetComponent<PieceView>();
        p.State.IsKing = isKing;
        p.State.Type = pieceType;
        p.State.Value = value;
        p.State.FacingUp = faceUp;
        p.State.Player = player.State;//.Color == Color.white ? 1 : 2;
        p.SetTile(tileView, true, null);
        p.Player = player;

        p.State.Value = (p.State.Player.Number == 1 ? 1 : -1) * p.State.Value; // pieces have positive/negative value, depending on what side they're on

        //Debug.Log("piece created: " + type + ", "+ faceUp);
        return p;
    }

}
