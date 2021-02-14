using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

// keep game state and handle inputs
public class GameManager : MonoBehaviour {
    PlayerPreferences Prefs;

    public Text TurnStatusDisplay;

    public PlayerView PlayerOne;
    public PlayerView PlayerTwo;
    PlayerView CurrentPlayer;

    public MoveLog MoveLog;

    public Button UndoButton;
    public Button QuitButton;

    public GameObject GameOverScreen;

    private bool WaitingForAnimation = false;

    public BoardView BoardView; 

    private void Start() {
        Prefs = PlayerPreferences.Instance;

        // when p2 is human, black is on bottom
        if (Prefs.PlayerTwoManager.IsHuman() && !Prefs.PlayerOneManager.IsHuman()) {
            PlayerOne.FacingUp = false;
        }
        else {
            PlayerOne.FacingUp = true;
        }

        PlayerOne.Color = Color.white;
        PlayerTwo.FacingUp = !PlayerOne.FacingUp;
        PlayerTwo.Color = PlayerOne.Color == Color.white ? Color.black : Color.white;
        
        PlayerOne.TurnManager = Prefs.PlayerOneManager;
        PlayerTwo.TurnManager = Prefs.PlayerTwoManager;

        BoardView.Initialize(PlayerOne, PlayerTwo);

        UndoButton.onClick.AddListener(delegate { UndoMove(); });
        QuitButton.onClick.AddListener(delegate { Quit(); });

        OnNextTurn();
    }

    private void Quit() {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    void UndoMove() {
        Debug.Log("Undoing...");
        Movement undo = MoveLog.Last();
        if (undo != null) {
            Execute(undo.Reverse());
        }
    }

    // Update is called once per frame
    void Update() {
        UpdateUI();

        BoardView.HighlightSelectedTile(CurrentPlayer);

        HandleClick();
    }


    private void HandleClick() {
        if (WaitingForAnimation) {
            return;
        }

        // TODO untangle this mess between game manager and board
        var tile = BoardView.TileAt(Input.mousePosition);
        if (tile != null && Input.GetMouseButtonDown(0)) // trying to select or move
        {
            if (BoardView.CurrentPieceView == null) {
                BoardView.SelectPiece(BoardView.PieceAt(tile), CurrentPlayer);
            }
            else {
                var play = BoardView.CurrentPieceView.UnblockedMoveTo(tile);
                if (play != null) {
                    Execute(play);
                }
                else {
                    BoardView.SelectPiece(null, CurrentPlayer);
                }
            }
        }

#if UNITY_EDITOR
        if (tile != null && Input.GetMouseButtonDown(1)) // highlight for debugging
        {
            BoardView.TogglePotentialMoves(BoardView.PieceAt(tile));
        }
#endif
    }

    private void OnNextTurn() {
        if (CurrentPlayer == null) {
            CurrentPlayer = PlayerOne;
        }
        else {
            PlayerView opponent = CurrentPlayer == PlayerOne ? PlayerTwo : PlayerOne;
            CurrentPlayer = opponent; // alternate player
        }

        Execute(
            CurrentPlayer.ActOn(
                CurrentPlayer == PlayerOne ? PlayerTwo : PlayerOne, 
                BoardView.Board,
                BoardView.TileViews)
        );
    }

    private void Execute(PieceCommand c) {
        switch (c) {
            case null:
                // wait for player to interact
                return;

            case LoseGame l:
                // TODO implement end of game
                Debug.Log("Game over!");
                GameOverScreen.SetActive(true);
                return;

            case Movement m:
                WaitingForAnimation = true;

                moveRecursively(m, m.ConnectedMovements, (moved) => {
                    // Debug.Log("animation done " + moved);
                    WaitingForAnimation = false;
                    BoardView.SelectPiece(null, CurrentPlayer);

                    OnNextTurn();
                });
                return;
            default:
                throw new Exception("Unhandled command: " + c);
        }
    }

    private void moveRecursively(Movement play, List<Movement> next, AfterAnimationCallback done) {
        if (play == null) {
            done.Invoke(true);
            return;
        }

        if (play.Play.IsRewind) {
            MoveLog.Pop();
        }
        else {
            MoveLog.Push(play);
        }

        BoardView.Board = play.Move(BoardView.Board, (moved) => {
            if (next != null && next.Count() > 0) {
                moveRecursively(next[0], next.GetRange(1, next.Count - 1), done);
            }
            else {
                moveRecursively(null, null, done);
            }
        });
    }

    private void UpdateUI() {
        TurnStatusDisplay.text = (CurrentPlayer.Color == Color.black ? "Dark" : "Light") + "'s turn";
    }

}