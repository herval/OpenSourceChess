using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// creates pieces & manages their assets
public class PieceFactory : MonoBehaviour {

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

    public GameObject WhiteCheckersPawn;
    public GameObject BlackCheckersPawn;


    public PieceView Create(TileView tileView, PieceType pieceType, PlayerView player) {
        GameObject prefab = null;
        int value = 0;
        bool isKing = false;
        PlayerPosition faceUp = player.StartingPosition;
        bool isWhite = player.Color == Color.white;
        char name;

        switch (pieceType) {
            case PieceType.Rook:
                value = 50;
                prefab = isWhite ? WhiteRook : BlackRook;
                name = 'R';
                break;

            case PieceType.Bishop:
                value = 30;
                prefab = isWhite ? WhiteBishop : BlackBishop;
                name = 'B';
                break;
            case PieceType.King:
                value = 900;
                isKing = true;
                prefab = isWhite ? WhiteKing : BlackKing;
                name = 'K';
                break;
            case PieceType.Queen:
                value = 90;
                prefab = isWhite ? WhiteQueen : BlackQueen;
                name = 'Q';
                break;
            case PieceType.Pawn:
                value = 10;
                prefab = isWhite ? WhitePawn : BlackPawn;
                name = ' '; // pawns are nameless poor things
                break;
            case PieceType.Knight:
                value = 30;
                prefab = isWhite ? WhiteKnight : BlackKnight;
                name = 'N';
                break;
            case PieceType.CheckersPawn:
                value = 10;
                prefab = isWhite ? WhiteCheckersPawn : BlackCheckersPawn;
                name = 'C';
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
        p.State.StartingPosition = faceUp;
        p.State.Player = player.State;
        p.State.FigurineAlgebraicNotation = name;
        p.SetTile(tileView, true, null);
        p.Player = player;

        p.State.Value = (p.State.Player.Number == 1 ? 1 : -1) * p.State.Value; // pieces have positive/negative value, depending on what side they're on

        //Debug.Log("piece created: " + type + ", "+ faceUp);
        return p;
    }

}
