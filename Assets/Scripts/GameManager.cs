using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

// keep game state and handle inputs
public class GameManager : MonoBehaviour
{
    PlayerPreferences prefs;

    public Text TurnStatusDisplay;
    public Board board;
    public GameArrangement arrangementManager;

    public Player playerOne;
    public Player playerTwo;
    Player currentPlayer;

    public MoveLog MoveLog;

    public Button UndoButton;

    public GameObject GameOverScreen;
    
    Tile currentHighlightedTile;
    private Piece currentPiece;
    
    private bool waitingForAnimation = false;

    private void Start()
    {
        prefs = PlayerPreferences.Instance;

        
        playerOne.color = Color.white; // TODO randomized option
        playerOne.facingUp = true;
        playerOne.turnManager = prefs.PlayerOneManager;

        playerTwo.color = Color.black;
        playerTwo.facingUp = false;
        playerTwo.turnManager = prefs.PlayerTwoManager;

        arrangementManager = new StandardGameArrangement();
        board.Reset();
        arrangementManager.Initialize(board, playerOne, playerTwo);

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
        if (waitingForAnimation)
        {
            return;
        }

        var tile = TileAt(Input.mousePosition);
        if (tile != null && Input.GetMouseButtonDown(0)) // trying to select or move
        {
            if (currentPiece == null)
            {
                SelectPiece(tile?.CurrentPiece);
            }
            else
            {
                var play = currentPiece.UnblockedMoveTo(tile);
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
        if (currentPlayer == null)
        {
            currentPlayer = playerOne;
        }
        else
        {
            Player opponent = currentPlayer == playerOne ? playerTwo : playerOne;
            currentPlayer = opponent; // alternate player
        }

        Execute(
            currentPlayer.turnManager.ActOn(currentPlayer, currentPlayer == playerOne ? playerTwo : playerOne, board)
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
                waitingForAnimation = true;

                moveRecursively(m.play, m.play.ConnectedPlays, (moved) =>
                {
                    // Debug.Log("animation done " + moved);
                    waitingForAnimation = false;
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
        return piece?.color == currentPlayer.color && (piece?.UnblockedMoves().Any() ?? false);
    }

    private void UpdateUI()
    {
        TurnStatusDisplay.text = (currentPlayer.color == Color.black ? "Dark" : "Light") + "'s turn";
    }

    // TODO highlight tile instead
    private void HighlightSelectedTile()
    {
        Tile selected = TileAt(Input.mousePosition);
        HighlightTile(selected, currentPiece);
    }
    
    private void SelectPiece(Piece piece)
    {
        if (currentPiece != null)
        {
            TogglePotentialMoves(currentPiece);
            currentPiece.tile.Selected = false;
        }
        
        if (piece == null)
        {
            currentPiece = null;
        }

        if (piece != null && CanSelect(piece))
        {
            piece.tile.Selected = true;
            currentPiece = piece;
            TogglePotentialMoves(piece);
        }
    }

    private void HighlightTile(Tile tile, Piece currentPiece)
    {
        if (tile == null && currentHighlightedTile != null) {
            currentHighlightedTile.Highlighted = false;
            currentHighlightedTile = null;
        }
        
        if(tile == null) {
            return;
        }

        // unhighlight previous tile
        if (currentHighlightedTile != null && tile != currentHighlightedTile)
        {
            currentHighlightedTile.Highlighted = false;
        }

        // highlight only pieces you own - except when dragging one already
        if ((currentPiece == null && CanSelect(tile.CurrentPiece)) || // new piece selection
            (currentPiece != null && currentPiece.UnblockedMoveTo(tile) != null)) // potential move 
        {
            tile.Highlighted = true;
            currentHighlightedTile = tile;
        }
    }
}
