using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode
{
    PlayerVersusComputer,
    PlayerVersusPlayer,
}


public class PlayerPreferences : Singleton<PlayerPreferences>
{
    public GameMode gameMode = GameMode.PlayerVersusComputer;
}
