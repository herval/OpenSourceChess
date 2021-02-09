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


    public Piece Create(Tile tile, Piece.PieceType pieceType, bool faceUp, Player player)
    {
        GameObject prefab = null;
        int value = 0;
        bool isKing = false;


        // TODO decouple facing from piece color
        switch (pieceType)
        {
            case Piece.PieceType.Rook:
                value = 50;
                prefab = faceUp ? WhiteRook : BlackRook;
                break;

            case Piece.PieceType.Bishop:
                value = 30;
                prefab = faceUp ? WhiteBishop : BlackBishop;
                break;
            case Piece.PieceType.King:
                value = 900;
                isKing = true;
                prefab = faceUp ? WhiteKing : BlackKing;
                break;
            case Piece.PieceType.Queen:
                value = 90;
                prefab = faceUp ? WhiteQueen : BlackQueen;
                break;
            case Piece.PieceType.Pawn:
                value = 10;
                prefab = faceUp ? WhitePawn : BlackPawn;
                break;
            case Piece.PieceType.Knight:
                value = 30;
                prefab = faceUp ? WhiteKnight : BlackKnight;
                break;
            default:
                Debug.Log("Unsupported type: " + pieceType);
                return null;
        }

        var piece = Instantiate(prefab, tile.transform);

        var p = piece.GetComponent<Piece>();
        p.IsKing = isKing;
        p.Type = pieceType;
        p.Value = value;
        p.FacingUp = faceUp;
        p.Color = player.Color;
        p.Tile = tile;
        p.Player = player;

        p.Value = (p.Color == Color.white ? 1 : -1) * p.Value; // pieces have positive/negative value, depending on what side they're on
        p.name = (p.Color == Color.white ? "white" : "black") + " " + pieceType.ToString();

        //Debug.Log("piece created: " + type + ", "+ faceUp);
        tile.CurrentPiece = p;
        return p;
    }

}
