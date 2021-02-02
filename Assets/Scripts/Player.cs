using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    public SpriteRenderer avatar;
    public TurnManager turnManager;
    public Color color;

    public bool facingUp;

    public List<Piece> Pieces = new List<Piece>();

    public PiecesStack CapturedPieces;

    public int TotalPoints()
    {
        return Pieces.ConvertAll(p => p.value).Sum();
    }

    private void Update()
    {
        avatar.color = color;
    }

    public void Capture(Piece p)
    {
        CapturedPieces.Add(p);
    }

    // Called when a player's turn starts
    void OnTurn(Board board)
    {
        turnManager?.ActOn(this, board);
    }
}
