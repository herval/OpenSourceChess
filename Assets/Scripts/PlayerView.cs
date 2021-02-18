using System;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour {
    public SpriteRenderer Avatar;

    public Text Username;

    private Color _color;

    public Color Color {
        get { return _color; }

        set {
            _color = value;
            State.Number = value == Color.white ? 1 : 2;
            Avatar.color = value;
        }
    }

    public Player State = new Player();

    public PiecesStack CapturedPieces;

    public PlayerPosition StartingPosition {
        get { return State.StartingPosition; }

        set { State.StartingPosition = value; }
    }

    public TurnManager TurnManager {
        get { return State.TurnManager; }

        set {
            State.TurnManager = value;
            UpdateUser();
        }
    }

    private void UpdateUser() {
        if (this.TurnManager is RemoteTurnManager) {
            Username.text = ((RemoteTurnManager) TurnManager).Username();
        }
        else if (this.TurnManager.IsHuman()) {
            Username.text = Player.TypeToString(PlayerType.PLAYER);
        }
        else {
            Username.text = Player.TypeToString(PlayerType.COMPUTER);
        }
    }

    public List<Piece> Pieces {
        get { return State.Pieces; }
        set { State.Pieces = value; }
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

        if (play is Lose) {
            return new LoseGame(((Lose)play).Player);
        }

        Move move = (Move)play;

        TileView tileFrom = tiles[move.TileFrom.X, move.TileFrom.Y];
        TileView tileTo = tiles[move.TileTo.X, move.TileTo.Y];
        TileView tileCapture = move.TileCaptured != null ? tiles[move.TileCaptured.X, move.TileCaptured.Y] : null;
        var ownPiece = tileFrom?.CurrentPiece;
        var destPiece = tileCapture?.CurrentPiece;

        return new Movement(
            move,
            ownPiece,
            destPiece,
            tileTo,
            tileFrom,
            tileCapture);
    }

    private void Awake() {
        Username.text = "foo?!!?";
    }

    private void Update() {
        if (TurnManager is RemoteTurnManager) {
        }
    }
}