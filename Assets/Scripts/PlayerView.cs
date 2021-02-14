using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    public SpriteRenderer Avatar;

    private Color _color;
    public Color Color {
        get {
            return _color;
        }
        
        set {
            _color = value;
            State.Number = value == Color.white ? 1 : 2;
        }
    }

    public Player State = new Player();

    public PiecesStack CapturedPieces;

    public bool FacingUp {
        get {
            return State.FacingUp;
        }
        
        set {
            State.FacingUp = value;
        }
    }

    public TurnManager TurnManager {
        get {
            return State.TurnManager;
        }
        
        set {
            State.TurnManager = value;
        }
    }

    public List<Piece> Pieces {
        get {
            return State.Pieces;
        }
        set {
            State.Pieces = value;
        }
    }

    private void Update()
    {
        Avatar.color = Color;
    }

    public void Capture(PieceView p) {
        State.Capture(p.State);
        
        CapturedPieces.Add(p);
    }

    public void PutBack(PieceView p) {
        State.PutBack(p.State);
        
        CapturedPieces.Remove(p);
    }

    public PieceCommand ActOn(PlayerView opponent, Board board, TileView[,] tiles) {
        // TODO create AnimatedPlay?
        Play play = TurnManager.ActOn(this.State, opponent.State, board);
        if (play == null) {
            return null;
        }

        TileView tileFrom = tiles[play.TileFrom.X, play.TileFrom.Y];
        TileView tileTo = tiles[play.TileTo.X, play.TileTo.Y];
        var destPiece = tileTo?.CurrentPiece;
        var ownPiece = tileFrom?.CurrentPiece;
        
        return new Movement(
            play,
            ownPiece,
            destPiece,
            tileTo,
            tileFrom);
    }
}
