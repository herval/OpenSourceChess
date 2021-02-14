using System;
using System.Collections.Generic;
using System.Linq;
public class Player
{
    public TurnManager TurnManager;
    public int Number;
    public bool FacingUp;

    public bool InCheck = false;
    public List<Piece> Pieces = new List<Piece>();
    public List<Piece> CapturedPieces = new List<Piece>();

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

    public void Capture(Piece piece) {
        CapturedPieces.Add(piece);
        piece.Player.Pieces.Remove(piece);
    }

    public void PutBack(Piece piece) {
        piece.Player.Pieces.Add(piece);
        CapturedPieces.Remove(piece);
    }
}