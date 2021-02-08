using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class PiecesStack : MonoBehaviour
{
    public GameObject container;

    public void Remove(Piece p)
    {
        p.Unfreeze();
        // TODO remove from stack    
    }
    
    public void Add(Piece p)
    {
        p.Freeze();

        var newPos = this.container.transform.position;

        var existing = this.container.GetComponentsInChildren<Piece>();
        if (existing != null && existing.Length > 0) // add after last one
        {
            var sprite = existing.Last().sprite;

            // TODO this smells wrong
            // newPos = existing.Last().transform.position + new Vector3(sprite.bounds.size.x / 3, 0);
            //
            // sprite.sortingOrder = existing.Length; // stack'em!

            //if (newPos.x >= container.transform.localScale.x) // add on next line
            //{
            //    Debug.Log("Next line");
            //    newPos = new Vector3(
            //        this.container.transform.position.x,
            //        newPos.y + (sprite.bounds.size.y * 1.1f));
            //}
        }



        p.transform.parent = this.container.transform;
        p.transform.position = newPos;

    }
}
