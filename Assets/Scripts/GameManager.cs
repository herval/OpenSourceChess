using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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

    Tile currentHighlightedTile;
    List<Play> currentPotentialMoves = new List<Play>();
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

        OnNextTurn();
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
                if (currentPiece.CanMoveTo(tile))
                {
                    Execute(new MoveTo(currentPiece, tile));
                }
                else
                {
                    SelectPiece(null);
                }
            }
        }
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

        PieceCommand c = currentPlayer.turnManager.ActOn(currentPlayer, currentPlayer == playerOne ? playerTwo : playerOne, board);
        Execute(c);
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
                return;
            case MoveTo m:
                waitingForAnimation = true;

                m.piece.MoveTo(m.tile, (moved) =>
                {
                    Debug.Log("animation done " + moved);
                    waitingForAnimation = false;
                    SelectPiece(null);
                    OnNextTurn();
                });
                return;
            default:
                throw new Exception("Unhandled command: " + c);
        }
    }

    private void TogglePotentialMoves()
    {
        // render potential moves
        if (currentPiece != null)
        {
            currentPiece.PotentialMoves.ForEach(m =>
            {
                m.Tile.BlockedMove = m.Blocker != null;
                m.Tile.PotentialMove = true;
                currentPotentialMoves.Add(m);
            });
        }
        else // de-select all
        {
            currentPotentialMoves.ForEach(m =>
            {
                m.Tile.BlockedMove = false;
                m.Tile.PotentialMove = false;
            });
            currentPotentialMoves.Clear();
        }
    }

    private bool CanSelect(Piece piece)
    {
        return piece?.color == currentPlayer.color;
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
        }
        
        TogglePotentialMoves();
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
        if ((currentPiece == null && tile.CurrentPiece?.color == currentPlayer.color) || // new piece selection
            (currentPiece != null && currentPiece.CanMoveTo(tile))) // potential move 
        {
            tile.Highlighted = true;
            currentHighlightedTile = tile;
        }
    }
}