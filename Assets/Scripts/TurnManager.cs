using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TurnManager : MonoBehaviour{

    public abstract PieceCommand ActOn(Player player, Board board);
}
