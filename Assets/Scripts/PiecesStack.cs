using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;


public class PiecesStack : MonoBehaviour
{
    public GameObject Container;

    public void Remove(PieceView p)
    {
        p.Unfreeze();
        // TODO remove from stack    
    }
    
    public void Add(PieceView p)
    {
        p.Freeze();

        var newPos = this.Container.transform.position;

        var existing = this.Container.GetComponentsInChildren<PieceView>();
        if (existing != null && existing.Length > 0) // add after last one
        {
            var sprite = existing.Last().Sprite;

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



        p.transform.parent = this.Container.transform;
        p.transform.position = newPos;

    }
}
