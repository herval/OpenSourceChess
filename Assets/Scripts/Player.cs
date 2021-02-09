using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    [FormerlySerializedAs("avatar")] public SpriteRenderer Avatar;
    public TurnManager TurnManager;
    [FormerlySerializedAs("color")] public Color Color;

    [FormerlySerializedAs("facingUp")] public bool FacingUp;

    public List<Piece> Pieces = new List<Piece>();

    public PiecesStack CapturedPieces;
    public bool InCheck = false;

    public List<Play> UnblockedMoves()
    {
        return this.Pieces
            .ConvertAll(p => p.PotentialMoves)
            .SelectMany(c => c.FindAll(m => !m.BlockedMove)) // only POSSIBLE moves please
            .ToList();
    }
    
    public int TotalPoints()
    {
        return Pieces.ConvertAll(p => p.Value).Sum();
    }

    private void Update()
    {
        Avatar.color = Color;
    }

    public void Capture(Piece p)
    {
        p.Player.Pieces.Remove(p);
        CapturedPieces.Add(p);
    }

    public void PutBack(Piece p)
    {
        p.Player.Pieces.Add(p);
        CapturedPieces.Remove(p);
    }
}
