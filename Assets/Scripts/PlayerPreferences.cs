public class PlayerPreferences : Singleton<PlayerPreferences>
{
    public TurnManager PlayerOneManager = new TurnManager();
    public PieceArrangement PlayerOneArrangement = new StandardPieceArrangement();
    
    public TurnManager PlayerTwoManager = new RandomMoves();
    public PieceArrangement PlayerTwoArrangement = new StandardPieceArrangement();
}
