using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PieceCommand { }


public class LoseGame : PieceCommand
{
    public readonly Player player;

    public LoseGame(Player p)
    {
        this.player = p;
    }
}

public class Movement : PieceCommand
{
    public readonly Play play;

    public Movement(Play play)
    {
        this.play = play;
    }
}