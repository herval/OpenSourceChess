using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerPreferences : Singleton<PlayerPreferences>
{
    public TurnManager PlayerOneManager = new TurnManager();
    public TurnManager PlayerTwoManager = new RandomMoves();
}
