using System;
using System.Collections.Generic;
using System.Linq;

public enum PlayerType {
    PLAYER,
    COMPUTER,
    REMOTE,
}

public class Player
{
    public static String TypeToString(PlayerType type) {
        switch (type) {
            case PlayerType.PLAYER:
                return "Human";
            case PlayerType.REMOTE:
                return "Online Entity";
            case PlayerType.COMPUTER:
                return "Computer";
        }

        return "Unknown";
    }

    public static PlayerType StringToType(String str) {
        switch (str) {
            case "Human":
                return PlayerType.PLAYER;
            case "Online Entity":
                return PlayerType.REMOTE;
            case "Computer":
                return PlayerType.COMPUTER;
        }

        return PlayerType.PLAYER;
    }
    
    public TurnManager TurnManager;
    public int Number;
    public PlayerPosition StartingPosition;

    public bool InCheck = false;
    public List<Piece> Pieces = new List<Piece>();
    public List<Piece> CapturedPieces = new List<Piece>();

    public List<Move> UnblockedMoves()
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