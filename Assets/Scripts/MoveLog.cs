using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MoveLog : MonoBehaviour
{
    public Text Text;
    private List<Play> MoveHistory;

    void Start()
    {
        MoveHistory = new List<Play>();
        Text.text = "";
    }

    public void Push(Play play)
    {
        MoveHistory.Add(play);
    }

    public void Pop()
    {
        if (MoveHistory.Count() > 0)
        {
            MoveHistory.RemoveAt(MoveHistory.Count() - 1);
        }
    }

    public void Update()
    {
        Text.text = String.Join(
                " ",
                MoveHistory.ConvertAll(m => m.ToOfficialNotation())
        );
    }

    public Play Last()
    {
        return MoveHistory.Count() == 0 ? null : MoveHistory.Last();
    }
}
