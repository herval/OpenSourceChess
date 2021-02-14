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

    public GameObject BoardView;

    public Text TurnStatusDisplay;
    public GameArrangement ArrangementManager;

    public PlayerView PlayerOne;
    public PlayerView PlayerTwo;
    PlayerView CurrentPlayer;

    public MoveLog MoveLog;

    public Button UndoButton;
    public Button QuitButton;

    public GameObject GameOverScreen;

    TileView CurrentHighlightedTileView;
    private PieceView CurrentPieceView;

    private bool WaitingForAnimation = false;

    public TileView[,] TileViews;
    public Board Board;

    // TODO make these a single thing
    public TileFactory TileFactory;
    public PieceFactory PieceFactory;

    public int Width = 10;
    public int Height = 10; // 10 as default so a single tile has a scale of 0.1

    private void Start() {
        Prefs = PlayerPreferences.Instance;

        PlayerOne.Color = Color.white; // TODO randomized option
        PlayerOne.FacingUp = true;
        PlayerOne.TurnManager = Prefs.PlayerOneManager;

        PlayerTwo.Color = Color.black;
        PlayerTwo.FacingUp = false;
        PlayerTwo.TurnManager = Prefs.PlayerTwoManager;

        ArrangementManager = new StandardGameArrangement();

        TileViews = TileFactory.Reset(Width, Height, BoardView);
        this.Board = new Board(
            GetTiles(this.TileViews),
            ArrangementManager.Initialize(PieceFactory, TileViews, PlayerOne, PlayerTwo)
        );

        UndoButton.onClick.AddListener(delegate { UndoMove(); });
        QuitButton.onClick.AddListener(delegate { Quit(); });

        OnNextTurn();
    }

    private Tile[,] GetTiles(TileView[,] tileView) {
        Tile[,] tiles = new Tile[tileView.GetLength(0), tileView.GetLength(1)];
        for (int x = 0; x < tileView.GetLength(0); x++) {
            for (int y = 0; y < tileView.GetLength(1); y++) {
                tiles[x, y] = tileView[x, y].State;
            }
        }

        return tiles;
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

        HighlightSelectedTile();

        HandleClick();
    }

    [CanBeNull]
    private TileView TileAt(Vector3 mousePosition) {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && hit.collider != null && hit.collider.tag == "Tile") // hovering over a tile
        {
            return hit.collider.gameObject.GetComponent<TileView>();
        }

        return null;
    }

    private void HandleClick() {
        if (WaitingForAnimation) {
            return;
        }

        var tile = TileAt(Input.mousePosition);
        if (tile != null && Input.GetMouseButtonDown(0)) // trying to select or move
        {
            if (CurrentPieceView == null) {
                SelectPiece(PieceAt(tile));
            }
            else {
                var play = CurrentPieceView.UnblockedMoveTo(tile);
                if (play != null) {
                    Execute(play);
                }
                else {
                    SelectPiece(null);
                }
            }
        }

#if UNITY_EDITOR
        if (tile != null && Input.GetMouseButtonDown(1)) // highlight for debugging
        {
            TogglePotentialMoves(PieceAt(tile));
        }
#endif
    }

    private PieceView PieceAt(TileView tile) {
        if (tile == null) {
            return null;
        }

        return tile.CurrentPiece;
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
                Board,
                TileViews)
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
                    SelectPiece(null);

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

        if (play.Play.isRewind) {
            MoveLog.Pop();
        }
        else {
            MoveLog.Push(play);
        }

        this.Board = play.Move(this.Board, (moved) => {
            if (next != null && next.Count() > 0) {
                moveRecursively(next[0], next.GetRange(1, next.Count - 1), done);
            }
            else {
                moveRecursively(null, null, done);
            }
        });
    }

    private void TogglePotentialMoves(PieceView pieceView) {
        if (pieceView != null && !pieceView.PotentialMoves.Any()) {
            return;
        }

        bool alreadyShowing = TileAtPosition(pieceView.PotentialMoves[0].TileTo).PotentialMove;

        // render potential moves
        if (!alreadyShowing) {
            pieceView.PotentialMoves.ForEach(m => {
                TileAtPosition(m.TileTo).BlockedMove = m.BlockedMove;
                TileAtPosition(m.TileTo).PotentialMove = true;
            });
        }
        else // de-select all
        {
            pieceView.PotentialMoves.ForEach(m => {
                TileAtPosition(m.TileTo).BlockedMove = false;
                TileAtPosition(m.TileTo).PotentialMove = false;
            });
        }
    }

    private TileView TileAtPosition(Tile pos) {
        return TileViews[pos.X, pos.Y];
    }

    private bool CanSelect(PieceView pieceView) {
        return pieceView?.Player == CurrentPlayer && (pieceView?.UnblockedMoves().Any() ?? false);
    }

    private void UpdateUI() {
        TurnStatusDisplay.text = (CurrentPlayer.Color == Color.black ? "Dark" : "Light") + "'s turn";
    }

    // TODO highlight tile instead
    private void HighlightSelectedTile() {
        TileView selected = TileAt(Input.mousePosition);
        HighlightTile(selected, CurrentPieceView);
    }

    private void SelectPiece(PieceView pieceView) {
        if (CurrentPieceView != null) {
            TogglePotentialMoves(CurrentPieceView);
            CurrentPieceView.TileView.Selected = false;
        }

        if (pieceView == null) {
            CurrentPieceView = null;
        }

        if (pieceView != null && CanSelect(pieceView)) {
            pieceView.TileView.Selected = true;
            CurrentPieceView = pieceView;
            TogglePotentialMoves(pieceView);
        }
    }

    private void HighlightTile(TileView tileView, PieceView currentPieceView) {
        if (tileView == null && CurrentHighlightedTileView != null) {
            CurrentHighlightedTileView.Highlighted = false;
            CurrentHighlightedTileView = null;
        }

        if (tileView == null) {
            return;
        }

        // unhighlight previous tile
        if (CurrentHighlightedTileView != null && tileView != CurrentHighlightedTileView) {
            CurrentHighlightedTileView.Highlighted = false;
        }

        // highlight only pieces you own - except when dragging one already
        if ((currentPieceView == null && CanSelect(PieceAt(tileView))) || // new piece selection
            (currentPieceView != null && currentPieceView.UnblockedMoveTo(tileView) != null)) // potential move 
        {
            tileView.Highlighted = true;
            CurrentHighlightedTileView = tileView;
        }
    }
}