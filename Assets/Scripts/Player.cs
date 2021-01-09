using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public SpriteRenderer avatar;
    public TurnManager turnManager;
    public Color color;

    public bool facingUp;

    public List<Piece> Pieces = new List<Piece>();

    private void Update()
    {
        avatar.color = color;
    }

    // Called when a player's turn starts
    void OnTurn(Board board)
    {
        turnManager?.ActOn(this, board);
    }
}
