using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;


public enum PlayerPosition {
    Bottom,
    Top
}

public class BoardView : MonoBehaviour {
    private TileView CurrentHighlightedTileView;
    public PieceView CurrentPieceView;

    public GameObject TileSpace;

    public TileView[,] TileViews;
    public Board Board;

    public int Width = 10;
    public int Height = 10; // 10 as default so a single tile has a scale of 0.1

    // Start is called before the first frame update
    public void Initialize(TileView[,] tiles, Piece[,] pieces, PlayerPosition firstPlayerPosition) {
        this.TileViews = tiles;
        this.Board = new Board(
            TileView.ToTiles(tiles),
            pieces,
            firstPlayerPosition
        );
        RenderCoords(TileViews);
    }

    private void RenderCoords(TileView[,] tiles) {
        for (int x = 0; x < tiles.GetLength(0); x++) {
            for (int y = 0; y < tiles.GetLength(1); y++) {
                tiles[x, y].name = tiles[x, y].State.ToFigurineAlgebraicNotation(this.Board);
            }
        }
    }

    private TileView TileAtPosition(Tile pos) {
        return TileViews[pos.X, pos.Y];
    }

    [CanBeNull]
    public TileView TileAt(Vector3 mousePosition) {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && hit.collider != null && hit.collider.tag == "Tile") // hovering over a tile
        {
            return hit.collider.gameObject.GetComponent<TileView>();
        }

        return null;
    }


    private bool CanSelect(PieceView pieceView, PlayerView player) {
        return pieceView?.Player == player && (pieceView?.UnblockedMoves().Any() ?? false);
    }

    // TODO highlight tile instead
    public void HighlightSelectedTile(PlayerView player) {
        TileView selected = TileAt(Input.mousePosition);
        HighlightTile(selected, CurrentPieceView, player);
    }

    public void TogglePotentialMoves(PieceView pieceView) {
        if (pieceView == null || !pieceView.PotentialMoves.Any()) {
            return;
        }

        bool alreadyShowing = TileAtPosition(pieceView.PotentialMoves[0].TileTo).PotentialMove;

        // render potential moves
        if (!alreadyShowing) {
            pieceView.PotentialMoves.ForEach(m => {
                TileAtPosition(m.TileTo).BlockedMove = m.BlockedMove;
                TileAtPosition(m.TileTo).PotentialMove = true;
                if (m.TileCaptured != null) {
                    TileAtPosition(m.TileCaptured).Threatened = true;
                }
            });
        } else // de-select all
          {
            pieceView.PotentialMoves.ForEach(m => {
                TileAtPosition(m.TileTo).BlockedMove = false;
                TileAtPosition(m.TileTo).PotentialMove = false;
                if (m.TileCaptured != null) {
                    TileAtPosition(m.TileCaptured).Threatened = false;
                }
            });
        }
    }

    public void SelectPiece(PieceView pieceView, PlayerView player) {
        if (CurrentPieceView != null) {
            TogglePotentialMoves(CurrentPieceView);
            CurrentPieceView.TileView.Selected = false;
        }

        if (pieceView == null) {
            CurrentPieceView = null;
        }

        if (pieceView != null && CanSelect(pieceView, player)) {
            pieceView.TileView.Selected = true;
            CurrentPieceView = pieceView;
            TogglePotentialMoves(pieceView);
        }
    }

    private void HighlightTile(TileView tileView, PieceView currentPieceView, PlayerView player) {
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
        if ((currentPieceView == null && CanSelect(PieceAt(tileView), player)) || // new piece selection
            (currentPieceView != null && currentPieceView.UnblockedMoveTo(tileView, this.TileViews) != null)) // potential move 
        {
            tileView.Highlighted = true;
            CurrentHighlightedTileView = tileView;
        }
    }

    public PieceView PieceAt(TileView tile) {
        if (tile == null) {
            return null;
        }

        return tile.CurrentPiece;
    }

    // Update is called once per frame
    void Update() {
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(
            new Vector3(0, 0, 0),
            this.GetComponent<SpriteRenderer>().bounds.size
        );

        // TODO draw the board wire
        //float tw= width / tilePrefab.transform.localScale.x;
        //float th = height / tilePrefab.transform.localScale.y;
        //Vector3 size = new Vector3(tw, th);

        //for (int x = 0; x < tiles.GetLength(0); x++)
        //{
        //    for (int y = 0; y < tiles.GetLength(1); y++)
        //    {
        //        Vector3 pos = new Vector3(x * tw, y * th);
        //        Gizmos.DrawWireCube(
        //            pos,
        //            size
        //        );
        //        Debug.Log("here");
        //    }
        //}
    }
}