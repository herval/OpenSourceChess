using System;
using System.Collections;
using System.Collections.Generic;
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

    Tile currentHighlightedTile;
    List<Tile> currentPotentialMoves = new List<Tile>();
    private Piece currentPiece;


    private void Start()
    {
        prefs = PlayerPreferences.Instance;

        playerOne.color = Color.white; // TODO randomized option
        playerOne.facingUp = true;

        playerTwo.color = Color.black;
        playerTwo.facingUp = false;
        if (prefs.gameMode == GameMode.PlayerVersusComputer)
        {
            playerTwo.turnManager = new DumbAI();
        }

        arrangementManager = new StandardGameArrangement();
        board.Reset();
        arrangementManager.Initialize(board, playerOne, playerTwo);

        OnNextTurn();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUI();

        DragPieceAround();

        HandleClick();
    }

    private void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && hit.collider != null && hit.collider.tag == "Tile") // hovering over a tile
        {
            var tile = hit.collider.gameObject.GetComponent<Tile>();

            if (Input.GetMouseButtonDown(0)) // trying to select or move
            {
                if (currentPiece != null)
                {
                    if (MoveToTile(tile))
                    {
                        OnNextTurn();
                    }
                }
                else
                {
                    SelectPiece(tile?.CurrentPiece);
                }
            }
            else
            {
                HighlightTile(tile);
            }
        }
        else
        { // select nothing
            HighlightTile(null); // TODO do we need to do this on update?
        }
    }

    private void OnNextTurn()
    {
        board.ComputePotentialMoves();
        if (currentPlayer == null)
        {
            currentPlayer = playerOne;
        }
        else
        {
            currentPlayer = currentPlayer == playerOne ? playerTwo : playerOne; // alternate player
        }

        PieceCommand c = currentPlayer.turnManager?.ActOn(currentPlayer, board);
        switch(c)
        {
            case null:
                return;
            case LoseGame l:
                // TODO implement end of game
                return;
            case MoveTo m:
                SelectPiece(m.piece);
                MoveToTile(m.tile);
                OnNextTurn();
                return;
            default:
                throw new Exception("Unhandled command: " + c);
        }
    }

    private void HighlightPotentialMoves()
    {
        // render potential moves
        if(currentPiece != null)
        {
            foreach(Tile t in currentPiece.PotentialMoves)
            {
                t.PotentialMove = true;
                currentPotentialMoves.Add(t);
            }
        } else
        {
            foreach(Tile movable in currentPotentialMoves)
            {
                movable.PotentialMove = false;
            }
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

    private void DragPieceAround()
    {
        if (currentPiece != null)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            currentPiece.transform.position = mousePos;
        }
    }

    // return if the piece actually moved
    private bool MoveToTile(Tile tile)
    {
        // no bueno
        if(currentPiece == null)
        {
            Debug.Log("Trying to move null piece? " + currentPiece);
            return false;
        }

        bool moved = currentPiece.MoveTo(tile);
        SelectPiece(null);
        return moved;

        //// put piece back
        //if (tile == currentPiece.tile)
        //{
        //    currentPiece.MoveTo(tile);
        //    SelectPiece(null);
        //    return false;
        //}

        //// move to new tile
        //if (currentPiece?.CanMoveTo(tile) ?? false)
        //{
        //    currentPiece.MoveTo(tile);
        //    SelectPiece(null);
        //    return true;
        //}

        //return false;
        //else // move back to home
        //{
        //    // TODO animate?
        //    //currentSelectedTile.SetPiece(currentPiece);
        //    //currentPiece = null;
        //    return false;
        //}
    }



    private void SelectPiece(Piece piece)
    {
        if (piece == null)
        {
            currentPiece = null;
            HighlightPotentialMoves();
            return;
        }

        if (CanSelect(piece))
        { 
            piece.Select();
            Debug.Log("Selecting piece: " + piece);
            currentPiece = piece;
            HighlightPotentialMoves();
        }
    }

    private void HighlightTile(Tile tile)
    {
        if(tile == null)
        {
            if (currentHighlightedTile != null)
            {
                currentHighlightedTile.Highlighted = false;
            }
            return;
        }

        // unhighlight previous tile
        if (currentHighlightedTile != null && tile != currentHighlightedTile)
        {
            currentHighlightedTile.Highlighted = false;
        }

        // highlight only pieces you own - except when dragging one already
        if (tile.CurrentPiece?.color == currentPlayer.color && currentPiece == null)
        {
            tile.Highlighted = true;
            currentHighlightedTile = tile;
        }
    }
}