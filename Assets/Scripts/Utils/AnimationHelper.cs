using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationHelper
{

    // https://answers.unity.com/questions/296347/move-transform-to-target-in-x-seconds.html
    public static IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
    }

}
