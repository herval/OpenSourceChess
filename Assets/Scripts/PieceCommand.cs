using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PieceCommand { }


public class LoseGame : PieceCommand
{
    public readonly Player Player;

    public LoseGame(Player p)
    {
        this.Player = p;
    }
}

public class Movement : PieceCommand
{
    public readonly Play Play;

    public Movement(Play play)
    {
        this.Play = play;
    }
}
