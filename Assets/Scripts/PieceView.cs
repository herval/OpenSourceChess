using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;


public class PieceView : MonoBehaviour
{
    public Piece State = new Piece();

    private TileView m_TileView;
    public TileView TileView
    {
        get
        {
            return m_TileView;
        }

        private set
        {
            m_TileView = value;
            if (Sprite == null)
            {
                Sprite = this.GetComponent<SpriteRenderer>();
                if (Sprite == null)
                {
                    Sprite = this.GetComponentInChildren<SpriteRenderer>();
                }
                if (Sprite == null)
                {
                    throw new MissingReferenceException("A sprite is required!");
                }
            }

            // set the order in layer for the sprite
            Sprite.sortingOrder = -value.Y;
            
            State.Tile = value.State;

        }
    }

    public PlayerView Player;
    public SpriteRenderer Sprite;

    public List<Move> PotentialMoves {
        get {
            return State.PotentialMoves;
        }
    }

    public bool IsKing {
        get {
            return State.IsKing;
        }
    }

    public void Freeze()
    {
        setAnimated(false);
    }

    public void Unfreeze()
    {
        setAnimated(true);
    }

    private void setAnimated(bool animated)
    {
        var animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            animator.enabled = animated;
        }
    }


    public Movement UnblockedMoveTo(TileView tileView)
    {
        // TODO fix this Tile comparison
        var play = this.PotentialMoves.Find(m => m.TileTo.X == tileView.State.X && m.TileTo.Y == tileView.State.Y && !m.BlockedMove);
        if (play == null) {
            {
                return null;
            }
        }

        return new Movement(
            play,
            this,
            tileView.CurrentPiece,
            tileView,
            this.TileView);
    }

    public String name => (Player.Color == Color.white ? "white" : "black") + " " + State.Type.ToString();

    public bool MovedAtLeastOnce {
        get {
            return State.MovedAtLeastOnce;
        }
    }

    public PieceType Type {
        get {
            return State.Type;
        }
    }

    public int Value {
        get {
            return State.Value;
        }
    }

    public List<Move> UnblockedMoves()
    {
        return this.PotentialMoves.FindAll(m => !m.BlockedMove);
    }

    public void SetTile(TileView tileView, bool skipAnimation, AfterAnimationCallback done)
    {
        tileView.CurrentPiece = this;
        
        if(this.TileView != null) {
            this.TileView.CurrentPiece = null; // remove ref from tile so piece doesn't show in two places at the same time
        }
        this.TileView = tileView;
        
        this.transform.parent = tileView.transform;
        if (skipAnimation)
        {
            this.transform.position = tileView.transform.position;
            done?.Invoke(true);
        }
        else
        {
            // TODO is this efficient?
            StartCoroutine(
                AnimationHelper.MoveOverSeconds(
                    this.gameObject, 
                    this.TileView.transform.position, 
                    0.2f,
                    done)); 
        }
        
        
    }
}
