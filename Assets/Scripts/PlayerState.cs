using UnityEngine;

public class PlayerState : Singleton<PlayerState> {
    public TurnManager PlayerOneManager = new TurnManager();
    public PieceArrangement PlayerOneArrangement = new StandardPieceArrangement();

    public TurnManager PlayerTwoManager = new RemoteTurnManager();
    public PieceArrangement PlayerTwoArrangement = new StandardPieceArrangement();


}
