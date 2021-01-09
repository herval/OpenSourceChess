using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public SpriteRenderer avatar;
    public DumbAI turnManager;
    public Color color;

    private void Update()
    {
        avatar.color = color;
    }

    // Called when a player's turn starts
    void OnTurn(Board board)
    {
        turnManager?.ActOn(board);
    }
}
