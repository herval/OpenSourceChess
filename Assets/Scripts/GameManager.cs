using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// keep game state and handle inputs
public class GameManager : MonoBehaviour
{
    PlayerPreferences Prefs;

    public Text TurnStatusDisplay;
    [FormerlySerializedAs("board")] public Board Board;
    [FormerlySerializedAs("arrangementManager")] public GameArrangement ArrangementManager;

    [FormerlySerializedAs("playerOne")] public Player PlayerOne;
    [FormerlySerializedAs("playerTwo")] public Player PlayerTwo;
    Player CurrentPlayer;

    public MoveLog MoveLog;

    public Button UndoButton;

    public GameObject GameOverScreen;
    
    Tile CurrentHighlightedTile;
    private Piece CurrentPiece;
    
    private bool WaitingForAnimation = false;

    private void Start()
    {
        Prefs = PlayerPreferences.Instance;

        
        PlayerOne.Color = Color.white; // TODO randomized option
        PlayerOne.FacingUp = true;
        PlayerOne.TurnManager = Prefs.PlayerOneManager;

        PlayerTwo.Color = Color.black;
        PlayerTwo.FacingUp = false;
        PlayerTwo.TurnManager = Prefs.PlayerTwoManager;

        ArrangementManager = new StandardGameArrangement();
        Board.Reset();
        ArrangementManager.Initialize(Board, PlayerOne, PlayerTwo);

        UndoButton.onClick.AddListener(delegate { UndoMove(); });
        
        OnNextTurn();
    }

    void UndoMove()
    {
        Debug.Log("Undoing...");
        // if(currentPlayer != )
        Play undo = MoveLog.Last();
        if (undo != null)
        {
            Execute(new Movement(undo.Reverse()));
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUI();

        HighlightSelectedTile();

        HandleClick();
    }

    [CanBeNull]
    private Tile TileAt(Vector3 mousePosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && hit.collider != null && hit.collider.tag == "Tile") // hovering over a tile
        {
            return hit.collider.gameObject.GetComponent<Tile>();
        }

        return null;
    }

    private void HandleClick()
    {
        if (WaitingForAnimation)
        {
            return;
        }

        var tile = TileAt(Input.mousePosition);
        if (tile != null && Input.GetMouseButtonDown(0)) // trying to select or move
        {
            if (CurrentPiece == null)
            {
                SelectPiece(tile?.CurrentPiece);
            }
            else
            {
                var play = CurrentPiece.UnblockedMoveTo(tile);
                if (play != null)
                {
                    Execute(new Movement(play));
                }
                else
                {
                    SelectPiece(null);
                }
            }
        }

#if UNITY_EDITOR        
        if (tile != null && Input.GetMouseButtonDown(1)) // highlight for debugging
        {
            if (tile.CurrentPiece != null)
            {
                TogglePotentialMoves(tile.CurrentPiece);
            }
        }
#endif
    }

    private void OnNextTurn()
    {
        if (CurrentPlayer == null)
        {
            CurrentPlayer = PlayerOne;
        }
        else
        {
            Player opponent = CurrentPlayer == PlayerOne ? PlayerTwo : PlayerOne;
            CurrentPlayer = opponent; // alternate player
        }

        Execute(
            CurrentPlayer.TurnManager.ActOn(CurrentPlayer, CurrentPlayer == PlayerOne ? PlayerTwo : PlayerOne, Board)
        );
    }

    private void Execute(PieceCommand c)
    {
        switch (c)
        {
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

                moveRecursively(m.Play, m.Play.ConnectedPlays, (moved) =>
                {
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

    private void moveRecursively(Play play, List<Play> next, AfterAnimationCallback done)
    {
        if (play == null)
        {
            done.Invoke(true);
            return;
        }

        if (play.isRewind)
        {
            MoveLog.Pop();
        } else {
            MoveLog.Push(play);
        }

        play.Move((moved) =>
        {
            if (next != null && next.Count() > 0)
            {
                moveRecursively(next[0], next.GetRange(1, next.Count-1), done);    
            }
            else
            {
                moveRecursively(null, null, done);
            }
        });
    }

    private void TogglePotentialMoves(Piece piece)
    {
        if (piece != null && !piece.PotentialMoves.Any())
        {
            return;
        }

        bool alreadyShowing = piece.PotentialMoves[0].TileTo.PotentialMove;

        // render potential moves
        if (!alreadyShowing) 
        {
            piece.PotentialMoves.ForEach(m =>
            {
                m.TileTo.BlockedMove = m.BlockedMove;
                m.TileTo.PotentialMove = true;
            });
        }
        else // de-select all
        {
            piece.PotentialMoves.ForEach(m =>
            {
                m.TileTo.BlockedMove = false;
                m.TileTo.PotentialMove = false;
            });
        }
    }

    private bool CanSelect(Piece piece)
    {
        return piece?.Color == CurrentPlayer.Color && (piece?.UnblockedMoves().Any() ?? false);
    }

    private void UpdateUI()
    {
        TurnStatusDisplay.text = (CurrentPlayer.Color == Color.black ? "Dark" : "Light") + "'s turn";
    }

    // TODO highlight tile instead
    private void HighlightSelectedTile()
    {
        Tile selected = TileAt(Input.mousePosition);
        HighlightTile(selected, CurrentPiece);
    }
    
    private void SelectPiece(Piece piece)
    {
        if (CurrentPiece != null)
        {
            TogglePotentialMoves(CurrentPiece);
            CurrentPiece.Tile.Selected = false;
        }
        
        if (piece == null)
        {
            CurrentPiece = null;
        }

        if (piece != null && CanSelect(piece))
        {
            piece.Tile.Selected = true;
            CurrentPiece = piece;
            TogglePotentialMoves(piece);
        }
    }

    private void HighlightTile(Tile tile, Piece currentPiece)
    {
        if (tile == null && CurrentHighlightedTile != null) {
            CurrentHighlightedTile.Highlighted = false;
            CurrentHighlightedTile = null;
        }
        
        if(tile == null) {
            return;
        }

        // unhighlight previous tile
        if (CurrentHighlightedTile != null && tile != CurrentHighlightedTile)
        {
            CurrentHighlightedTile.Highlighted = false;
        }

        // highlight only pieces you own - except when dragging one already
        if ((currentPiece == null && CanSelect(tile.CurrentPiece)) || // new piece selection
            (currentPiece != null && currentPiece.UnblockedMoveTo(tile) != null)) // potential move 
        {
            tile.Highlighted = true;
            CurrentHighlightedTile = tile;
        }
    }
}
