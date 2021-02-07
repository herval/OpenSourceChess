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
    public bool InCheck = false;

    public List<Play> PotentialMoves()
    {
        return this.Pieces
            .ConvertAll(p => p.PotentialMoves)
            .SelectMany(c => c.FindAll(m => m.Blocker == null)) // only POSSIBLE moves please
            .ToList();
    }
    
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
        p.player.Pieces.Remove(p);
        CapturedPieces.Add(p);
    }

}
