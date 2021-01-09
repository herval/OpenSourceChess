using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// keep game state and handle inputs
public class GameManager : MonoBehaviour
{
    public Text TurnStatusDisplay;
    public Board board;
    public StandardGameArrangement arrangementManager;

    public Player playerOne;
    public Player playerTwo;
    Player currentPlayer;

    Tile currentHighlightedTile;
    Tile currentSelectedTile;
    List<Tile> currentPotentialMoves = new List<Tile>();
    private Piece currentPiece;


    private void Start()
    {
        playerOne.color = Color.white; // TODO randomized option
        playerOne.facingUp = true;

        playerTwo.color = Color.black;
        playerTwo.facingUp = false;
        playerTwo.turnManager = new DumbAI();

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
                    if (CanSelect(tile))
                    {
                        SelectTile(tile);
                    }
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
                SelectTile(m.piece.tile);
                MoveToTile(m.tile);
                OnNextTurn();
                return;
            default:
                throw new Exception("Unhandled command: " + c);
        }
    }

    private void UpdatePotentialMoves()
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

    private bool CanSelect(Tile tile)
    {
        return tile.CurrentPiece() != null &&
            tile.CurrentPiece().color == currentPlayer.color;
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
        if(currentPiece == null || currentSelectedTile == null)
        {
            Debug.Log("Trying to move null piece? " + currentPiece + " / " + currentSelectedTile);
            return false;
        }

        // put piece back
        if(tile == currentSelectedTile)
        {
            currentPiece.MoveTo(tile);
            SelectTile(null);
            return false;
        }

        // move to new tile
        if (currentPiece?.CanMoveTo(tile) ?? false)
        {
            currentPiece.MoveTo(tile);
            SelectTile(null);
            return true;
        }

        return false;
        //else // move back to home
        //{
        //    // TODO animate?
        //    //currentSelectedTile.SetPiece(currentPiece);
        //    //currentPiece = null;
        //    return false;
        //}
    }

   

    private void SelectTile(Tile tile)
    {
        if(tile == null)
        {
            currentSelectedTile.Selected = false;
            currentSelectedTile = null;
            currentPiece = null;
            UpdatePotentialMoves();
            return;
        }

        // TODO if player can select...
        Piece current = tile.CurrentPiece();
        if(current != null)
        {
            tile.Selected = true;
            Debug.Log("Selecting piece: " + current);
            currentPiece = current;
            currentSelectedTile = tile;
            UpdatePotentialMoves();
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
        if (tile.CurrentPiece()?.color == currentPlayer.color && currentPiece == null)
        {
            tile.Highlighted = true;
            currentHighlightedTile = tile;
        }
    }
}